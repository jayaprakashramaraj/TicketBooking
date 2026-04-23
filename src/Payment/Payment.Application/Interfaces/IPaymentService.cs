using System.Threading.Tasks;
using TicketBooking.Common.Events;

namespace Payment.Application.Interfaces
{
    public interface IPaymentService
    {
        Task ProcessPaymentAsync(BookingInitiated bookingInitiated);
    }
}
