using System;
using System.Collections.Generic;
using System.Text;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.BookingService.Validation
{
    public interface IBookingRequestValidator
    {
        PdrValidationResult ValidateRequest(AddBookingRequest request);
    }
}
