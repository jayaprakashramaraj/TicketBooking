using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Booking.Domain.Entities;

namespace Booking.Domain.Repositories
{
    public interface IBookingRepository
    {
        Task<BookingRecord?> GetByIdAsync(Guid id);
        Task<IEnumerable<BookingRecord>> GetByEmailAsync(string email);
        Task AddAsync(BookingRecord booking);
        Task UpdateAsync(BookingRecord booking);
        Task<IEnumerable<string>> GetBookedSeatsAsync(Guid showId);
        Task AddSeatsAsync(IEnumerable<Seat> seats);
        /// <summary>
        /// Updates existing seat records or inserts new ones if they don't exist.
        /// Prevents unique constraint violations on (ShowId, SeatNumber).
        /// </summary>
        Task UpsertSeatsAsync(Guid showId, IEnumerable<string> seatNumbers, Guid bookingId);
        Task MarkSeatsAsAvailableAsync(Guid bookingId);
        Task SaveChangesAsync();
    }
}
