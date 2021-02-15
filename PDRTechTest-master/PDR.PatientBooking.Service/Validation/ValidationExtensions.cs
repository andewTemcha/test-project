using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDR.PatientBooking.Service.Validation
{
    public static class ValidationExtensions
    {
        public static string AsString(this List<string> errors)
        {
            return string.Join(Environment.NewLine, errors);
        }
    }
}
