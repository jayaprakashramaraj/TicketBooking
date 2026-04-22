using MassTransit;
using Microsoft.Extensions.Logging;
using TicketBooking.Common.Events;
using Booking.Application.Interfaces;

namespace Booking.API.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailed>
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<PaymentFailedConsumer> _logger;

        public PaymentFailedConsumer(IBookingService bookingService, ILogger<PaymentFailedConsumer> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            var message = context.Message;
            _logger.LogWarning("Processing payment failure for booking {BookingId}.", message.BookingId);

            await _bookingService.CancelBookingAsync(message.BookingId);
        }
    }
}
