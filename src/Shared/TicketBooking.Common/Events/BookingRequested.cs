using System;
using System.Collections.Generic;

namespace TicketBooking.Common.Events
{
    public class BookingRequested
    {
        public Guid BookingId { get; set; }
        public Guid ShowId { get; set; }
        public required string ShowName { get; set; }
        public DateTime ShowTime { get; set; }
        public required string CustomerEmail { get; set; }
        public required List<string> SeatNumbers { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
