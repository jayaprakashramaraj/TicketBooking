using MassTransit;
using Microsoft.Extensions.Logging;
using TicketBooking.Common.Events;
using Booking.Application.Interfaces;
using Booking.Domain.Services;

namespace Booking.API.Consumers
{
    public class BookingRequestedConsumer : IConsumer<BookingRequested>
    {
        private readonly IBookingService _bookingService;
        private readonly ISeatReservationService _reservationService;
        private readonly ILogger<BookingRequestedConsumer> _logger;

        public BookingRequestedConsumer(IBookingService bookingService, ISeatReservationService reservationService, ILogger<BookingRequestedConsumer> logger)
        {
            _bookingService = bookingService;
            _reservationService = reservationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingRequested> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing booking request {BookingId}.", message.BookingId);

            try
            {
                await _bookingService.PersistBookingAsync(
                    message.BookingId,
                    message.ShowId,
                    message.ShowName,
                    message.ShowTime,
                    message.CustomerEmail,
                    message.SeatNumbers,
                    message.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist booking {BookingId}. Releasing seats in Redis.", message.BookingId);
                
                // If the booking cannot be saved to the database (e.g., unique constraint violation,
                // database down), we MUST release the seats in Redis immediately. Otherwise, the
                // seats would remain locked for the TTL duration (e.g., 10 minutes) despite 
                // being available in the database, preventing other users from booking them.
                await _reservationService.ReleaseSeatsAsync(message.ShowId, message.SeatNumbers);
                
                throw; // Re-throw to allow MassTransit retry/dead-lettering
            }
        }
    }
}
