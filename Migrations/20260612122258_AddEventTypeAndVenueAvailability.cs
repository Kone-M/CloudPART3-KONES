using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cloudpart2.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeAndVenueAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "EventTypes",
                schema: "dbo",
                columns: table => new
                {
                    EventTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                schema: "dbo",
                columns: table => new
                {
                    VenueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AvailabilityStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    LastMaintenanceDate = table.Column<DateTime>(type: "date", nullable: true),
                    NextAvailableDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    OperatingHours = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.VenueID);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "dbo",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    OrganizerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventTypeID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventID);
                    table.ForeignKey(
                        name: "FK_Events_EventTypes_EventTypeID",
                        column: x => x.EventTypeID,
                        principalSchema: "dbo",
                        principalTable: "EventTypes",
                        principalColumn: "EventTypeID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "dbo",
                columns: table => new
                {
                    BookingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueID = table.Column<int>(type: "int", nullable: false),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Confirmed"),
                    SpecialRequests = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingID);
                    table.ForeignKey(
                        name: "FK_Bookings_Events_EventID",
                        column: x => x.EventID,
                        principalSchema: "dbo",
                        principalTable: "Events",
                        principalColumn: "EventID");
                    table.ForeignKey(
                        name: "FK_Bookings_Venues_VenueID",
                        column: x => x.VenueID,
                        principalSchema: "dbo",
                        principalTable: "Venues",
                        principalColumn: "VenueID");
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "EventTypes",
                columns: new[] { "EventTypeID", "CategoryName", "CreatedAt", "Description", "DisplayOrder", "IsActive" },
                values: new object[,]
                {
                    { 1, "Conference", new DateTime(2026, 6, 12, 14, 22, 57, 94, DateTimeKind.Local).AddTicks(3821), "Business conferences and seminars", 1, true },
                    { 2, "Wedding", new DateTime(2026, 6, 12, 14, 22, 57, 94, DateTimeKind.Local).AddTicks(3823), "Wedding ceremonies and receptions", 2, true },
                    { 3, "Workshop", new DateTime(2026, 6, 12, 14, 22, 57, 94, DateTimeKind.Local).AddTicks(3825), "Training workshops and classes", 3, true },
                    { 4, "Concert", new DateTime(2026, 6, 12, 14, 22, 57, 94, DateTimeKind.Local).AddTicks(3827), "Music concerts and performances", 4, true },
                    { 5, "Corporate Event", new DateTime(2026, 6, 12, 14, 22, 57, 94, DateTimeKind.Local).AddTicks(3829), "Corporate meetings and events", 5, true },
                    { 6, "Birthday Party", new DateTime(2026, 6, 12, 14, 22, 57, 94, DateTimeKind.Local).AddTicks(3830), "Birthday celebrations", 6, true },
                    { 7, "Exhibition", new DateTime(2026, 6, 12, 14, 22, 57, 94, DateTimeKind.Local).AddTicks(3832), "Art and trade exhibitions", 7, true },
                    { 8, "Other", new DateTime(2026, 6, 12, 14, 22, 57, 94, DateTimeKind.Local).AddTicks(3834), "Other types of events", 8, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventID",
                schema: "dbo",
                table: "Bookings",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "UQ_Booking_DateTime",
                schema: "dbo",
                table: "Bookings",
                columns: new[] { "VenueID", "BookingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeID",
                schema: "dbo",
                table: "Events",
                column: "EventTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Venues",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EventTypes",
                schema: "dbo");
        }
    }
}
