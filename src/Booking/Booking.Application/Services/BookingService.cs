using Booking.Application.DTOs;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Booking.Domain.Repositories;
using Booking.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TicketBooking.Common.Events;

namespace Booking.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repository;
        private readonly ISeatReservationService _reservationService;
        private readonly IEventBus _eventBus;

        public BookingService(
            IBookingRepository repository,
            ISeatReservationService reservationService,
            IEventBus eventBus)
        {
            _repository = repository;
            _reservationService = reservationService;
            _eventBus = eventBus;
        }

        public async Task<Guid> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
        {
            var bookingId = Guid.NewGuid();

            // Phase 1: Try to reserve seats in Redis (In-memory, high scale)
            var reserved = await _reservationService.ReserveSeatsAsync(request.ShowId, request.SeatNumbers, bookingId, TimeSpan.FromMinutes(10));

            if (!reserved)
            {
                throw new Exception("Conflict: One or more selected seats are already being booked.");
            }

            // Phase 2: Publish event for background processing (Asynchronous)
            await _eventBus.PublishAsync(new BookingRequested
            {
                BookingId = bookingId,
                ShowId = request.ShowId,
                ShowName = request.ShowName,
                ShowTime = request.ShowTime,
                CustomerEmail = request.CustomerEmail,
                SeatNumbers = request.SeatNumbers,
                TotalAmount = request.TotalAmount
            });

            return bookingId;
        }

        public async Task<BookingDto?> GetBookingAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var booking = await _repository.GetByIdAsync(id);
            if (booking == null) return null;

            return new BookingDto
            {
                Id = booking.Id,
                ShowId = booking.ShowId,
                ShowName = booking.ShowName,
                ShowTime = booking.ShowTime,
                CustomerEmail = booking.CustomerEmail,
                SeatNumbers = booking.SeatNumbers,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt
            };
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(string email, CancellationToken cancellationToken = default)
        {
            var bookings = await _repository.GetByEmailAsync(email);

            return bookings.Select(b => new BookingDto
            {
                Id = b.Id,
                ShowId = b.ShowId,
                ShowName = b.ShowName,
                ShowTime = b.ShowTime,
                CustomerEmail = b.CustomerEmail,
                SeatNumbers = b.SeatNumbers,
                TotalAmount = b.TotalAmount,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt
            });
        }

        public async Task CancelBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _repository.GetByIdAsync(bookingId);
            if (booking == null) return;

            // 1. Release locks in Redis
            await _reservationService.ReleaseSeatsAsync(booking.ShowId, booking.SeatNumbers);

            // 2. Update status
            booking.Status = Domain.Enums.BookingStatus.Cancelled;
            await _repository.UpdateAsync(booking);

            // 3. Mark seats available in DB
            await _repository.MarkSeatsAsAvailableAsync(booking.Id);

            await _repository.SaveChangesAsync();
        }

        public async Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _repository.GetByIdAsync(bookingId);
            if (booking == null) return;

            await _reservationService.ReleaseSeatsAsync(booking.ShowId, booking.SeatNumbers);

            // 2. Update status in DB
            booking.Status = Domain.Enums.BookingStatus.Confirmed;
            await _repository.UpdateAsync(booking);
            await _repository.SaveChangesAsync();

            await _eventBus.PublishAsync(new BookingConfirmed
            {
                BookingId = booking.Id,
                CustomerEmail = booking.CustomerEmail,
                ShowName = booking.ShowName,
                ShowTime = booking.ShowTime,
                SeatNumbers = booking.SeatNumbers,
                TotalAmount = booking.TotalAmount
            });
        }

        public async Task PersistBookingAsync(Guid bookingId, Guid showId, string showName, DateTime showTime, string customerEmail, List<string> seatNumbers, decimal totalAmount, CancellationToken cancellationToken = default)
        {
            var booking = new BookingRecord
            {
                Id = bookingId,
                ShowId = showId,
                ShowName = showName,
                ShowTime = showTime,
                CustomerEmail = customerEmail,
                SeatNumbers = seatNumbers,
                TotalAmount = totalAmount,
                Status = Domain.Enums.BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(booking);
            
            // We use UpsertSeatsAsync instead of a simple Add to handle cases where 
            // a seat record might already exist (e.g., from a previously cancelled booking).
            // This prevents unique constraint violations on (ShowId, SeatNumber).
            await _repository.UpsertSeatsAsync(showId, seatNumbers, bookingId);
            await _repository.SaveChangesAsync();

            await _eventBus.PublishAsync(new BookingInitiated
            {
                BookingId = booking.Id,
                CustomerEmail = booking.CustomerEmail,
                TotalAmount = booking.TotalAmount,
                SeatNumbers = booking.SeatNumbers,
                ShowName = booking.ShowName,
                ShowTime = booking.ShowTime
            });
        }
    }
}
