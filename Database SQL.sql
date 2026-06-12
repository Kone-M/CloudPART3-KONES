-- =============================================
-- Database: VenueBookingSystem
-- Description: Complete database schema for Event Booking System
-- Date: 2026-06-12
-- =============================================

-- =============================================
-- 1. EVENT TYPES TABLE (Lookup)
-- =============================================
CREATE TABLE [dbo].[EventTypes] (
    [EventTypeID] INT IDENTITY(1,1) NOT NULL,
    [CategoryName] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(200) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_EventTypes] PRIMARY KEY CLUSTERED ([EventTypeID] ASC)
);

-- Create indexes for EventTypes
CREATE NONCLUSTERED INDEX [IX_EventTypes_IsActive] ON [dbo].[EventTypes]([IsActive] ASC);
CREATE NONCLUSTERED INDEX [IX_EventTypes_DisplayOrder] ON [dbo].[EventTypes]([DisplayOrder] ASC);

-- =============================================
-- 2. VENUES TABLE
-- =============================================
CREATE TABLE [dbo].[Venues] (
    [VenueID] INT IDENTITY(1,1) NOT NULL,
    [VenueName] NVARCHAR(100) NOT NULL,
    [Location] NVARCHAR(200) NOT NULL,
    [Capacity] INT NOT NULL,
    [ImageUrl] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [AvailabilityStatus] NVARCHAR(20) NOT NULL DEFAULT 'Available',
    [LastMaintenanceDate] DATE NULL,
    [NextAvailableDate] DATETIME NULL,
    [OperatingHours] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Venues] PRIMARY KEY CLUSTERED ([VenueID] ASC),
    CONSTRAINT [CHK_Venues_Capacity] CHECK ([Capacity] > 0),
    CONSTRAINT [CHK_Venues_AvailabilityStatus] CHECK ([AvailabilityStatus] IN ('Available', 'Maintenance', 'Booked', 'Limited'))
);

-- Create indexes for Venues
CREATE NONCLUSTERED INDEX [IX_Venues_VenueName] ON [dbo].[Venues]([VenueName] ASC);
CREATE NONCLUSTERED INDEX [IX_Venues_IsActive] ON [dbo].[Venues]([IsActive] ASC);
CREATE NONCLUSTERED INDEX [IX_Venues_AvailabilityStatus] ON [dbo].[Venues]([AvailabilityStatus] ASC);
CREATE NONCLUSTERED INDEX [IX_Venues_Location] ON [dbo].[Venues]([Location] ASC);

