using HotelBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories.Interfaces
{
    public interface IBookingRepository : IRepository<Booking, Guid>
    {
        Task<IEnumerable<Booking>> GetBookingsByGuestIdAsync(Guid guestId);
        Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Booking>> GetActiveBookingAsync();
        Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startDate, DateTime endDate);
    }
}
