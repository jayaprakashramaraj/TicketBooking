using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Booking.API.Services
{
    public interface ISeatReservationService
    {
        Task<bool> ReserveSeatsAsync(Guid showId, List<string> seatNumbers, Guid bookingId, TimeSpan expiry);
        Task ReleaseSeatsAsync(Guid showId, List<string> seatNumbers);
    }
}
