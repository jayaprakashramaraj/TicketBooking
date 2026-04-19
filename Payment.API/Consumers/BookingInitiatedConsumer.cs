using MassTransit;
using System;
using System.Threading.Tasks;
using TicketBooking.Common.Events;
using Payment.API.Data;
using Payment.API.Domain;

namespace Payment.API.Consumers
{
    public class BookingInitiatedConsumer : IConsumer<BookingInitiated>
    {
        private readonly PaymentDbContext _context;

        public BookingInitiatedConsumer(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<BookingInitiated> context)
        {
            var message = context.Message;
            Console.WriteLine($"Processing real payment for Booking: {message.BookingId}, Amount: {message.TotalAmount}");

            // Simulate real gateway delay
            await Task.Delay(500);

            var externalId = Guid.NewGuid().ToString();

            // Persist the transaction record
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                BookingId = message.BookingId,
                Amount = message.TotalAmount,
                Status = "Success",
                ExternalTransactionId = externalId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Publish PaymentCompleted event
            await context.Publish(new PaymentCompleted
            {
                BookingId = message.BookingId,
                TransactionId = externalId,
                PaymentDate = DateTime.UtcNow
            });

            Console.WriteLine($"Payment transaction {transaction.Id} saved and published.");
        }
    }
}
