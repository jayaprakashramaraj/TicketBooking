using MassTransit;
using System;
using System.Threading.Tasks;
using TicketBooking.Common.Events;
using Notification.API.Services;

namespace Notification.API.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
    {
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IEmailService _emailService;

        public BookingConfirmedConsumer(IPdfGenerator pdfGenerator, IEmailService emailService)
        {
            _pdfGenerator = pdfGenerator;
            _emailService = emailService;
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

            await _emailService.SendEmailWithAttachmentAsync(
                message.CustomerEmail,
                "Your Ticket Booking Confirmation",
                $"Hello,\n\nYour booking for {message.ShowName} is confirmed. Please find your ticket attached.",
                pdf,
                $"Ticket_{message.BookingId}.pdf");
        }
    }
}
