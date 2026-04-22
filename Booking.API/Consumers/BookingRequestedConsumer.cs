using MassTransit;
using Microsoft.Extensions.Logging;
using TicketBooking.Common.Events;
using Booking.Application.Interfaces;

namespace Booking.API.Consumers
{
    public class BookingRequestedConsumer : IConsumer<BookingRequested>
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingRequestedConsumer> _logger;

        public BookingRequestedConsumer(IBookingService bookingService, ILogger<BookingRequestedConsumer> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingRequested> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing booking request {BookingId}.", message.BookingId);

            await _bookingService.PersistBookingAsync(
                message.BookingId,
                message.ShowId,
                message.ShowName,
                message.ShowTime,
                message.CustomerEmail,
                message.SeatNumbers,
                message.TotalAmount);
        }
    }
}
