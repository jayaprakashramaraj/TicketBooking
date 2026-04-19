using Microsoft.EntityFrameworkCore;
using Booking.API.Domain;

namespace Booking.API.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<BookingRecord> Bookings { get; set; }
        public DbSet<Seat> Seats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Seat>()
                .HasIndex(s => new { s.ShowId, s.SeatNumber })
                .IsUnique();

            modelBuilder.Entity<BookingRecord>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
