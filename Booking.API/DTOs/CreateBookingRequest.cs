using System;
using System.Collections.Generic;

namespace Booking.API.DTOs
{
    public record CreateBookingRequest
    {
        public Guid ShowId { get; init; }
        public string ShowName { get; init; }
        public DateTime ShowTime { get; init; }
        public string CustomerEmail { get; init; }
        public List<string> SeatNumbers { get; init; }
        public decimal TotalAmount { get; init; }
    }
}