-- =============================================
-- 3. EVENTS TABLE
-- =============================================
CREATE TABLE [dbo].[Events] (
    [EventID] INT IDENTITY(1,1) NOT NULL,
    [EventName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [EventDate] DATETIME NOT NULL,
    [DurationHours] INT NOT NULL,
    [OrganizerName] NVARCHAR(100) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [EventTypeID] INT NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([EventID] ASC),
    CONSTRAINT [FK_Events_EventTypes] FOREIGN KEY ([EventTypeID]) 
        REFERENCES [dbo].[EventTypes]([EventTypeID]) ON DELETE SET NULL,
    CONSTRAINT [CHK_Events_DurationHours] CHECK ([DurationHours] BETWEEN 1 AND 72)
);

-- Create indexes for Events
CREATE NONCLUSTERED INDEX [IX_Events_EventName] ON [dbo].[Events]([EventName] ASC);
CREATE NONCLUSTERED INDEX [IX_Events_EventDate] ON [dbo].[Events]([EventDate] ASC);
CREATE NONCLUSTERED INDEX [IX_Events_EventTypeID] ON [dbo].[Events]([EventTypeID] ASC);
CREATE NONCLUSTERED INDEX [IX_Events_OrganizerName] ON [dbo].[Events]([OrganizerName] ASC);

-- =============================================
-- 4. BOOKINGS TABLE
-- =============================================
CREATE TABLE [dbo].[Bookings] (
    [BookingID] INT IDENTITY(1,1) NOT NULL,
    [VenueID] INT NOT NULL,
    [EventID] INT NOT NULL,
    [BookingDate] DATETIME NOT NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Confirmed',
    [SpecialRequests] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Bookings] PRIMARY KEY CLUSTERED ([BookingID] ASC),
    CONSTRAINT [FK_Bookings_Venues] FOREIGN KEY ([VenueID]) 
        REFERENCES [dbo].[Venues]([VenueID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Bookings_Events] FOREIGN KEY ([EventID]) 
        REFERENCES [dbo].[Events]([EventID]) ON DELETE NO ACTION,
    CONSTRAINT [CHK_Bookings_Status] CHECK ([Status] IN ('Confirmed', 'Cancelled', 'Completed', 'Pending'))
);

-- Create indexes for Bookings
CREATE NONCLUSTERED INDEX [IX_Bookings_VenueID] ON [dbo].[Bookings]([VenueID] ASC);
CREATE NONCLUSTERED INDEX [IX_Bookings_EventID] ON [dbo].[Bookings]([EventID] ASC);
CREATE NONCLUSTERED INDEX [IX_Bookings_BookingDate] ON [dbo].[Bookings]([BookingDate] ASC);
CREATE NONCLUSTERED INDEX [IX_Bookings_Status] ON [dbo].[Bookings]([Status] ASC);
CREATE NONCLUSTERED INDEX [IX_Bookings_CreatedAt] ON [dbo].[Bookings]([CreatedAt] ASC);

-- Unique constraint to prevent double booking
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Booking_DateTime] 
    ON [dbo].[Bookings]([VenueID] ASC, [BookingDate] ASC);

-- Composite index for common queries
CREATE NONCLUSTERED INDEX [IX_Bookings_VenueID_BookingDate] 
    ON [dbo].[Bookings]([VenueID] ASC, [BookingDate] ASC) 
    INCLUDE ([Status], [EventID]);

-- =============================================
-- 5. ENHANCED BOOKINGS VIEW
-- =============================================
CREATE OR ALTER VIEW [dbo].[vw_EnhancedBookings]
AS
SELECT 
    b.BookingID,
    b.BookingDate,
    b.Status,
    b.SpecialRequests,
    b.CreatedAt AS BookingCreatedAt,
    v.VenueID,
    v.VenueName,
    v.Location,
    v.Capacity,
    v.ImageUrl,
    v.AvailabilityStatus AS VenueAvailabilityStatus,
    e.EventID,
    e.EventName,
    e.Description,
    e.EventDate,
    e.DurationHours,
    e.OrganizerName,
    et.EventTypeID,
    et.CategoryName AS EventTypeName,
    -- Calculated fields
    DATEADD(HOUR, e.DurationHours, e.EventDate) AS EventEndTime,
    DATEDIFF(DAY, GETDATE(), e.EventDate) AS DaysUntilEvent,
    CASE 
        WHEN e.EventDate > GETDATE() THEN 'Upcoming'
        WHEN e.EventDate < GETDATE() THEN 'Past'
        ELSE 'Today'
    END AS EventStatus,
    CONCAT(e.EventName, ' @ ', v.VenueName) AS DisplayTitle
FROM Bookings b
INNER JOIN Venues v ON b.VenueID = v.VenueID
INNER JOIN Events e ON b.EventID = e.EventID
LEFT JOIN EventTypes et ON e.EventTypeID = et.EventTypeID;

-- =============================================
-- 6. STORED PROCEDURES
-- =============================================

