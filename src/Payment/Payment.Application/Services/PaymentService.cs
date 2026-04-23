using System;
using System.Threading.Tasks;
using MassTransit;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Repositories;
using TicketBooking.Common.Events;

namespace Payment.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public PaymentService(ITransactionRepository transactionRepository, IPublishEndpoint publishEndpoint)
        {
            _transactionRepository = transactionRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task ProcessPaymentAsync(BookingInitiated bookingInitiated)
        {
            Console.WriteLine($"Processing real payment for Booking: {bookingInitiated.BookingId}, Amount: {bookingInitiated.TotalAmount}");

            // Simulate real gateway delay
            await Task.Delay(500);

            var externalId = Guid.NewGuid().ToString();

            // Persist the transaction record
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                BookingId = bookingInitiated.BookingId,
                Amount = bookingInitiated.TotalAmount,
                Status = "Success",
                ExternalTransactionId = externalId,
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();

            // Publish PaymentCompleted event
            await _publishEndpoint.Publish(new PaymentCompleted
            {
                BookingId = bookingInitiated.BookingId,
                TransactionId = externalId,
                PaymentDate = DateTime.UtcNow
            });

            Console.WriteLine($"Payment transaction {transaction.Id} saved and published.");
        }
    }
}
