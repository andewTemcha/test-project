using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.BookingService.Validation;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;

        private readonly IBookingRequestValidator _validator;

        public BookingService(PatientBookingContext context, IBookingRequestValidator validator)
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
                    .Include(p=>p.Clinic)
                    .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            _context.Order.Add(new Order
            {
                Id = request.Id,
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

        public async Task<IEnumerable<Order>> GetBookings(AllBookingsRequest ordersRequest, CancellationToken cancellationToken)
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

            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Order> GetBookingById(Guid id, CancellationToken cancellationToken)
        {
            return await _context
                .Order
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }
    }
}
