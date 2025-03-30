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
    public class RoomRepository : Repository<Room, Guid>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate)
        {
            var bookedRoomIds = await _context.Bookings
                .Where(b => b.ReservationStatus == "Confirmed" && !b.IsCancelled &&
                        b.ArrivalDateYear == startDate.Year &&
                        GetMonthNumber(b.ArrivalDateMonth) >= startDate.Month &&
                        GetMonthNumber(b.ArrivalDateMonth) <= endDate.Month)
                .Select(b => b.RoomId)
                .Distinct()
                .ToListAsync();

            return await _context.Rooms
                .Where(r => !bookedRoomIds.Contains(r.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetRoomsByTypeAsync(string roomType)
        {
            return await _context.Rooms
                .Where(r => r.ReservedRoomType == roomType)
                .ToListAsync();
        }

        public async Task<Room> GetByRoomNumberAsync(string roomNumber)
        {
            return await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
        }

        private int GetMonthNumber(string monthName)
        {
            return DateTime.ParseExact(monthName, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;
        }
    }
}
