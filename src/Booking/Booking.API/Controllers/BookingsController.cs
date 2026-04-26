using Microsoft.AspNetCore.Mvc;
using Booking.Application.Interfaces;
using Booking.Application.DTOs;

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ISeatService _seatService;

        public BookingsController(IBookingService bookingService, ISeatService seatService)
        {
            _bookingService = bookingService;
            _seatService = seatService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBookingRequest request, [FromServices] ILogger<BookingsController> logger, [FromServices] IConfiguration config)
        {
            try
            {
                var bookingId = await _bookingService.CreateBookingAsync(request);
                
                // Get URLs from configuration
                var simulatorUrl = (config["PaymentSimulatorUrl"] ?? "").TrimEnd('/');
                var defaultUiUrl = (config["UIBaseUrl"] ?? "").TrimEnd('/');
                
                if (string.IsNullOrEmpty(simulatorUrl))
                {
                    logger.LogError("PaymentSimulatorUrl is not configured!");
                    return StatusCode(500, "Payment simulator configuration is missing.");
                }

                // Use the redirect URL provided by the frontend, or fall back to the configured base UI URL
                var redirectUrl = !string.IsNullOrEmpty(request.RedirectUrl) 
                    ? request.RedirectUrl.TrimEnd('/') 
                    : $"{defaultUiUrl}/booking-result";

                // Construct the final payment URL. 
                // The simulatorUrl should be configured to include its full base path (e.g. http://localhost:5002/payment-simulator)
                var paymentUrl = $"{simulatorUrl}/index.html?bookingId={bookingId}&amount={request.TotalAmount}&redirectUrl={redirectUrl}";

                return StatusCode(202, new 
                { 
                    BookingId = bookingId, 
                    Status = "Processing",
                    PaymentUrl = paymentUrl 
                });
            }
            catch (Exception ex) when (ex.Message.Contains("Conflict"))
            {
                logger.LogWarning("Booking conflict: {Message}", ex.Message);
                return Conflict("One or more selected seats are already being booked. Please try different seats.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating booking for {Email}", request.CustomerEmail);
                return BadRequest($"Booking failed: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var booking = await _bookingService.GetBookingAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetBookingStatus(Guid id)
        {
            var booking = await _bookingService.GetBookingAsync(id);
            
            if (booking == null)
            {
                return Ok(new { BookingId = id, Status = "Processing" });
            }

            return Ok(new { BookingId = id, Status = booking.Status });
        }

        [HttpGet("shows/{showId}/seats")]
        public async Task<IActionResult> GetShowSeats(Guid showId)
        {
            var bookedSeats = await _seatService.GetBookedSeatsAsync(showId);
            return Ok(bookedSeats);
        }

        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetByUser(string email)
        {
            var bookings = await _bookingService.GetBookingsByUserAsync(email);
            return Ok(bookings);
        }
    }
}
