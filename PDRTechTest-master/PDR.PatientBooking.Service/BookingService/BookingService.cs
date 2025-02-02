﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.BookingService.Responses;
using PDR.PatientBooking.Service.BookingService.Validation;
using PDR.PatientBooking.Service.DoctorServices.Responses;
using PDR.PatientBooking.Service.Enums;
using PDR.PatientBooking.Service.PatientServices.Responses;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;

        private readonly IAddBookingRequestValidator _validator;

        public BookingService(PatientBookingContext context, IAddBookingRequestValidator validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task AddBooking(AddBookingRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.AsString());
            }

            var patient = await _context
                    .Patient
                    .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            _context.Order.Add(new Order
            {
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsCancelled = false,
                SurgeryType = (int)patient.Clinic.SurgeryType
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        //TODO add validation to forbid cancelling orders within specific time-range or in the past (e.g. starts in 5/10 minutes from now)
        public async Task CancelBooking(Guid bookingId, CancellationToken cancellationToken)
        {
            var booking = await _context.Order.FirstOrDefaultAsync(o => o.Id == bookingId, cancellationToken);

            if (booking != null)
            {
                booking.IsCancelled = true;

                await _context.SaveChangesAsync(cancellationToken);
            }
            
        }

        public async Task<GetAllBookingsResponse> GetBookings(AllBookingsRequest ordersRequest, CancellationToken cancellationToken)
        {
            var query = _context.Order.AsQueryable();

            if (ordersRequest.PatientIdentificationNumber != default)
            {
                query = query.Where(x => x.PatientId == ordersRequest.PatientIdentificationNumber);
            }

            if (ordersRequest.ExcludeCancelled)
            {
                query = query.Where(x => !x.IsCancelled);
            }

            if (ordersRequest.ExcludePastDue)
            {
                query = query.Where(x => x.StartTime > DateTime.UtcNow);
            }

            var bookings = await query
                .Include(o => o.Doctor)
                .Include(o => o.Patient)
                .Include(o => o.Patient.Clinic)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new GetAllBookingsResponse
            {
                Bookings = bookings.Select(MapFrom).ToList()
            };
        }

        public async Task<GetAllBookingsResponse.Order> GetBookingById(Guid id, CancellationToken cancellationToken)
        {
            var booking = await _context
                .Order
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

            if (booking == null)
            {
                return null;
            }

            return MapFrom(booking);
        }

        //TODO add auto-mapper instead
        private static GetAllBookingsResponse.Order MapFrom(Order booking)
        {
            return new GetAllBookingsResponse.Order
            {
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                IsCancelled = booking.IsCancelled,
                Id = booking.Id,
                SurgeryType = booking.Patient.Clinic.SurgeryType,
                Patient = booking.Patient != null ? new GetAllPatientsResponse.Patient
                {
                    Id = booking.Patient.Id,
                    FirstName = booking.Patient.FirstName,
                    LastName = booking.Patient.LastName,
                    Email = booking.Patient.Email,
                    DateOfBirth = booking.Patient.DateOfBirth,
                    Gender = (Gender)booking.Patient.Gender,
                    Clinic = new GetAllPatientsResponse.Clinic
                    {
                        Id = booking.Patient.ClinicId,
                        Name = booking.Patient.Clinic.Name
                    }
                } : null,
                Doctor = new GetAllDoctorsResponse.Doctor
                {
                    Id = booking.Doctor.Id,
                    FirstName = booking.Doctor.FirstName,
                    LastName = booking.Doctor.LastName,
                    Email = booking.Doctor.Email,
                    DateOfBirth = booking.Doctor.DateOfBirth,
                    Gender = (Gender)booking.Doctor.Gender
                }
            };
        }
    }
}
