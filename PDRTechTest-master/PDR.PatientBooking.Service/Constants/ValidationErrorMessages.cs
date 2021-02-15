using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.Constants
{
    public static class ValidationErrorMessages
    {
        public static string ShouldBePopulated(string property) => $"{property} must be populated";

        public static string EntityWithEmailAlreadyExists(string entity) => $"A {entity} with that email address already exists";

        public const string ProvideValidEmail = "Please provide a valid email address";

        public const string ClinicWithThatIdNotExists = "A clinic with that ID could not be found";

    }
}
