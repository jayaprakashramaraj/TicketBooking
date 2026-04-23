using Booking.Application.Interfaces;
using Booking.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Booking.Application.Services
{
    public class SeatService : ISeatService
    {
        private readonly IBookingRepository _repository;

        public SeatService(IBookingRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<string>> GetBookedSeatsAsync(Guid showId, CancellationToken cancellationToken = default)
        {
            return await _repository.GetBookedSeatsAsync(showId);
        }
    }
}
