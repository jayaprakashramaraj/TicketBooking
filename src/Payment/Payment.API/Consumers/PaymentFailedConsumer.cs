using MassTransit;
using System.Threading.Tasks;
using TicketBooking.Common.Events;
using Payment.Domain.Repositories;
using System;

namespace Payment.API.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailed>
    {
        private readonly ITransactionRepository _repository;

        public PaymentFailedConsumer(ITransactionRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            var transaction = await _repository.GetByBookingIdAsync(context.Message.BookingId);
            if (transaction != null)
            {
                transaction.Status = "Failed";
                transaction.ExternalTransactionId = "FAILED-" + context.Message.Reason;
                await _repository.UpdateAsync(transaction);
                await _repository.SaveChangesAsync();
                Console.WriteLine($"Payment failed for Booking {context.Message.BookingId}: {context.Message.Reason}");
            }
        }
    }
}
