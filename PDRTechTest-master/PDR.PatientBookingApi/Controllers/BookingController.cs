﻿using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PDR.PatientBooking.Service.BookingService;
using PDR.PatientBooking.Service.BookingService.Requests;
using PDR.PatientBooking.Service.BookingService.Validation;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {

        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public async Task<IActionResult> GetPatientNextAppointment(long patientId, CancellationToken token)
        {
            var bookings = (await _bookingService.GetBookings(new AllBookingsRequest
            {
                PatientIdentificationNumber = patientId,
                ExcludeCancelled = true,
                ExcludePastDue = true
            }, token)).ToArray();

            if (!bookings.Any())
            {
                return NotFound($"No Upcoming Appointments were found for patient ID: {patientId}");
            }

            var nextBooking = bookings.OrderBy(x => x.StartTime).First();

            return Ok(new
            {
                nextBooking.Id,
                nextBooking.DoctorId,
                nextBooking.StartTime,
                nextBooking.EndTime
            });
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<Order> GetBooking(Guid id, CancellationToken token)
        {
            var bookingDetails = await _bookingService.GetBookingById(id, token);

            return bookingDetails;
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddBooking(NewBooking newBooking, CancellationToken token)
        {
            var bookingId = new Guid();

            var bookingAddRequest = new AddBookingRequest()
            {
                Id = bookingId,
                StartTime = newBooking.StartTime,
                EndTime = newBooking.EndTime,
                PatientId = newBooking.PatientId,
                DoctorId = newBooking.DoctorId
            };

            await _bookingService.AddBooking(bookingAddRequest, token);

            return CreatedAtAction(nameof(GetBooking), new {id = bookingId});
        }


        [HttpPost("{bookingId:guid}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid bookingId, CancellationToken token)
        {
            await _bookingService.CancelBooking(bookingId, token);

            return Ok();
        }

        public class NewBooking
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
        }

        //private static MyOrderResult UpdateLatestBooking(List<Order> bookings2, int i)
        //{
        //    MyOrderResult latestBooking;
        //    latestBooking = new MyOrderResult();
        //    latestBooking.Id = bookings2[i].Id;
        //    latestBooking.DoctorId = bookings2[i].DoctorId;
        //    latestBooking.StartTime = bookings2[i].StartTime;
        //    latestBooking.EndTime = bookings2[i].EndTime;
        //    latestBooking.PatientId = bookings2[i].PatientId;
        //    latestBooking.SurgeryType = (int)bookings2[i].GetSurgeryType();

        //    return latestBooking;
        //}

        //private class MyOrderResult
        //{
        //    public Guid Id { get; set; }
        //    public DateTime StartTime { get; set; }
        //    public DateTime EndTime { get; set; }
        //    public long PatientId { get; set; }
        //    public long DoctorId { get; set; }
        //    public int SurgeryType { get; set; }
        //}
    }
}