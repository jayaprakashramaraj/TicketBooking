-- Seed data for past and upcoming bookings
USE [BookingDb];
GO

-- 1. Create a Past Booking (The Dark Knight)
DECLARE @PastBookingId UNIQUEIDENTIFIER = 'B1B1B1B1-B1B1-B1B1-B1B1-B1B1B1B1B1B1';
DECLARE @DarkKnightShowId UNIQUEIDENTIFIER = '5843CF14-46B0-42C9-B0AF-ACD30D2137F6';

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE Id = @PastBookingId)
BEGIN
    INSERT INTO Bookings (Id, ShowId, ShowName, ShowTime, CustomerEmail, SeatNumbers, TotalAmount, Status, CreatedAt)
    VALUES (
        @PastBookingId, 
        @DarkKnightShowId, 
        'The Dark Knight', 
        DATEADD(day, -5, GETUTCDATE()), -- 5 days ago
        'jpsoftmail@gmail.com', 
        '["C1", "C2"]', 
        30.00, 
        1, -- Confirmed
        DATEADD(day, -6, GETUTCDATE())
    );

    INSERT INTO Seats (Id, ShowId, SeatNumber, IsBooked, BookingId)
    VALUES 
        (NEWID(), @DarkKnightShowId, 'C1', 1, @PastBookingId),
        (NEWID(), @DarkKnightShowId, 'C2', 1, @PastBookingId);
END

-- 2. Create an Upcoming Booking (Inception)
DECLARE @FutureBookingId UNIQUEIDENTIFIER = 'F2F2F2F2-F2F2-F2F2-F2F2-F2F2F2F2F2F2';
DECLARE @InceptionShowId UNIQUEIDENTIFIER = 'E4BF9E8D-873E-44E4-9C37-608AB50196DF';

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE Id = @FutureBookingId)
BEGIN
    INSERT INTO Bookings (Id, ShowId, ShowName, ShowTime, CustomerEmail, SeatNumbers, TotalAmount, Status, CreatedAt)
    VALUES (
        @FutureBookingId, 
        @InceptionShowId, 
        'Inception', 
        DATEADD(day, 2, GETUTCDATE()), -- 2 days from now
        'jpsoftmail@gmail.com', 
        '["A7", "A8"]', 
        30.00, 
        0, -- Pending
        GETUTCDATE()
    );

    INSERT INTO Seats (Id, ShowId, SeatNumber, IsBooked, BookingId)
    VALUES 
        (NEWID(), @InceptionShowId, 'A7', 1, @FutureBookingId),
        (NEWID(), @InceptionShowId, 'A8', 1, @FutureBookingId);
END

GO
