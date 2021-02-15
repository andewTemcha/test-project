using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PDR.PatientBooking.Service.BookingService.Requests
{
    public class AllBookingsRequest
    {
        public long PatientIdentificationNumber { get; set; }

        public bool ExcludeCancelled { get; set; }

        public bool ExcludePastDue { get; set; }
    }
}
