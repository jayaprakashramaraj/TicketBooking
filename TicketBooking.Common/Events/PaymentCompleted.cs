using System;

namespace TicketBooking.Common.Events
{
    public record PaymentCompleted
    {
        public Guid BookingId { get; init; }
        public string TransactionId { get; init; }
        public DateTime PaymentDate { get; init; }
    }
}
