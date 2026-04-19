using System;
using System.Collections.Generic;

namespace Booking.API.Domain
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

    public class BookingRecord
    {
        public Guid Id { get; set; }
        public Guid ShowId { get; set; }
        public required string ShowName { get; set; }
        public DateTime ShowTime { get; set; }
        public required string CustomerEmail { get; set; }
        public required List<string> SeatNumbers { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
