using MassTransit;
using System.Threading.Tasks;
using TicketBooking.Common.Events;
using Payment.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace Payment.API.Consumers
{
    public class PaymentCompletedConsumer : IConsumer<PaymentCompleted>
    {
        private readonly ITransactionRepository _repository;

        public PaymentCompletedConsumer(ITransactionRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<PaymentCompleted> context)
        {
            var transaction = await _repository.GetByBookingIdAsync(context.Message.BookingId);
            if (transaction != null)
            {
                transaction.Status = "Success";
                transaction.ExternalTransactionId = context.Message.TransactionId;
                await _repository.UpdateAsync(transaction);
                await _repository.SaveChangesAsync();
                Console.WriteLine($"Payment confirmed for Booking {context.Message.BookingId}");
            }
        }
    }
}
