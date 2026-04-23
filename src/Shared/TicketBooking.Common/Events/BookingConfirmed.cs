using System;
using System.Collections.Generic;

namespace TicketBooking.Common.Events
{
    public record BookingConfirmed
    {
        public Guid BookingId { get; init; }
        public string CustomerEmail { get; init; }
        public string ShowName { get; init; }
        public DateTime ShowTime { get; init; }
        public List<string> SeatNumbers { get; init; }
        public decimal TotalAmount { get; init; }
    }
}
