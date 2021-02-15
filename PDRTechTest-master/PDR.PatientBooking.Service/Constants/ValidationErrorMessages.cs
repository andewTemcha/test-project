using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.Constants
{
    public static class ValidationErrorMessages
    {
        public static string ShouldBePopulated(string property) => $"{property} must be populated";

        public const string DoctorAlreadyInDb = "A doctor with that email address already exists";

        public const string ProvideValidEmail = "Please provide a valid email address";

    }
}
