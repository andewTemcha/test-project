using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PDR.PatientBooking.Service.BookingService;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.BookingService.Responses;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {

        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("patient/{patientId}/next")]
        public async Task<IActionResult> GetPatientNextAppointment(long patientId, CancellationToken token)
        {
            var bookingsResponse = await _bookingService.GetBookings(new AllBookingsRequest
            {
                PatientIdentificationNumber = patientId,
                ExcludeCancelled = true,
                ExcludePastDue = true
            }, token);

            var bookings = bookingsResponse.Bookings.ToArray();

            if (!bookings.Any())
            {
                return NotFound($"No Upcoming Appointments were found for patient ID: {patientId}");
            }

            var nextBooking = bookings.OrderBy(x => x.StartTime).First();

            return Ok(new 
            {
                nextBooking.Id,
                nextBooking.Doctor,
                nextBooking.StartTime,
                nextBooking.EndTime
            });
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<GetAllBookingsResponse.Order> GetBooking(Guid id, CancellationToken token)
        {
            var bookingDetails = await _bookingService.GetBookingById(id, token);

            return bookingDetails;
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddBooking(NewBooking newBooking, CancellationToken token)
        {
            try
            {
                var bookingAddRequest = new AddBookingRequest()
                {
                    StartTime = newBooking.StartTime,
                    EndTime = newBooking.EndTime,
                    PatientId = newBooking.PatientId,
                    DoctorId = newBooking.DoctorId
                };

                await _bookingService.AddBooking(bookingAddRequest, token);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }


        [HttpPost("{bookingId:guid}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid bookingId, CancellationToken token)
        {
            await _bookingService.CancelBooking(bookingId, token);

            return Ok();
        }

        public class NewBooking
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
        }
    }
}