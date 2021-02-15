using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingService.Requests;

namespace PDR.PatientBooking.Service.BookingService
{
    public interface IBookingService
    {
        Task AddBooking(AddBookingRequest request, CancellationToken cancellationToken);

        Task CancelBooking(Guid bookingId, CancellationToken cancellationToken);

        Task<IEnumerable<Order>> GetBookings(AllBookingsRequest ordersRequest, CancellationToken cancellationToken);

        Task<Order> GetBookingById(Guid id, CancellationToken cancellationToken);
    }
}
