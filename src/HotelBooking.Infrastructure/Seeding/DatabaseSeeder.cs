using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Seeding
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Apply any pending migrations
                await _context.Database.MigrateAsync();

                // Seed data in order of dependencies
                await SeedRolesAsync();
                await SeedUsersAsync();
                await SeedRoomsAsync();
                await SeedSalesChannelsAsync();
                await SeedGuestsAsync();
                await SeedBookingsAsync();

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            if (await _context.Roles.AnyAsync())
            {
                return; // Roles already seeded
            }

            _logger.LogInformation("Seeding roles");

            var roles = new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Name = "Admin",
                    Description = "Administrator with full access to all features",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CanManageBookings = true,
                    CanManageRooms = true,
                    CanManageUsers = true,
                    CanAccessReports = true,
                    CanManagePayments = true
                },
                new ApplicationRole
                {
                    Name = "Manager",
                    Description = "Hotel manager with access to most features",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CanManageBookings = true,
                    CanManageRooms = true,
                    CanManageUsers = false,
                    CanAccessReports = true,
                    CanManagePayments = true
                },
                new ApplicationRole
                {
                    Name = "Receptionist",
                    Description = "Front desk staff with limited access",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CanManageBookings = true,
                    CanManageRooms = false,
                    CanManageUsers = false,
                    CanAccessReports = false,
                    CanManagePayments = true
                },
                new ApplicationRole
                {
                    Name = "Guest",
                    Description = "Hotel guest with access to own bookings only",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CanManageBookings = false,
                    CanManageRooms = false,
                    CanManageUsers = false,
                    CanAccessReports = false,
                    CanManagePayments = false
                }
            };

            foreach (var role in roles)
            {
                await _roleManager.CreateAsync(role);
            }
        }

        private async Task SeedUsersAsync()
        {
            if (await _context.Users.AnyAsync())
            {
                return; // Users already seeded
            }

            _logger.LogInformation("Seeding users");

            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = "admin@hotel.com",
                Email = "admin@hotel.com",
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                PhoneNumber = "1234567890",
                PhoneNumberConfirmed = true,
                Address = "123 Admin Street, Admin City",
                Title = "Mr.", // Add title
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
                IsStaff = true,
                Department = "Administration",
                Position = "System Administrator"
            };

            await _userManager.CreateAsync(adminUser, "Admin@123");
            await _userManager.AddToRoleAsync(adminUser, "Admin");

            // Create manager user
            var managerUser = new ApplicationUser
            {
                UserName = "manager@hotel.com",
                Email = "manager@hotel.com",
                FirstName = "Hotel",
                LastName = "Manager",
                EmailConfirmed = true,
                PhoneNumber = "1234567891",
                PhoneNumberConfirmed = true,
                Address = "456 Manager Road, Manager City",
                Title = "Ms.", // Add title
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
                IsStaff = true,
                Department = "Management",
                Position = "Hotel Manager"
            };

            await _userManager.CreateAsync(managerUser, "Manager@123");
            await _userManager.AddToRoleAsync(managerUser, "Manager");

            // Create receptionist user
            var receptionistUser = new ApplicationUser
            {
                UserName = "receptionist@hotel.com",
                Email = "receptionist@hotel.com",
                FirstName = "Front",
                LastName = "Desk",
                EmailConfirmed = true,
                PhoneNumber = "1234567892",
                PhoneNumberConfirmed = true,
                Address = "789 Reception Avenue, Reception Town",
                Title = "Mrs.", // Add title
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
                IsStaff = true,
                Department = "Front Desk",
                Position = "Receptionist"
            };

            await _userManager.CreateAsync(receptionistUser, "Receptionist@123");
            await _userManager.AddToRoleAsync(receptionistUser, "Receptionist");

            // Create guest user
            var guestUser = new ApplicationUser
            {
                UserName = "guest@example.com",
                Email = "guest@example.com",
                FirstName = "Test",
                LastName = "Guest",
                EmailConfirmed = true,
                PhoneNumber = "1234567893",
                PhoneNumberConfirmed = true,
                Address = "101 Guest Place, Guest Village",
                Title = "Dr.", // Add title
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
                IsStaff = false,
                Department = "", // Add empty string for non-null
                Position = "" // Add empty string for non-null
            };

            await _userManager.CreateAsync(guestUser, "Guest@123");
            await _userManager.AddToRoleAsync(guestUser, "Guest");
        }

        private async Task SeedRoomsAsync()
        {
            if (await _context.Rooms.AnyAsync())
            {
                return; // Rooms already seeded
            }

            _logger.LogInformation("Seeding rooms");

            var rooms = new List<Room>
            {
                // Standard Rooms
                new Room { RoomNumber = "101", ReservedRoomType = "A", AssignedRoomType = "A", Capacity = 2, BaseRate = 100.00 },
                new Room { RoomNumber = "102", ReservedRoomType = "A", AssignedRoomType = "A", Capacity = 2, BaseRate = 100.00 },
                new Room { RoomNumber = "103", ReservedRoomType = "A", AssignedRoomType = "A", Capacity = 2, BaseRate = 100.00 },
                new Room { RoomNumber = "104", ReservedRoomType = "A", AssignedRoomType = "A", Capacity = 2, BaseRate = 100.00 },
                new Room { RoomNumber = "105", ReservedRoomType = "A", AssignedRoomType = "A", Capacity = 2, BaseRate = 100.00 },
                
                // Deluxe Rooms
                new Room { RoomNumber = "201", ReservedRoomType = "B", AssignedRoomType = "B", Capacity = 2, BaseRate = 150.00 },
                new Room { RoomNumber = "202", ReservedRoomType = "B", AssignedRoomType = "B", Capacity = 2, BaseRate = 150.00 },
                new Room { RoomNumber = "203", ReservedRoomType = "B", AssignedRoomType = "B", Capacity = 2, BaseRate = 150.00 },
                
                // Family Rooms
                new Room { RoomNumber = "301", ReservedRoomType = "C", AssignedRoomType = "C", Capacity = 4, BaseRate = 200.00 },
                new Room { RoomNumber = "302", ReservedRoomType = "C", AssignedRoomType = "C", Capacity = 4, BaseRate = 200.00 },
                
                // Suites
                new Room { RoomNumber = "401", ReservedRoomType = "D", AssignedRoomType = "D", Capacity = 2, BaseRate = 300.00 },
                new Room { RoomNumber = "402", ReservedRoomType = "D", AssignedRoomType = "D", Capacity = 2, BaseRate = 300.00 }
            };

            await _context.Rooms.AddRangeAsync(rooms);
            await _context.SaveChangesAsync();
        }

        private async Task SeedSalesChannelsAsync()
        {
            if (await _context.SalesChannels.AnyAsync())
            {
                return; // Sales channels already seeded
            }

            _logger.LogInformation("Seeding sales channels");

            var salesChannels = new List<SalesChannel>
            {
                new SalesChannel { MarketSegment = "Direct", DistributionChannel = "Direct", Agent = "None", Company = "" },
                new SalesChannel { MarketSegment = "Corporate", DistributionChannel = "Corporate", Agent = "None", Company = "ABC Corporation" },
                new SalesChannel { MarketSegment = "Corporate", DistributionChannel = "Corporate", Agent = "None", Company = "XYZ Inc." },
                new SalesChannel { MarketSegment = "Online TA", DistributionChannel = "TA/TO", Agent = "Booking.com", Company = "" },
                new SalesChannel { MarketSegment = "Online TA", DistributionChannel = "TA/TO", Agent = "Expedia", Company = "" },
                new SalesChannel { MarketSegment = "Offline TA", DistributionChannel = "TA/TO", Agent = "Local Travel", Company = "" }
            };

            await _context.SalesChannels.AddRangeAsync(salesChannels);
            await _context.SaveChangesAsync();
        }

        private async Task SeedGuestsAsync()
        {
            if (await _context.Guests.AnyAsync())
            {
                return; // Guests already seeded
            }

            _logger.LogInformation("Seeding guests");

            // Get the guest user to link
            var guestUser = await _userManager.FindByEmailAsync("guest@example.com");

            var guests = new List<Guest>
            {
                new Guest
                {
                    Name = "John Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "1234567001",
                    Country = "USA",
                    IsRepeatedGuest = false,
                    PreviousCancellations = 0,
                    PreviousBookingsNotCancelled = 0,
                    CustomerType = "Transient"
                },
                new Guest
                {
                    Name = "Jane Smith",
                    Email = "jane.smith@example.com",
                    PhoneNumber = "1234567002",
                    Country = "Canada",
                    IsRepeatedGuest = true,
                    PreviousCancellations = 1,
                    PreviousBookingsNotCancelled = 2,
                    CustomerType = "Transient"
                },
                new Guest
                {
                    Name = "Bob Johnson",
                    Email = "bob.johnson@company.com",
                    PhoneNumber = "1234567003",
                    Country = "UK",
                    IsRepeatedGuest = false,
                    PreviousCancellations = 0,
                    PreviousBookingsNotCancelled = 0,
                    CustomerType = "Contract"
                },
                new Guest
                {
                    Name = "Test Guest",
                    Email = "guest@example.com",
                    PhoneNumber = "1234567893",
                    Country = "USA",
                    IsRepeatedGuest = false,
                    PreviousCancellations = 0,
                    PreviousBookingsNotCancelled = 0,
                    CustomerType = "Transient",
                    ApplicationUserId = guestUser?.Id
                }
            };

            await _context.Guests.AddRangeAsync(guests);
            await _context.SaveChangesAsync();
        }

        private async Task SeedBookingsAsync()
        {
            if (await _context.Bookings.AnyAsync())
            {
                return; // Bookings already seeded
            }

            _logger.LogInformation("Seeding bookings");

            // Get existing data to reference
            var guests = await _context.Guests.ToListAsync();
            var rooms = await _context.Rooms.ToListAsync();
            var salesChannels = await _context.SalesChannels.ToListAsync();

            // Create booking details
            var bookingDetails1 = new BookingDetails
            {
                Adults = 2,
                Children = 0,
                Babies = 0,
                Meal = "BB",
                RequiredCarParkingSpaces = 1,
                TotalOfSpecialRequests = 1
            };

            var bookingDetails2 = new BookingDetails
            {
                Adults = 2,
                Children = 1,
                Babies = 0,
                Meal = "HB",
                RequiredCarParkingSpaces = 0,
                TotalOfSpecialRequests = 2
            };

            var bookingDetails3 = new BookingDetails
            {
                Adults = 1,
                Children = 0,
                Babies = 0,
                Meal = "BB",
                RequiredCarParkingSpaces = 0,
                TotalOfSpecialRequests = 0
            };

            await _context.BookingDetails.AddRangeAsync(new[] { bookingDetails1, bookingDetails2, bookingDetails3 });
            await _context.SaveChangesAsync();

            // Create payments
            var payment1 = new Payment
            {
                DepositType = "Credit Card",
                CreditCard = "4111111111111111", // This would be encrypted in production
                CardExpiryDate = "12/25"
            };

            var payment2 = new Payment
            {
                DepositType = "Credit Card",
                CreditCard = "5555555555554444", // This would be encrypted in production
                CardExpiryDate = "06/24"
            };

            await _context.Payments.AddRangeAsync(new[] { payment1, payment2 });
            await _context.SaveChangesAsync();

            // Create bookings
            var bookings = new List<Booking>
            {
                new Booking
                {
                    Hotel = "Grand Hotel",
                    IsCancelled = false,
                    LeadTime = 30,
                    ArrivalDateYear = DateTime.Now.Year,
                    ArrivalDateMonth = DateTime.Now.AddDays(30).ToString("MMMM"),
                    ArrivalDateWeekNumber = (byte)GetIso8601WeekOfYear(DateTime.Now.AddDays(30)),
                    ArrivalDateDayOfMonth = (byte)DateTime.Now.AddDays(30).Day,
                    StaysInWeekendNights = 2,
                    StaysInWeekNights = 3,
                    BookingChanges = 0,
                    DaysInWaitingList = 0,
                    AverageDailyRate = 100.00,
                    ReservationStatus = "Confirmed",
                    ReservationStatusDate = DateTimeOffset.UtcNow,
                    GuestId = guests[0].Id,
                    RoomId = rooms[0].Id,
                    SalesChannelId = salesChannels[0].Id,
                    BookingDetailsId = bookingDetails1.Id,
                    PaymentId = payment1.Id
                },
                new Booking
                {
                    Hotel = "Grand Hotel",
                    IsCancelled = false,
                    LeadTime = 60,
                    ArrivalDateYear = DateTime.Now.Year,
                    ArrivalDateMonth = DateTime.Now.AddDays(60).ToString("MMMM"),
                    ArrivalDateWeekNumber = (byte)GetIso8601WeekOfYear(DateTime.Now.AddDays(60)),
                    ArrivalDateDayOfMonth = (byte)DateTime.Now.AddDays(60).Day,
                    StaysInWeekendNights = 0,
                    StaysInWeekNights = 3,
                    BookingChanges = 1,
                    DaysInWaitingList = 0,
                    AverageDailyRate = 150.00,
                    ReservationStatus = "Confirmed",
                    ReservationStatusDate = DateTimeOffset.UtcNow,
                    GuestId = guests[1].Id,
                    RoomId = rooms[5].Id,
                    SalesChannelId = salesChannels[3].Id,
                    BookingDetailsId = bookingDetails2.Id,
                    PaymentId = payment2.Id
                },
                new Booking
                {
                    Hotel = "Grand Hotel",
                    IsCancelled = true,
                    LeadTime = 15,
                    ArrivalDateYear = DateTime.Now.Year,
                    ArrivalDateMonth = DateTime.Now.AddDays(-15).ToString("MMMM"),
                    ArrivalDateWeekNumber = (byte)GetIso8601WeekOfYear(DateTime.Now.AddDays(-15)),
                    ArrivalDateDayOfMonth = (byte)DateTime.Now.AddDays(-15).Day,
                    StaysInWeekendNights = 0,
                    StaysInWeekNights = 2,
                    BookingChanges = 0,
                    DaysInWaitingList = 0,
                    AverageDailyRate = 100.00,
                    ReservationStatus = "Cancelled",
                    ReservationStatusDate = DateTimeOffset.UtcNow.AddDays(-20),
                    GuestId = guests[2].Id,
                    RoomId = rooms[2].Id,
                    SalesChannelId = salesChannels[1].Id,
                    BookingDetailsId = bookingDetails3.Id,
                    PaymentId = null
                }
            };

            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();
        }

        private int GetIso8601WeekOfYear(DateTime date)
        {
            // Use ISO 8601 definition of week number
            var day = (int)System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day == 0) day = 7; // Convert Sunday from 0 to 7

            // Add 3 and subtract the day number
            date = date.AddDays(4 - day);

            // Get the first day of the year
            var firstDayOfYear = new DateTime(date.Year, 1, 1);

            // Calculate number of days between date and first day
            var dayOfYear = (date - firstDayOfYear).Days + 1;

            // Return the week number
            return (dayOfYear - 1) / 7 + 1;
        }
    }
}
