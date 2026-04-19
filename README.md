# Ticket Booking System - Microservices

A decoupled, event-driven ticket booking system built with .NET and Domain-Driven Design.

## Architecture
- **Catalog Service:** Search for shows.
- **Booking Service:** Reserve seats and manage bookings.
- **Payment Service:** Process payments.
- **Notification Service:** Generate PDF tickets and send emails.

## Prerequisites
- .NET 10.0 SDK
- Docker (for RabbitMQ)

## Running the Project

1. **Start RabbitMQ:**
   ```bash
   docker-compose up -d
   ```

2. **Run Microservices:**
   You can run each service using `dotnet run` from its directory, or start them all via your IDE.

   - Catalog: `http://localhost:5000` (example)
   - Booking: `http://localhost:5001` (example)
   - Payment: `http://localhost:5002` (example)
   - Notification: `http://localhost:5003` (example)

## Feature Flow
1. **Search:** `GET /api/shows/search?time=...` on Catalog Service.
2. **Book:** `POST /api/bookings` on Booking Service.
3. **Automated Flow:**
   - Booking Service publishes `BookingInitiated`.
   - Payment Service consumes it and publishes `PaymentCompleted`.
   - Booking Service consumes it and publishes `BookingConfirmed`.
   - Notification Service consumes it, generates a PDF ticket, and sends an email.
