using MassTransit;
using System.Threading.Tasks;
using TicketBooking.Common.Events;
using Notification.Application.Interfaces;

namespace Notification.API.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
    {
        private readonly ITicketService _ticketService;

        public BookingConfirmedConsumer(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public async Task Consume(ConsumeContext<BookingConfirmed> context)
        {
            await _ticketService.ProcessBookingConfirmedAsync(context.Message);
        }
    }
}
