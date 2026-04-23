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

        public async Task UpsertSeatsAsync(Guid showId, IEnumerable<string> seatNumbers, Guid bookingId)
        {
            // Find existing seat records for this show and specific seat numbers.
            // These might exist from previously cancelled bookings.
            var existingSeats = await _context.Seats
                .Where(s => s.ShowId == showId && seatNumbers.Contains(s.SeatNumber))
                .ToListAsync();

            var existingSeatNumbers = existingSeats.Select(s => s.SeatNumber).ToHashSet();

            // Update existing records instead of inserting new ones to avoid unique index violations.
            foreach (var seat in existingSeats)
            {
                seat.IsBooked = true;
                seat.BookingId = bookingId;
            }

            // Create new records only for seats that don't already have an entry in the DB.
            var newSeats = seatNumbers
                .Where(sn => !existingSeatNumbers.Contains(sn))
                .Select(sn => new Seat
                {
                    Id = Guid.NewGuid(),
                    ShowId = showId,
                    SeatNumber = sn,
                    IsBooked = true,
                    BookingId = bookingId
                });

            if (newSeats.Any())
            {
                await _context.Seats.AddRangeAsync(newSeats);
            }
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
