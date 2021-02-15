using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.PatientServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.Constants;

namespace PDR.PatientBooking.Service.PatientServices.Validation
{
    public class AddPatientRequestValidator : IAddPatientRequestValidator
    {
        private readonly PatientBookingContext _context;

        public AddPatientRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(AddPatientRequest request)
        {
            var result = new PdrValidationResult(true);

            if (MissingRequiredFields(request, ref result))
                return result;

            if (PatientAlreadyInDb(request, ref result))
                return result;

            if (ClinicNotFound(request, ref result))
                return result;

            return result;
        }

        private bool MissingRequiredFields(AddPatientRequest request, ref PdrValidationResult result)
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

        //TODO remove ref parameters?
        private bool PatientAlreadyInDb(AddPatientRequest request, ref PdrValidationResult result)
        {
            if (_context.Patient.Any(x => x.Email == request.Email))
            {
                result.PassedValidation = false;
                result.Errors.Add(ValidationErrorMessages.EntityWithEmailAlreadyExists(nameof(Patient)));
                return true;
            }

            return false;
        }

        private bool ClinicNotFound(AddPatientRequest request, ref PdrValidationResult result)
        {
            if (!_context.Clinic.Any(x => x.Id == request.ClinicId))
            {
                result.PassedValidation = false;
                result.Errors.Add(ValidationErrorMessages.ClinicWithThatIdNotExists);
                return true;
            }

            return false;
        }
    }
}
