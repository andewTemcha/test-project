using System;

namespace PDR.PatientBooking.Data.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        //TODO Do we really need to have SurgeryType for Booking entity? If we need to have it for frond end, we should get it from DB separately ad-hoc.
        //TODO Thus we wont need to get related entity while creating new booking (booking->patient->clinic)
        public int SurgeryType { get; set; }
        public bool IsCancelled { get; set; }
        public virtual long PatientId { get; set; }
        public virtual long DoctorId { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
    }
}
