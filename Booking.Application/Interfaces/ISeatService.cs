using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Booking.Application.Interfaces
{
    public interface ISeatService
    {
        Task<IEnumerable<string>> GetBookedSeatsAsync(Guid showId, CancellationToken cancellationToken = default);
    }
}
