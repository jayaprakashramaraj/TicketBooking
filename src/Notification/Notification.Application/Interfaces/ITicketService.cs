using System;
using System.Threading.Tasks;
using TicketBooking.Common.Events;

namespace Notification.Application.Interfaces
{
    public interface ITicketService
    {
        Task ProcessBookingConfirmedAsync(BookingConfirmed bookingConfirmed);
        Task<byte[]?> GetTicketAsync(Guid bookingId);
    }
}
