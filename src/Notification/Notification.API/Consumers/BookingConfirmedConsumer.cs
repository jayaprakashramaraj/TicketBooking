using MassTransit;
using System;
using System.Threading.Tasks;
using TicketBooking.Common.Events;
using Notification.API.Services;
using StackExchange.Redis;

namespace Notification.API.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
    {
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IEmailService _emailService;
        private readonly IDatabase _redis;

        public BookingConfirmedConsumer(IPdfGenerator pdfGenerator, IEmailService emailService, IConnectionMultiplexer redis)
        {
            _pdfGenerator = pdfGenerator;
            _emailService = emailService;
            _redis = redis.GetDatabase();
        }

        public async Task Consume(ConsumeContext<BookingConfirmed> context)
        {
            var message = context.Message;
            Console.WriteLine($"Generating notification for Booking: {message.BookingId}");

            var pdf = _pdfGenerator.GenerateTicket(
                message.CustomerEmail,
                message.ShowName,
                message.ShowTime,
                message.SeatNumbers,
                message.TotalAmount);

            // Save PDF to Redis with 30-day expiration
            await _redis.StringSetAsync($"ticket:{message.BookingId}", pdf, TimeSpan.FromDays(30));
            Console.WriteLine($"Saved PDF ticket to Redis for Booking: {message.BookingId}");

            await _emailService.SendEmailWithAttachmentAsync(
                message.CustomerEmail,
                "Your Ticket Booking Confirmation",
                $"Hello,\n\nYour booking for {message.ShowName} is confirmed. Please find your ticket attached.",
                pdf,
                $"Ticket_{message.BookingId}.pdf");
        }
    }
}