-- Stored Procedure: Get Available Venues for a Date Range
CREATE OR ALTER PROCEDURE [dbo].[sp_GetAvailableVenues]
    @StartDate DATETIME,
    @EndDate DATETIME,
    @Capacity INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT v.*
    FROM Venues v
    WHERE v.IsActive = 1 
        AND v.AvailabilityStatus = 'Available'
        AND (@Capacity IS NULL OR v.Capacity >= @Capacity)
        AND NOT EXISTS (
            SELECT 1 
            FROM Bookings b 
            WHERE b.VenueID = v.VenueID 
                AND b.BookingDate BETWEEN @StartDate AND @EndDate
                AND b.Status = 'Confirmed'
        )
    ORDER BY v.VenueName;
END;

-- Stored Procedure: Get Booking Statistics
CREATE OR ALTER PROCEDURE [dbo].[sp_GetBookingStatistics]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(DISTINCT b.BookingID) AS TotalBookings,
        COUNT(DISTINCT CASE WHEN b.Status = 'Confirmed' THEN b.BookingID END) AS ConfirmedBookings,
        COUNT(DISTINCT CASE WHEN b.Status = 'Cancelled' THEN b.BookingID END) AS CancelledBookings,
        COUNT(DISTINCT CASE WHEN b.Status = 'Completed' THEN b.BookingID END) AS CompletedBookings,
        COUNT(DISTINCT v.VenueID) AS TotalVenues,
        COUNT(DISTINCT CASE WHEN v.AvailabilityStatus = 'Available' THEN v.VenueID END) AS AvailableVenues,
        COUNT(DISTINCT e.EventID) AS TotalEvents,
        COUNT(DISTINCT e.EventTypeID) AS TotalEventTypes,
        SUM(e.DurationHours) AS TotalBookingHours,
        AVG(v.Capacity) AS AverageVenueCapacity
    FROM Bookings b
    CROSS JOIN Venues v
    CROSS JOIN Events e
    WHERE (@StartDate IS NULL OR b.BookingDate >= @StartDate)
        AND (@EndDate IS NULL OR b.BookingDate <= @EndDate);
END;

-- Stored Procedure: Search Bookings with Filters
CREATE OR ALTER PROCEDURE [dbo].[sp_SearchBookings]
    @SearchTerm NVARCHAR(100) = NULL,
    @EventTypeID INT = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM vw_EnhancedBookings
    WHERE (@SearchTerm IS NULL 
        OR BookingID LIKE '%' + @SearchTerm + '%'
        OR EventName LIKE '%' + @SearchTerm + '%'
        OR VenueName LIKE '%' + @SearchTerm + '%'
        OR OrganizerName LIKE '%' + @SearchTerm + '%')
    AND (@EventTypeID IS NULL OR EventTypeID = @EventTypeID)
    AND (@StartDate IS NULL OR EventDate >= @StartDate)
    AND (@EndDate IS NULL OR EventDate <= @EndDate)
    AND (@Status IS NULL OR Status = @Status)
    ORDER BY BookingDate DESC;
END;

-- =============================================
-- 7. SEED DATA
-- =============================================

-- Insert Event Types
SET IDENTITY_INSERT [dbo].[EventTypes] ON;
INSERT INTO [dbo].[EventTypes] ([EventTypeID], [CategoryName], [Description], [DisplayOrder], [IsActive], [CreatedAt])
VALUES 
    (1, 'Conference', 'Business conferences and seminars', 1, 1, GETDATE()),
    (2, 'Wedding', 'Wedding ceremonies and receptions', 2, 1, GETDATE()),
    (3, 'Workshop', 'Training workshops and classes', 3, 1, GETDATE()),
    (4, 'Concert', 'Music concerts and performances', 4, 1, GETDATE()),
    (5, 'Corporate Event', 'Corporate meetings and events', 5, 1, GETDATE()),
    (6, 'Birthday Party', 'Birthday celebrations', 6, 1, GETDATE()),
    (7, 'Exhibition', 'Art and trade exhibitions', 7, 1, GETDATE()),
    (8, 'Other', 'Other types of events', 8, 1, GETDATE());
SET IDENTITY_INSERT [dbo].[EventTypes] OFF;

