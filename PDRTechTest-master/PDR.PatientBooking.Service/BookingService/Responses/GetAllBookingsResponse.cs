using System;
using System.Collections.Generic;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.DoctorServices.Responses;
using PDR.PatientBooking.Service.PatientServices.Responses;

namespace PDR.PatientBooking.Service.BookingService.Responses
{
    public class GetAllBookingsResponse
    {
        public IEnumerable<Order> Bookings { get; set; }

        public class Order
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public SurgeryType SurgeryType { get; set; }
            public bool IsCancelled { get; set; }
            public virtual GetAllPatientsResponse.Patient Patient { get; set; }
            public virtual GetAllDoctorsResponse.Doctor Doctor { get; set; }
        }
    }
}
