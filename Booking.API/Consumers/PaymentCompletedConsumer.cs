using MassTransit;
using TicketBooking.Common.Events;
using Booking.Application.Interfaces;

namespace Booking.API.Consumers
{
    public class PaymentCompletedConsumer : IConsumer<PaymentCompleted>
    {
        private readonly IBookingService _bookingService;

        public PaymentCompletedConsumer(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task Consume(ConsumeContext<PaymentCompleted> context)
        {
            var message = context.Message;
            Console.WriteLine($"Processing payment completion for Booking: {message.BookingId}.");

            await _bookingService.ConfirmBookingAsync(message.BookingId);
        }
    }
}