-- Insert Sample Venues
SET IDENTITY_INSERT [dbo].[Venues] ON;
INSERT INTO [dbo].[Venues] ([VenueID], [VenueName], [Location], [Capacity], [ImageUrl], [IsActive], [AvailabilityStatus], [OperatingHours], [CreatedAt])
VALUES 
    (1, 'Grand Conference Hall', '123 Business Ave, Downtown', 500, NULL, 1, 'Available', 'Mon-Fri 8AM-10PM, Sat-Sun 9AM-8PM', GETDATE()),
    (2, 'Garden Wedding Pavilion', '45 Park Lane, Suburbs', 200, NULL, 1, 'Available', 'Daily 10AM-9PM', GETDATE()),
    (3, 'Tech Workshop Center', '89 Innovation Drive', 50, NULL, 1, 'Available', 'Mon-Fri 9AM-9PM', GETDATE()),
    (4, 'Concert Arena', '100 Music Boulevard', 1000, NULL, 1, 'Maintenance', 'By appointment only', GETDATE()),
    (5, 'Corporate Boardroom', '22 Executive Plaza', 30, NULL, 1, 'Limited', 'Mon-Fri 9AM-5PM', GETDATE());
SET IDENTITY_INSERT [dbo].[Venues] OFF;

-- Insert Sample Events
SET IDENTITY_INSERT [dbo].[Events] ON;
INSERT INTO [dbo].[Events] ([EventID], [EventName], [Description], [EventDate], [DurationHours], [OrganizerName], [EventTypeID], [CreatedAt])
VALUES 
    (1, 'Annual Tech Conference 2026', 'Biggest tech conference of the year', DATEADD(DAY, 30, GETDATE()), 8, 'Tech Events Inc', 1, GETDATE()),
    (2, 'Smith-Johnson Wedding', 'Wedding ceremony and reception', DATEADD(DAY, 15, GETDATE()), 5, 'Wedding Planners Co', 2, GETDATE()),
    (3, 'Azure Cloud Workshop', 'Hands-on Azure training', DATEADD(DAY, 7, GETDATE()), 3, 'Microsoft Learning', 3, GETDATE()),
    (4, 'Summer Music Festival', 'Outdoor concert experience', DATEADD(DAY, 45, GETDATE()), 6, 'Live Nation', 4, GETDATE()),
    (5, 'Annual Shareholder Meeting', 'Yearly corporate meeting', DATEADD(DAY, 20, GETDATE()), 4, 'Global Corp', 5, GETDATE());
SET IDENTITY_INSERT [dbo].[Events] OFF;

-- Insert Sample Bookings
SET IDENTITY_INSERT [dbo].[Bookings] ON;
INSERT INTO [dbo].[Bookings] ([BookingID], [VenueID], [EventID], [BookingDate], [Status], [SpecialRequests], [CreatedAt])
VALUES 
    (1, 1, 1, DATEADD(DAY, -5, GETDATE()), 'Confirmed', 'Need projector and sound system', GETDATE()),
    (2, 2, 2, DATEADD(DAY, -3, GETDATE()), 'Confirmed', 'Flower arrangements required', GETDATE()),
    (3, 3, 3, DATEADD(DAY, -1, GETDATE()), 'Confirmed', 'WiFi for 50 attendees', GETDATE()),
    (4, 5, 5, DATEADD(DAY, -7, GETDATE()), 'Cancelled', 'Reschedule requested', GETDATE());
SET IDENTITY_INSERT [dbo].[Bookings] OFF;

-- =============================================
-- 8. ADDITIONAL HELPER FUNCTIONS
-- =============================================

