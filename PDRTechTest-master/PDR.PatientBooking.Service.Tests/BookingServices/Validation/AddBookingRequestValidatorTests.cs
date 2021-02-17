using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.BookingService.Validation;
using PDR.PatientBooking.Service.Constants;


namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class AddBookingRequestValidatorTests
    {
        private IFixture _fixture;

        private PatientBookingContext _context;

        private Mock<ISystemClock> _clockMock;


        private AddBookingRequestValidator _addBookingRequestValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            _clockMock = CreateSystemClockMock(DateTime.UtcNow);

            // Sut instantiation
            _addBookingRequestValidator = new AddBookingRequestValidator(
                _context,
                _clockMock.Object
            );

            FillBaseData();
        }

        [Test]
        public void ValidateRequest_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            res.PassedValidation.Should().BeTrue();
        }

        [Test]
        public void ValidateRequest_DoctorIdNullOrEmpty_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.DoctorId = 0;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().BeEquivalentTo(ValidationErrorMessages.EntityMustBeSet(nameof(request.DoctorId)));
        }

        [Test]
        public void ValidateRequest_DoctorIdNotInDb_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.DoctorId = 111;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().BeEquivalentTo(ValidationErrorMessages.EntityMustExistInDb(nameof(Doctor), request.DoctorId.ToString()));
        }

        [Test]
        public void ValidateRequest_EndDateLessThenStart_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.StartTime = _clockMock.Object.UtcNow.AddHours(12).UtcDateTime;
            request.EndTime = _clockMock.Object.UtcNow.AddHours(10).UtcDateTime;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain(ValidationErrorMessages.BookingEndDateShouldBeGreaterThenStartDate);
        }

        [Test]
        public void ValidateRequest_BookingInThePast_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.StartTime = _clockMock.Object.UtcNow.AddHours(-10).UtcDateTime;
            request.EndTime = _clockMock.Object.UtcNow.AddHours(-5).UtcDateTime;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain(ValidationErrorMessages.BookingIsNotAllowedAtPast);
        }

        [Test]
        public void ValidateRequest_DoctorPreparationTimeLessThenDefaultValue_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();
            request.StartTime = _clockMock.Object.UtcNow.AddMinutes(3).UtcDateTime;
            request.EndTime = _clockMock.Object.UtcNow.AddMinutes(60).UtcDateTime;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain(ValidationErrorMessages.DoctorRequiresAdditionalTime);
        }

        [Test]
        public void ValidateRequest_PatientAlreadyHaveBooking_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            var now = _clockMock.Object.UtcNow.UtcDateTime;
            var order = _fixture.Create<Order>();
            order.DoctorId = request.DoctorId;
            order.Doctor = null;
            order.PatientId = request.PatientId;
            order.Patient = null;
            order.StartTime = now.AddMinutes(30);
            order.EndTime = now.AddHours(2);
            order.IsCancelled = false;

            _context.Order.Add(order);
            _context.SaveChanges();

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain(ValidationErrorMessages.IsAlreadyBooked(nameof(Patient)));
        }

        [Test]
        public void ValidateRequest_DoctorAlreadyHasBooking_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            var now = _clockMock.Object.UtcNow.UtcDateTime;
            var order = _fixture.Create<Order>();
            order.DoctorId = request.DoctorId;
            order.Doctor = null;
            order.IsCancelled = false;
            order.StartTime = now.AddMinutes(30);
            order.EndTime = now.AddHours(2);

            _context.Order.Add(order);
            _context.SaveChanges();

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain(ValidationErrorMessages.IsAlreadyBooked(nameof(Doctor)));
        }

        private AddBookingRequest GetValidRequest()
        {
            var request = _fixture.Build<AddBookingRequest>()
                .With(x => x.DoctorId, 1)
                .With(x => x.PatientId, 1)
                .With(x => x.StartTime, _clockMock.Object.UtcNow.AddHours(1).UtcDateTime)
                .With(x => x.EndTime, _clockMock.Object.UtcNow.AddHours(2).UtcDateTime)
                .Create();

            return request;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        private static Mock<ISystemClock> CreateSystemClockMock(DateTime utcNow)
        {
            var systemClockMock = new Mock<ISystemClock>();

            var utcNowZeroTimezone = new DateTimeOffset(utcNow, offset: TimeSpan.Zero);

            systemClockMock.SetupGet(clock => clock.UtcNow).Returns(utcNowZeroTimezone);

            return systemClockMock;
        }

        private void FillBaseData()
        {
            var doctor = _fixture.Create<Doctor>();
            doctor.Id = 1;
            doctor.Orders = null;
            _context.Doctor.Add(doctor);

            var patient = _fixture.Create<Patient>();
            patient.Id = 1;
            patient.Orders = null;
            _context.Patient.Add(patient);

            _context.SaveChanges();
        }
    }
}
