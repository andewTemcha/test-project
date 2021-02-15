using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.Constants
{
    public static class ValidationErrorMessages
    {
        public static string ShouldBePopulated(string property) => $"{property} must be populated";

        public static string EntityWithEmailAlreadyExists(string entity) => $"A {entity} with that email address already exists";

        public static string EntityMustBeSet(string entity) => $"A {entity} must be set";
        public static string EntityMustExistInDb(string entity, string id) => $"A {entity} with ID = { id } does not exist";

        public static string IsAlreadyBooked(string person) => $"The {person} already has an appointment for this time interval";

        public static string DoctorRequiresAdditionalTime = $"Cannot create booking for the next {DomainConstants.MinutesForDoctorPreparation} minutes. Please choose another time";

        public const string ProvideValidEmail = "Please provide a valid email address";

        public const string ClinicWithThatIdNotExists = "A clinic with that ID could not be found";

        public const string BookingIsNotAllowedAtPast = "Cannot create booking in the past. Please choose appropriate start/end time";

        public const string BookingEndDateShouldBeGreaterThenStartDate = "Booking End date should be greated then Start date";
    }
}
