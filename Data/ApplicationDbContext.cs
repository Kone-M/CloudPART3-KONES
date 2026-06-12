using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using VenueBookingSystem.Models;

namespace VenueBookingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }  // New
        public DbSet<BookingViewModel> vw_EnhancedBookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Venue configuration with new availability fields
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(e => e.VenueID);
                entity.Property(e => e.VenueName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Capacity).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // New availability fields
                entity.Property(e => e.AvailabilityStatus)
                      .HasMaxLength(20)
                      .HasDefaultValue("Available");
                entity.Property(e => e.OperatingHours).HasMaxLength(200);
                entity.Property(e => e.LastMaintenanceDate).HasColumnType("date");
                entity.Property(e => e.NextAvailableDate).HasColumnType("datetime");

                entity.ToTable("Venues", "dbo");
            });

            // Event configuration with EventType relationship
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventID);
                entity.Property(e => e.EventName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.EventDate).IsRequired();
                entity.Property(e => e.DurationHours).IsRequired();
                entity.Property(e => e.OrganizerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                // New EventType foreign key
                entity.Property(e => e.EventTypeID);

                // Configure relationship with EventType
                entity.HasOne(e => e.EventType)
                      .WithMany()
                      .HasForeignKey(e => e.EventTypeID)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.ToTable("Events", "dbo");
            });

            // EventType configuration (new table)
            modelBuilder.Entity<EventType>(entity =>
            {
                entity.HasKey(e => e.EventTypeID);
                entity.Property(e => e.CategoryName)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.ToTable("EventTypes", "dbo");

                // Seed initial Event Types
                entity.HasData(
                    new EventType { EventTypeID = 1, CategoryName = "Conference", Description = "Business conferences and seminars", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.Now },
                    new EventType { EventTypeID = 2, CategoryName = "Wedding", Description = "Wedding ceremonies and receptions", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.Now },
                    new EventType { EventTypeID = 3, CategoryName = "Workshop", Description = "Training workshops and classes", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.Now },
                    new EventType { EventTypeID = 4, CategoryName = "Concert", Description = "Music concerts and performances", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.Now },
                    new EventType { EventTypeID = 5, CategoryName = "Corporate Event", Description = "Corporate meetings and events", DisplayOrder = 5, IsActive = true, CreatedAt = DateTime.Now },
                    new EventType { EventTypeID = 6, CategoryName = "Birthday Party", Description = "Birthday celebrations", DisplayOrder = 6, IsActive = true, CreatedAt = DateTime.Now },
                    new EventType { EventTypeID = 7, CategoryName = "Exhibition", Description = "Art and trade exhibitions", DisplayOrder = 7, IsActive = true, CreatedAt = DateTime.Now },
                    new EventType { EventTypeID = 8, CategoryName = "Other", Description = "Other types of events", DisplayOrder = 8, IsActive = true, CreatedAt = DateTime.Now }
                );
            });

            // Booking configuration (keeping existing)
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingID);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Confirmed");
                entity.Property(e => e.SpecialRequests).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => new { e.VenueID, e.BookingDate })
                      .IsUnique()
                      .HasDatabaseName("UQ_Booking_DateTime");

                entity.HasOne(e => e.Venue)
                      .WithMany(e => e.Bookings)
                      .HasForeignKey(e => e.VenueID)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Event)
                      .WithMany(e => e.Bookings)
                      .HasForeignKey(e => e.EventID)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.ToTable("Bookings", "dbo");
            });

            // View configuration (keeping existing)
            modelBuilder.Entity<BookingViewModel>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_EnhancedBookings", "dbo");
            });
        }
    }
}