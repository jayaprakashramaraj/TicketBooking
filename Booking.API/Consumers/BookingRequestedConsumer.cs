using Booking.API.Data;
using Booking.API.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TicketBooking.Common.Events;

using Booking.API.Services;

namespace Booking.API.Consumers
{
    public class BookingRequestedConsumer : IConsumer<BookingRequested>
    {
        private readonly BookingDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISeatReservationService _reservationService;
        private readonly ILogger<BookingRequestedConsumer> _logger;

        public BookingRequestedConsumer(BookingDbContext context, 
            IPublishEndpoint publishEndpoint, 
            ISeatReservationService reservationService,
            ILogger<BookingRequestedConsumer> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _reservationService = reservationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingRequested> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing booking request: {BookingId} for {CustomerEmail}", message.BookingId, message.CustomerEmail);

            try
            {
                // Create the booking record
                var booking = new BookingRecord
                {
                    Id = message.BookingId,
                    ShowId = message.ShowId,
                    ShowName = message.ShowName,
                    ShowTime = message.ShowTime,
                    CustomerEmail = message.CustomerEmail,
                    SeatNumbers = message.SeatNumbers,
                    TotalAmount = message.TotalAmount,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);

                // Reserve the seats in the DB
                foreach (var seatNum in message.SeatNumbers)
                {
                    _context.Seats.Add(new Seat
                    {
                        Id = Guid.NewGuid(),
                        ShowId = message.ShowId,
                        SeatNumber = seatNum,
                        IsBooked = true,
                        BookingId = message.BookingId
                    });
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully persisted booking {BookingId} to database.", message.BookingId);

                // Now publish the event to trigger payment
                await _publishEndpoint.Publish(new BookingInitiated
                {
                    BookingId = booking.Id,
                    CustomerEmail = booking.CustomerEmail,
                    TotalAmount = booking.TotalAmount,
                    SeatNumbers = booking.SeatNumbers,
                    ShowName = booking.ShowName,
                    ShowTime = booking.ShowTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error persisting booking {BookingId} to database. Triggering compensating transaction.", message.BookingId);
                
                // Compensating Transaction: Release the seats in Redis so others can book them
                try 
                {
                    await _reservationService.ReleaseSeatsAsync(message.ShowId, message.SeatNumbers);
                    _logger.LogInformation("Compensating transaction successful: Redis seats released for {BookingId}", message.BookingId);
                }
                catch(Exception redisEx)
                {
                    _logger.LogCritical(redisEx, "Failed to release Redis seats during compensating transaction for {BookingId}!", message.BookingId);
                }

                throw; // Re-throw to let MassTransit handle retries/DLQ
            }
        }
    }
}
