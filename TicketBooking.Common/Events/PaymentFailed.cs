using System;

namespace TicketBooking.Common.Events
{
    public record PaymentFailed
    {
        public Guid BookingId { get; init; }
        public string Reason { get; init; }
    }
}
