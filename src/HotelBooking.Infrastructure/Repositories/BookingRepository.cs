using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Data;
using HotelBooking.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking, Guid>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetActiveBookingAsync()
        {
            return await _context.Bookings
               .Where(b => b.ReservationStatus == "Confirmed" && !b.IsCancelled)
               .Include(b => b.Guest)
               .Include(b => b.Room)
               .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            DateTimeOffset startDateOffset = new DateTimeOffset(startDate);
            DateTimeOffset endDateOffset = new DateTimeOffset(endDate);

            return await _context.Bookings
                .Where(b => b.ReservationStatusDate >= startDateOffset && b.ReservationStatusDate <= endDateOffset)
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByGuestIdAsync(Guid guestId)
        {
            return await _context.Bookings
                .Where(b => b.GuestId == guestId)
                .Include(b => b.BookingDetails)
                .Include(b => b.Room)
                .OrderByDescending(b => b.ReservationStatusDate)
                .ToListAsync();
        }

        public async Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startDate, DateTime endDate)
        {
            return !(await _context.Bookings
                .AnyAsync(b => b.RoomId == roomId &&
                          b.ReservationStatus == "Confirmed" &&
                          !b.IsCancelled &&
                          // Check for date overlap
                          (b.ArrivalDateYear > startDate.Year ||
                           (b.ArrivalDateYear == startDate.Year &&
                            GetMonthNumber(b.ArrivalDateMonth) > startDate.Month) ||
                           (b.ArrivalDateYear == startDate.Year &&
                            GetMonthNumber(b.ArrivalDateMonth) == startDate.Month &&
                            b.ArrivalDateDayOfMonth >= startDate.Day))));
        }
        private int GetMonthNumber(string monthName)
        {
            return DateTime.ParseExact(monthName, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;
        }
    }
}
