using Booking.API.Data;
using Booking.API.Domain;
using Booking.API.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TicketBooking.Common.Events;

namespace Booking.API.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailed>
    {
        private readonly BookingDbContext _context;
        private readonly ISeatReservationService _reservationService;
        private readonly ILogger<PaymentFailedConsumer> _logger;

        public PaymentFailedConsumer(BookingDbContext context, ISeatReservationService reservationService, ILogger<PaymentFailedConsumer> logger)
        {
            _context = context;
            _reservationService = reservationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            var message = context.Message;
            _logger.LogWarning("Payment failed for booking {BookingId}. Reason: {Reason}. Releasing seats.", message.BookingId, message.Reason);

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == message.BookingId);

            if (booking != null)
            {
                // 1. Release the locks in Redis immediately
                await _reservationService.ReleaseSeatsAsync(booking.ShowId, booking.SeatNumbers);
                _logger.LogInformation("Released Redis seats for show {ShowId}: {Seats}", booking.ShowId, string.Join(", ", booking.SeatNumbers));

                // 2. Update DB status
                booking.Status = BookingStatus.Cancelled;
                
                // 3. Mark seats as unbooked in SQL as well (if they were persisted)
                var seats = await _context.Seats.Where(s => s.BookingId == booking.Id).ToListAsync();
                foreach(var seat in seats)
                {
                    seat.IsBooked = false;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Booking {BookingId} marked as Cancelled in database.", booking.Id);
            }
            else
            {
                _logger.LogError("Could not find booking {BookingId} to release seats.", message.BookingId);
            }
        }
    }
}
