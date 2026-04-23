using MassTransit;
using TicketBooking.Common.Events;
using Booking.Application.Interfaces;
using System.Threading.Tasks;
using System;

namespace Booking.API.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailed>
    {
        private readonly IBookingService _bookingService;

        public PaymentFailedConsumer(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            var message = context.Message;
            Console.WriteLine($"Processing payment failure for Booking: {message.BookingId}. Reason: {message.Reason}");

            await _bookingService.CancelBookingAsync(message.BookingId);
        }
    }
}
