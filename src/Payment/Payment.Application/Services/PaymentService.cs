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
            Console.WriteLine($"Recording PENDING payment for Booking: {bookingInitiated.BookingId}, Amount: {bookingInitiated.TotalAmount}");

            // Persist the transaction record as Pending
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                BookingId = bookingInitiated.BookingId,
                Amount = bookingInitiated.TotalAmount,
                Status = "Pending",
                ExternalTransactionId = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();

            Console.WriteLine($"Payment transaction {transaction.Id} recorded as PENDING. Waiting for simulator...");
        }
    }
}
