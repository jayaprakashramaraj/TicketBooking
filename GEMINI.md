# Project Context: Ticket Booking System

## Overview
A microservice-based ticket booking system using Domain-Driven Design (DDD) and Event-Driven Architecture.

## Tech Stack
- Backend: .NET 10.0 Web API
- Language: C#
- Message Broker: RabbitMQ (MassTransit)
- PDF Generation: QuestPDF
- Email: MailKit

## Microservices
1. **Catalog.API:** Show search and discovery.
2. **Booking.API:** Seat reservation and booking management.
3. **Payment.API:** Payment processing simulation.
4. **Notification.API:** PDF ticket generation and email notifications.

## Event Flow
- `BookingInitiated` (Booking -> Payment)
- `PaymentCompleted` (Payment -> Booking)
- `BookingConfirmed` (Booking -> Notification)
