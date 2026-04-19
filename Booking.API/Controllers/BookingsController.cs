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

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly BookingDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public BookingsController(BookingDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBookingRequest request)
        {
            if (request.SeatNumbers == null || !request.SeatNumbers.Any())
                return BadRequest("No seats selected.");

            // Start a database transaction to ensure atomicity and prevent double booking
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if any of the requested seats are already booked for this show
                var alreadyBooked = await _context.Seats
                    .Where(s => s.ShowId == request.ShowId && request.SeatNumbers.Contains(s.SeatNumber) && s.IsBooked)
                    .ToListAsync();

                if (alreadyBooked.Any())
                {
                    return BadRequest($"Seats {string.Join(", ", alreadyBooked.Select(s => s.SeatNumber))} are already booked.");
                }

                var bookingId = Guid.NewGuid();

                // Reserve the seats
                foreach (var seatNum in request.SeatNumbers)
                {
                    _context.Seats.Add(new Seat
                    {
                        Id = Guid.NewGuid(),
                        ShowId = request.ShowId,
                        SeatNumber = seatNum,
                        IsBooked = true,
                        BookingId = bookingId
                    });
                }

                // Create the booking record
                var booking = new BookingRecord
                {
                    Id = bookingId,
                    ShowId = request.ShowId,
                    ShowName = request.ShowName,
                    ShowTime = request.ShowTime,
                    CustomerEmail = request.CustomerEmail,
                    SeatNumbers = request.SeatNumbers,
                    TotalAmount = request.TotalAmount,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);

                // Save changes within the transaction
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                // Publish event to the message broker
                await _publishEndpoint.Publish(new BookingInitiated
                {
                    BookingId = booking.Id,
                    CustomerEmail = booking.CustomerEmail,
                    TotalAmount = booking.TotalAmount,
                    SeatNumbers = booking.SeatNumbers,
                    ShowName = booking.ShowName,
                    ShowTime = booking.ShowTime
                });

                return Ok(new { BookingId = bookingId, Status = "Pending" });
            }
            catch (Exception ex)
            {
                // Rollback if something goes wrong
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while processing your booking.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();
            return Ok(booking);
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
    }
}
