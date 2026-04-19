using System;

namespace Booking.API.Domain
{
    public class Seat
    {
        public Guid Id { get; set; }
        public Guid ShowId { get; set; }
        public required string SeatNumber { get; set; }
        public bool IsBooked { get; set; }
        public Guid? BookingId { get; set; }
    }
}
