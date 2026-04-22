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
        Task MarkSeatsAsAvailableAsync(Guid bookingId);
        Task SaveChangesAsync();
    }
}
