using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Notification.Application.Interfaces;

namespace Notification.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetTicket(Guid bookingId)
        {
            var pdf = await _ticketService.GetTicketAsync(bookingId);
            if (pdf == null) return NotFound("Ticket not found or still generating.");

            return File(pdf, "application/pdf", $"Ticket_{bookingId}.pdf");
        }
    }
}
