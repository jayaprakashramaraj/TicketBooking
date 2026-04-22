using System;
using System.Collections.Generic;

namespace Booking.Application.DTOs
{
    public record CreateBookingRequest
    {
        public Guid ShowId { get; init; }
        public required string ShowName { get; init; }
        public DateTime ShowTime { get; init; }
        public required string CustomerEmail { get; init; }
        public required List<string> SeatNumbers { get; init; }
        public decimal TotalAmount { get; init; }
    }

    public record BookingDto
    {
        public Guid Id { get; init; }
        public Guid ShowId { get; init; }
        public required string ShowName { get; init; }
        public DateTime ShowTime { get; init; }
        public required string CustomerEmail { get; init; }
        public required List<string> SeatNumbers { get; init; }
        public decimal TotalAmount { get; init; }
        public required string Status { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
