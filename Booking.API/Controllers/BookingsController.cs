using Microsoft.AspNetCore.Mvc;
using Booking.API.Domain;
using Booking.API.DTOs;
using Booking.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using TicketBooking.Common.Events;
using Microsoft.EntityFrameworkCore;

using Booking.API.Services;

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly BookingDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISeatReservationService _reservationService;

        public BookingsController(BookingDbContext context, IPublishEndpoint publishEndpoint, ISeatReservationService reservationService)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _reservationService = reservationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBookingRequest request)
        {
            if (request.SeatNumbers == null || !request.SeatNumbers.Any())
                return BadRequest("No seats selected.");

            var bookingId = Guid.NewGuid();

            // Phase 1: Try to reserve seats in Redis (In-memory, high scale)
            var reserved = await _reservationService.ReserveSeatsAsync(request.ShowId, request.SeatNumbers, bookingId, TimeSpan.FromMinutes(10));

            if (!reserved)
            {
                return Conflict("One or more selected seats are already being booked. Please try different seats.");
            }

            // Phase 2: Publish event for background processing (Asynchronous)
            await _publishEndpoint.Publish(new BookingRequested
            {
                BookingId = bookingId,
                ShowId = request.ShowId,
                ShowName = request.ShowName,
                ShowTime = request.ShowTime,
                CustomerEmail = request.CustomerEmail,
                SeatNumbers = request.SeatNumbers,
                TotalAmount = request.TotalAmount
            });

            // Return 202 Accepted immediately
            return AcceptedAtAction(nameof(GetBooking), new { id = bookingId }, new { BookingId = bookingId, Status = "Processing" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetBookingStatus(Guid id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            
            if (booking == null)
            {
                // If not in DB yet, it might still be in the queue
                return Ok(new { BookingId = id, Status = "Processing" });
            }

            return Ok(new { BookingId = id, Status = booking.Status.ToString() });
        }

        [HttpGet("shows/{showId}/seats")]
        public async Task<IActionResult> GetShowSeats(Guid showId)
        {
            var bookedSeats = await _context.Seats
                .Where(s => s.ShowId == showId && s.IsBooked)
                .Select(s => s.SeatNumber)
                .ToListAsync();

            return Ok(bookedSeats);
        }

        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetByUser(string email)
        {
            var bookings = await _context.Bookings
                .Where(b => b.CustomerEmail == email)
                .OrderByDescending(b => b.ShowTime)
                .ToListAsync();

            return Ok(bookings);
        }
    }
}
