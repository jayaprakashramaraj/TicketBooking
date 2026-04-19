using MassTransit;
using System;
using System.Threading.Tasks;
using TicketBooking.Common.Events;
using Booking.API.Domain;
using Booking.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Booking.API.Consumers
{
    public class PaymentCompletedConsumer : IConsumer<PaymentCompleted>
    {
        private readonly BookingDbContext _context;

        public PaymentCompletedConsumer(BookingDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<PaymentCompleted> context)
        {
            var message = context.Message;
            Console.WriteLine($"Payment completed for Booking: {message.BookingId}");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == message.BookingId);
            
            if (booking != null)
            {
                booking.Status = BookingStatus.Confirmed;
                await _context.SaveChangesAsync();

                await context.Publish(new BookingConfirmed
                {
                    BookingId = booking.Id,
                    CustomerEmail = booking.CustomerEmail,
                    ShowName = booking.ShowName,
                    ShowTime = booking.ShowTime,
                    SeatNumbers = booking.SeatNumbers,
                    TotalAmount = booking.TotalAmount
                });

                Console.WriteLine($"Booking {booking.Id} confirmed and notification event published.");
            }
            else
            {
                Console.WriteLine($"Booking {message.BookingId} not found!");
            }
        }
    }
}
