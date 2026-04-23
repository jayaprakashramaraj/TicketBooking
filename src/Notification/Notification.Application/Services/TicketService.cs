using System;
using System.Threading.Tasks;
using Notification.Application.Interfaces;
using Notification.Domain.Repositories;
using TicketBooking.Common.Events;

namespace Notification.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IEmailService _emailService;
        private readonly ITicketRepository _ticketRepository;

        public TicketService(IPdfGenerator pdfGenerator, IEmailService emailService, ITicketRepository ticketRepository)
        {
            _pdfGenerator = pdfGenerator;
            _emailService = emailService;
            _ticketRepository = ticketRepository;
        }

        public async Task ProcessBookingConfirmedAsync(BookingConfirmed bookingConfirmed)
        {
            // Generate PDF
            var pdfContent = _pdfGenerator.GenerateTicket(
                bookingConfirmed.CustomerEmail,
                bookingConfirmed.ShowName,
                bookingConfirmed.ShowTime,
                bookingConfirmed.SeatNumbers,
                bookingConfirmed.TotalAmount);

            // Save to Repository (Redis)
            await _ticketRepository.SaveTicketAsync(bookingConfirmed.BookingId, pdfContent);

            // Send Email
            await _emailService.SendEmailWithAttachmentAsync(
                bookingConfirmed.CustomerEmail,
                "Your Ticket Confirmation",
                "Please find your ticket attached.",
                pdfContent,
                $"Ticket_{bookingConfirmed.BookingId}.pdf");
        }

        public async Task<byte[]?> GetTicketAsync(Guid bookingId)
        {
            return await _ticketRepository.GetTicketAsync(bookingId);
        }
    }
}
