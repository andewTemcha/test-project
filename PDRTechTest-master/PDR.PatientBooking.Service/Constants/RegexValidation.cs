using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.Constants
{
    public static class RegexValidation
    {
        public const string EmailValidationRegexpPattern = @"[\w\.\+-]*[a-zA-Z0-9_]@[\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]";
    }
}
