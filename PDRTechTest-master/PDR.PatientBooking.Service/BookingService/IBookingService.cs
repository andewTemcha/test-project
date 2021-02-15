using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.BookingService.Responses;
using Order = PDR.PatientBooking.Data.Models.Order;

namespace PDR.PatientBooking.Service.BookingService
{
    public interface IBookingService
    {
        Task AddBooking(AddBookingRequest request, CancellationToken cancellationToken);

        Task CancelBooking(Guid bookingId, CancellationToken cancellationToken);

        Task<GetAllBookingsResponse> GetBookings(AllBookingsRequest ordersRequest, CancellationToken cancellationToken);

        Task<GetAllBookingsResponse.Order> GetBookingById(Guid id, CancellationToken cancellationToken);
    }
}
