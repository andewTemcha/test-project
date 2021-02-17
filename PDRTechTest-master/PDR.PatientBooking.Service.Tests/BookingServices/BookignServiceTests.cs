using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.BookingService.Validation;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookignServiceTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private PatientBookingContext _context;
        private Mock<IAddBookingRequestValidator> _validator;

        private BookingService.BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            //Prevent fixture from generating circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _validator = _mockRepository.Create<IAddBookingRequestValidator>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _bookingService = new BookingService.BookingService(
                _context,
                _validator.Object
            );
        }

        private void SetupMockDefaults()
        {
            _validator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>()))
                .Returns(new PdrValidationResult(true));
        }

        [Test]
        public async Task AddBooking_ValidatesRequestTriggered()
        {
            //arrange
            var patient = _fixture.Create<Patient>();

            _context.Patient.Add(patient);

            _context.SaveChanges();

            var request = _fixture.Create<AddBookingRequest>();
            request.PatientId = patient.Id;

            //act
            await _bookingService.AddBooking(request, CancellationToken.None);

            //assert
            _validator.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public async Task AddBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _validator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _bookingService.AddBooking(_fixture.Create<AddBookingRequest>(), CancellationToken.None));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }

        [Test]
        public async Task AddBooking_AddsBookingToContextWithGeneratedId()
        {
            //arrange
            var patient = _fixture.Create<Patient>();
            patient.Id = 100;
            patient.Clinic.SurgeryType = SurgeryType.SystemOne;
            _context.Patient.Add(patient);
            _context.SaveChanges();

            var start = DateTime.UtcNow.AddHours(1);

            var request = _fixture.Create<AddBookingRequest>();
            request.DoctorId = 1;
            request.PatientId = 100;
            request.StartTime = start;
            request.EndTime = start.AddHours(1);
            
            var expected = new Order
            {
                DoctorId = 1,
                PatientId = 100,
                StartTime = start,
                EndTime = start.AddHours(1),
                SurgeryType = (int)SurgeryType.SystemOne
            };

            //act
            await _bookingService.AddBooking(request, CancellationToken.None);

            //assert
            _context.Order.Should().ContainEquivalentOf(expected, options => options
                .Excluding(order => order.Id)
                .Excluding(order => order.Doctor)
                .Excluding(order => order.Patient));
        }

        [Test]
        public async Task CancelledBooking_IsNotReturned_WhenCancelled()
        {
            var order = _fixture.Create<Order>();
            order.IsCancelled = false;
            _context.Order.Add(order);
            _context.SaveChanges();

            //act
            await _bookingService.CancelBooking(order.Id, CancellationToken.None);

            var res = await _bookingService.GetBookings(new AllBookingsRequest
            {
                ExcludeCancelled = true
            }, CancellationToken.None);

            //assert
            res.Bookings.Should().BeEmpty();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
