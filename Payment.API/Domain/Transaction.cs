using System;

namespace Payment.API.Domain
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string ExternalTransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
