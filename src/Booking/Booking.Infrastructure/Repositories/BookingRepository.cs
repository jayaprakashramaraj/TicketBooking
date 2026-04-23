using Booking.Domain.Entities;
using Booking.Domain.Repositories;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context)
        {
            _context = context;
        }

        public async Task<BookingRecord?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<BookingRecord>> GetByEmailAsync(string email)
        {
            return await _context.Bookings
                .Where(b => b.CustomerEmail == email)
                .OrderByDescending(b => b.ShowTime)
                .ToListAsync();
        }

        public async Task AddAsync(BookingRecord booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public async Task UpdateAsync(BookingRecord booking)
        {
            _context.Bookings.Update(booking);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<string>> GetBookedSeatsAsync(Guid showId)
        {
            return await _context.Seats
                .Where(s => s.ShowId == showId && s.IsBooked)
                .Select(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task AddSeatsAsync(IEnumerable<Seat> seats)
        {
            await _context.Seats.AddRangeAsync(seats);
        }

        public async Task MarkSeatsAsAvailableAsync(Guid bookingId)
        {
            var seats = await _context.Seats.Where(s => s.BookingId == bookingId).ToListAsync();
            foreach (var seat in seats)
            {
                seat.IsBooked = false;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