-- Function: Check Venue Availability
CREATE OR ALTER FUNCTION [dbo].[fn_IsVenueAvailable]
(
    @VenueID INT,
    @BookingDate DATETIME
)
RETURNS BIT
AS
BEGIN
    DECLARE @IsAvailable BIT = 1;
    
    IF EXISTS (
        SELECT 1 
        FROM Bookings 
        WHERE VenueID = @VenueID 
            AND BookingDate = @BookingDate 
            AND Status = 'Confirmed'
    )
    BEGIN
        SET @IsAvailable = 0;
    END
    
    RETURN @IsAvailable;
END;

-- Function: Get Upcoming Events Count
CREATE OR ALTER FUNCTION [dbo].[fn_GetUpcomingEventsCount]()
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    
    SELECT @Count = COUNT(*)
    FROM Events
    WHERE EventDate > GETDATE();
    
    RETURN @Count;
END;

-- =============================================
-- 9. TRIGGERS FOR AUDIT AND VALIDATION
-- =============================================

-- Trigger: Prevent booking if venue is under maintenance
CREATE OR ALTER TRIGGER [dbo].[trg_CheckVenueAvailability]
ON [dbo].[Bookings]
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        INNER JOIN Venues v ON i.VenueID = v.VenueID
        WHERE v.AvailabilityStatus = 'Maintenance'
    )
    BEGIN
        RAISERROR('Cannot book venue that is under maintenance', 16, 1);
        RETURN;
    END
    
    INSERT INTO Bookings (VenueID, EventID, BookingDate, Status, SpecialRequests, CreatedAt)
    SELECT VenueID, EventID, BookingDate, Status, SpecialRequests, CreatedAt
    FROM inserted;
END;

-- =============================================
-- 10. DATABASE PERMISSIONS (AZURE SQL)
-- =============================================

-- Create roles for different access levels
-- Uncomment and modify as needed for your Azure SQL Database

/*
-- Create roles
CREATE ROLE BookingAdmin;
CREATE ROLE BookingStaff;
CREATE ROLE BookingViewer;

-- Grant permissions to Admin
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Venues] TO BookingAdmin;
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Events] TO BookingAdmin;
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Bookings] TO BookingAdmin;
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[EventTypes] TO BookingAdmin;
GRANT EXECUTE ON [dbo].[sp_GetAvailableVenues] TO BookingAdmin;
GRANT EXECUTE ON [dbo].[sp_GetBookingStatistics] TO BookingAdmin;
GRANT EXECUTE ON [dbo].[sp_SearchBookings] TO BookingAdmin;

-- Grant permissions to Staff
GRANT SELECT, INSERT, UPDATE ON [dbo].[Bookings] TO BookingStaff;
GRANT SELECT ON [dbo].[Venues] TO BookingStaff;
GRANT SELECT ON [dbo].[Events] TO BookingStaff;
GRANT SELECT ON [dbo].[EventTypes] TO BookingStaff;
GRANT EXECUTE ON [dbo].[sp_GetAvailableVenues] TO BookingStaff;
GRANT EXECUTE ON [dbo].[sp_SearchBookings] TO BookingStaff;

-- Grant permissions to Viewer
GRANT SELECT ON [dbo].[vw_EnhancedBookings] TO BookingViewer;
GRANT SELECT ON [dbo].[Venues] TO BookingViewer;
GRANT SELECT ON [dbo].[Events] TO BookingViewer;
*/

-- =============================================
-- 11. VERIFICATION QUERIES
-- =============================================

-- Verify all tables exist
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('EventTypes', 'Venues', 'Events', 'Bookings')
ORDER BY TABLE_NAME;

-- Verify all columns in Venues table
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Venues'
ORDER BY ORDINAL_POSITION;

-- Verify the enhanced view
SELECT TOP 10 * FROM vw_EnhancedBookings;

-- Run statistics procedure
EXEC sp_GetBookingStatistics;

-- Check available venues
EXEC sp_GetAvailableVenues @StartDate = GETDATE(), @EndDate = DATEADD(DAY, 30, GETDATE());

-- Test search functionality
EXEC sp_SearchBookings @SearchTerm = 'Conference';