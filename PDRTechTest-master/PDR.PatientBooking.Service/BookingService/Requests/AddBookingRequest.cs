using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.BookingService.Requests
{
    public class AddBookingRequest
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public virtual long PatientId { get; set; }
        public virtual long DoctorId { get; set; }
    }
}
