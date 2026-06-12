
# ?? Venue Booking System - CloudPART3-KONES

![Version](https://img.shields.io/badge/version-3.0-blue)
![ASP.NET](https://img.shields.io/badge/ASP.NET-Core%206.0-purple)
![Azure](https://img.shields.io/badge/Azure-Deployed-blue)
![Database](https://img.shields.io/badge/Database-Azure%20SQL-green)

## ?? Project Overview

A comprehensive venue booking management system built with ASP.NET Core MVC and deployed on Microsoft Azure. The system allows users to manage venues, events, and bookings with advanced filtering capabilities, double-booking prevention, and real-time availability tracking.

**Live Application URL:** [https://cloudpart220260612150138-cjaregbga8ghcdah.canadacentral-01.azurewebsites.net/]

---

## ? Features

### ?? Advanced Search & Filtering
- Search by Booking ID, Event Name, Venue Name, or Organizer
- Filter by Event Type (Conference, Wedding, Workshop, Concert, etc.)
- Filter by Venue Availability Status
- Date Range Filtering (Start Date / End Date)
- Real-time search results with count

### ?? Venue Management
- Create, Read, Update, Delete venues
- Upload venue images to Azure Blob Storage
- Track venue availability status (Available, Maintenance, Booked, Limited)
- Set operating hours and capacity
- View venue statistics and booking history

### ?? Event Management
- Create and manage events with categories
- Event Type classification system (8 predefined types)
- Set event dates, durations, and organizer information
- Track upcoming and past events
- View event booking statistics

### ?? Booking Management
- Book venues for specific events
- Double-booking prevention system
- Track booking status (Confirmed, Cancelled, Completed)
- Special requests handling
- Automatic conflict detection
- Email-style confirmation messages

### ?? Dashboard & Reporting
- Real-time statistics cards
- Recent bookings display
- Venue availability overview
- Booking status breakdown
- Quick action buttons

### ?? Azure Integration
- Azure App Service hosting
- Azure SQL Database for data persistence
- Azure Blob Storage for image uploads
- Cloud-native architecture

---

## ??? Technology Stack

### Backend
| Technology | Purpose |
|------------|---------|
| ASP.NET Core MVC 6.0 | Web framework |
| C# 10.0 | Programming language |
| Entity Framework Core | ORM for database operations |
| LINQ | Data querying |

### Frontend
| Technology | Purpose |
|------------|---------|
| Bootstrap 5 | UI framework |
| jQuery | JavaScript library |
| Bootstrap Icons | Icon library |
| HTML5/CSS3 | Structure and styling |

### Database
| Technology | Purpose |
|------------|---------|
| Azure SQL Database | Production database |
| SQL Server | Local development |
| Entity Framework Migrations | Database version control |

### Azure Services
| Service | Purpose |
|---------|---------|
| Azure App Service | Web application hosting |
| Azure SQL Database | Cloud database |
| Azure Blob Storage | Image storage |
| Azure Application Insights | Monitoring |

---

## ?? Project Structure

```
CloudPART3-KONES/
?
??? Controllers/
?   ??? BookingsController.cs      # Booking management with filters
?   ??? EventsController.cs         # Event CRUD operations
?   ??? VenuesController.cs         # Venue management with images
?   ??? HomeController.cs           # Dashboard and statistics
?
??? Models/
?   ??? Booking.cs                  # Booking entity
?   ??? Event.cs                    # Event entity
?   ??? Venue.cs                    # Venue entity
?   ??? EventType.cs                # Event type lookup
?   ??? BookingViewModel.cs         # Booking view model
?   ??? BookingFilterViewModel.cs   # Advanced filters model
?
??? Views/
?   ??? Bookings/
?   ?   ??? Index.cshtml            # Main page with filters
?   ?   ??? Create.cshtml           # Create booking form
?   ?   ??? Delete.cshtml           # Cancel booking
?   ??? Events/
?   ?   ??? Index.cshtml            # Events list
?   ?   ??? Create.cshtml           # Create event
?   ?   ??? Delete.cshtml           # Delete event
?   ??? Venues/
?   ?   ??? Index.cshtml            # Venues list
?   ?   ??? Create.cshtml           # Add venue with image
?   ?   ??? Delete.cshtml           # Delete venue
?   ??? Home/
?       ??? Index.cshtml            # Dashboard
?
??? Data/
?   ??? ApplicationDbContext.cs     # Database context
?
??? Services/
?   ??? ValidationService.cs        # Booking validation
?   ??? BlobStorageService.cs       # Azure storage operations
?
??? Migrations/                      # EF Core migrations
??? wwwroot/                        # Static files
?   ??? css/
?   ??? js/
?   ??? images/
?
??? appsettings.json                # Configuration
??? Program.cs                      # Application entry point
??? README.md                       # Documentation
```

---

## ??? Database Schema

### Tables

#### EventTypes (Lookup Table)
| Column | Type | Description |
|--------|------|-------------|
| EventTypeID | INT (PK) | Primary key |
| CategoryName | NVARCHAR(50) | Conference, Wedding, etc. |
| Description | NVARCHAR(200) | Category description |
| IsActive | BIT | Active status |
| DisplayOrder | INT | Sort order |

#### Venues
| Column | Type | Description |
|--------|------|-------------|
| VenueID | INT (PK) | Primary key |
| VenueName | NVARCHAR(100) | Venue name |
| Location | NVARCHAR(200) | Address |
| Capacity | INT | Maximum attendees |
| AvailabilityStatus | NVARCHAR(20) | Available/Maintenance/Booked/Limited |
| OperatingHours | NVARCHAR(200) | Business hours |
| ImageUrl | NVARCHAR(500) | Azure storage URL |

#### Events
| Column | Type | Description |
|--------|------|-------------|
| EventID | INT (PK) | Primary key |
| EventName | NVARCHAR(100) | Event name |
| EventTypeID | INT (FK) | Links to EventTypes |
| EventDate | DATETIME | Event date/time |
| DurationHours | INT | Duration in hours |
| OrganizerName | NVARCHAR(100) | Organizer name |

#### Bookings
| Column | Type | Description |
|--------|------|-------------|
| BookingID | INT (PK) | Primary key |
| VenueID | INT (FK) | Links to Venues |
| EventID | INT (FK) | Links to Events |
| BookingDate | DATETIME | Booking creation date |
| Status | NVARCHAR(20) | Confirmed/Cancelled/Completed |

---

## ?? Setup Instructions

### Prerequisites
- Visual Studio 2022 or later
- .NET SDK 6.0 or later
- Azure Subscription (for deployment)
- SQL Server (local development)

### Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/Kone-M/CloudPART3-KONES.git
cd CloudPART3-KONES
```

2. **Update connection string** in `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VenueBookingDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

3. **Install dependencies**
```bash
dotnet restore
```

4. **Run database migrations**
```bash
dotnet ef database update
```

5. **Run the application**
```bash
dotnet run
```

6. **Open browser** and navigate to:
```
https://localhost:5001
```

### Azure Deployment

1. **Create Azure App Service**
```bash
az appservice plan create --name VenueBookingPlan --resource-group VenueBookingRG --sku F1 --is-linux
az webapp create --name venuebookingapp --resource-group VenueBookingRG --plan VenueBookingPlan --runtime "DOTNET:6.0"
```

2. **Create Azure SQL Database**
```bash
az sql server create --name venuebooking-server --resource-group VenueBookingRG --location eastus --admin-user adminuser --admin-password YourPassword123!
az sql db create --resource-group VenueBookingRG --server venuebooking-server --name VenueBookingDB --service-objective S0
```

3. **Create Azure Storage Account** (for images)
```bash
az storage account create --name venuebookingstorage --resource-group VenueBookingRG --location eastus --sku Standard_LRS
az storage container create --name venueimages --account-name venuebookingstorage
```

4. **Deploy the application**
```bash
dotnet publish -c Release -o ./publish
az webapp deployment source config-zip --resource-group VenueBookingRG --name venuebookingapp --src ./publish.zip
```

---

## ?? Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Database=your-db;User ID=your-user;Password=your-password;"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=yourstorage;AccountKey=yourkey;",
    "VenueImagesContainer": "venueimages"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

---

## ?? API Endpoints

### Statistics Endpoints
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Home/GetStatistics` | GET | System-wide statistics |
| `/Home/GetRecentBookings` | GET | Recent 10 bookings |
| `/Home/GetVenuesByStatus` | GET | Venues grouped by status |
| `/Home/GetBookingsByStatus` | GET | Bookings grouped by status |
| `/Home/GetEventTypes` | GET | All event types |

### Booking Endpoints
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Bookings/Index` | GET | Main page with filters |
| `/Bookings/Create` | GET/POST | Create new booking |
| `/Bookings/Delete/{id}` | GET/POST | Cancel booking |
| `/Bookings/Details/{id}` | GET | Booking details |

### Venue Endpoints
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Venues` | GET | List all venues |
| `/Venues/Create` | GET/POST | Add new venue |
| `/Venues/Edit/{id}` | GET/POST | Edit venue |
| `/Venues/Delete/{id}` | GET/POST | Delete venue |

---

## ?? Testing

### Sample Test Data

**Venues:**
```sql
INSERT INTO Venues (VenueName, Location, Capacity, AvailabilityStatus, OperatingHours)
VALUES 
('Grand Conference Hall', '123 Business Ave', 500, 'Available', 'Mon-Fri 8AM-10PM'),
('Garden Wedding Pavilion', '45 Park Lane', 200, 'Available', 'Daily 10AM-9PM'),
('Tech Workshop Center', '89 Innovation Drive', 50, 'Available', 'Mon-Fri 9AM-9PM');
```

**Event Types (Pre-seeded):**
- Conference
- Wedding
- Workshop
- Concert
- Corporate Event
- Birthday Party
- Exhibition
- Other

---

## ?? Troubleshooting

### Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| **Database connection error** | Check connection string and firewall rules |
| **Image upload fails** | Verify Azure Storage container permissions |
| **Double-booking not working** | Check unique index on VenueID + BookingDate |
| **Filters not showing** | Ensure BookingFilterViewModel is used |
| **Migration errors** | Run `dotnet ef migrations add` and `dotnet ef database update` |

---

## ?? Contributors

| Name | Role | Contact |
|------|------|---------|
| Kone Moshapo | Full Stack Developer | [GitHub](https://github.com/Kone-M) |

---

## ?? Acknowledgments

- Microsoft Azure Documentation
- ASP.NET Core Community
- Bootstrap Team
- IIE Rosebank College - CLDV7111W Module

---

## ?? License

This project is for educational purposes as part of the CLDV7111W POE (Proof of Event) assignment.

---

## ?? Links

- **GitHub Repository:** https://github.com/Kone-M/CloudPART3-KONES
- **Live Application:** https://cloudpart220260612150138-cjaregbga8ghcdah.canadacentral-01.azurewebsites.net/
- **Azure Portal:** https://portal.azure.com

---

## ?? Support

For issues or questions:
- Create an issue on GitHub
- Contact: kones@gmail.com

---

## ?? Version History

| Version | Date | Changes |
|---------|------|---------|
| v3.0 | June 2026 | Advanced filtering, Event Types, Venue availability |
| v2.0 | May 2026 | Azure integration, Image upload |
| v1.0 | April 2026 | Basic CRUD operations |

---

## ?? Future Enhancements

- [ ] User authentication and authorization
- [ ] Email notifications for bookings
- [ ] Calendar view for bookings
- [ ] Payment integration
- [ ] Mobile responsive PWA
- [ ] Export reports to PDF/Excel
- [ ] Real-time availability calendar
- [ ] QR code check-in system

---

**Built with ?? using ASP.NET Core and Azure**
