using Booking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Booking.Application.Interfaces
{
    public interface IBookingService
    {
        Task<Guid> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
        Task<BookingDto?> GetBookingAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(string email, CancellationToken cancellationToken = default);
        Task CancelBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
        Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
        Task PersistBookingAsync(Guid bookingId, Guid showId, string showName, DateTime showTime, string customerEmail, List<string> seatNumbers, decimal totalAmount, CancellationToken cancellationToken = default);
    }
}
