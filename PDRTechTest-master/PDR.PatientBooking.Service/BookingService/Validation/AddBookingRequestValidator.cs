using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.Constants;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Internal;
using PDR.PatientBooking.Data.Models;

namespace PDR.PatientBooking.Service.BookingService.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;
        private readonly ISystemClock _systemClock;

        public AddBookingRequestValidator(PatientBookingContext context, ISystemClock systemClock)
        {
            _context = context;
            _systemClock = systemClock;
        }

        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (MissingRequiredFields(request, ref result))
                return result;
            
            if (ValidateRelatedEntities(request, ref result))
                return result;

            if (ValidateBookingDateTimeAvailability(request, ref result))
                return result;

            return result;
        }

        public bool MissingRequiredFields(AddBookingRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            if (request.PatientId == default)
                errors.Add(ValidationErrorMessages.EntityMustBeSet(nameof(request.PatientId)));

            if (request.DoctorId == default)
                errors.Add(ValidationErrorMessages.EntityMustBeSet(nameof(request.DoctorId)));

            if (request.StartTime == default)   
                errors.Add(ValidationErrorMessages.EntityMustBeSet(nameof(request.StartTime)));

            if (request.EndTime == default)
                errors.Add(ValidationErrorMessages.EntityMustBeSet(nameof(request.EndTime)));

            if (errors.Any())
            {
                result.PassedValidation = false;
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }

        private bool ValidateRelatedEntities(AddBookingRequest request, ref PdrValidationResult result)
        {
            var patient = _context.Patient.FirstOrDefault(p => p.Id == request.PatientId);

            if (patient == null)
            {
                result.Errors.Add(ValidationErrorMessages.EntityMustExistInDb(nameof(Patient), request.PatientId.ToString()));
            }

            var doctor = _context.Doctor.FirstOrDefault(p => p.Id == request.DoctorId);

            if (doctor == null)
            {
                result.Errors.Add(ValidationErrorMessages.EntityMustExistInDb(nameof(Doctor), request.DoctorId.ToString()));
            }

            if (result.Errors.Any())
            {
                result.PassedValidation = false;
                return true;
            }

            return false;
        }

        private bool ValidateBookingDateTimeAvailability(AddBookingRequest request, ref PdrValidationResult result)
        {
            var now = _systemClock.UtcNow;

            if (request.EndTime <= request.StartTime)
            {
                result.Errors.Add(ValidationErrorMessages.BookingEndDateShouldBeGreaterThenStartDate);
            }
            if (request.StartTime <= now)
            {
                result.Errors.Add(ValidationErrorMessages.BookingIsNotAllowedAtPast);
            }
            //add custom time to let doctor prepare for an appointment/complete booking
            if (request.StartTime <= now.AddMinutes(DomainConstants.MinutesForDoctorPreparation))
            {
                result.Errors.Add(ValidationErrorMessages.DoctorRequiresAdditionalTime);
            }

            if (result.Errors.Any())
            {
                result.PassedValidation = false;
                return true;
            }

            var patientOverlappingAppointments = _context.Order.Where(o => !o.IsCancelled &&
                                                                        o.PatientId == request.PatientId &&
                                                                        request.StartTime < o.EndTime &&
                                                                        o.StartTime < request.EndTime);

            if (patientOverlappingAppointments.Any())
            {
                result.PassedValidation = false;
                result.Errors.Add(ValidationErrorMessages.IsAlreadyBooked(nameof(Patient)));
                return true;
            }

            var doctorOverLappingAppointments = _context.Order.Where(o => !o.IsCancelled &&
                                                     o.DoctorId == request.DoctorId &&
                                                     request.StartTime < o.EndTime && 
                                                     o.StartTime < request.EndTime);
            if (doctorOverLappingAppointments.Any())
            {
                result.PassedValidation = false;
                result.Errors.Add(ValidationErrorMessages.IsAlreadyBooked(nameof(Doctor)));
                return true;
            }

            return false;
        }

    }
}
