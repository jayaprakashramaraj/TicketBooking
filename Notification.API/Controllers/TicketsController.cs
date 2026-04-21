using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Notification.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly IDatabase _redis;

        public TicketsController(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        [HttpGet("{id}")]
        [HttpHead("{id}")]
        public async Task<IActionResult> DownloadTicket(Guid id)
        {
            var pdfData = await _redis.StringGetAsync($"ticket:{id}");

            if (pdfData.IsNull)
            {
                return NotFound("Ticket not found or expired.");
            }

            if (Request.Method == "HEAD")
            {
                return Ok();
            }

            return File((byte[])pdfData!, "application/pdf", $"Ticket_{id}.pdf");
        }
    }
}
