using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.DoctorServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.Constants;

namespace PDR.PatientBooking.Service.DoctorServices.Validation
{
    public class AddDoctorRequestValidator : IAddDoctorRequestValidator
    {
        private readonly PatientBookingContext _context;

        public AddDoctorRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(AddDoctorRequest request)
        {
            var result = new PdrValidationResult(true);

            if (ValidateGeneralFields(request, ref result))
                return result;

            if (DoctorAlreadyInDb(request, ref result))
                return result;

            return result;
        }

        private bool ValidateGeneralFields(AddDoctorRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(request.FirstName))
                errors.Add(ValidationErrorMessages.ShouldBePopulated(nameof(request.FirstName)));

            if (string.IsNullOrEmpty(request.LastName))
                errors.Add(ValidationErrorMessages.ShouldBePopulated(nameof(request.LastName)));

            if (string.IsNullOrEmpty(request.Email))
            {
                errors.Add(ValidationErrorMessages.ShouldBePopulated(nameof(request.Email)));
            }
            else if (!Regex.IsMatch(request.Email, RegexValidation.EmailValidationRegexpPattern))
            {
                errors.Add(ValidationErrorMessages.ProvideValidEmail);
            }

            if (errors.Any())
            {
                result.PassedValidation = false;
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }

        private bool DoctorAlreadyInDb(AddDoctorRequest request, ref PdrValidationResult result)
        {
            if (_context.Doctor.Any(x => x.Email == request.Email))
            {
                result.PassedValidation = false;
                result.Errors.Add(ValidationErrorMessages.EntityWithEmailAlreadyExists(nameof(Doctor)));
                return true;
            }

            return false;
        }
    }
}
