using MassTransit;
using System.Threading.Tasks;
using TicketBooking.Common.Events;
using Payment.Application.Interfaces;

namespace Payment.API.Consumers
{
    public class BookingInitiatedConsumer : IConsumer<BookingInitiated>
    {
        private readonly IPaymentService _paymentService;

        public BookingInitiatedConsumer(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task Consume(ConsumeContext<BookingInitiated> context)
        {
            await _paymentService.ProcessPaymentAsync(context.Message);
        }
    }
}
