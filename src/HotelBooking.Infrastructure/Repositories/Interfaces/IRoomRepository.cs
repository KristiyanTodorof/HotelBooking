using HotelBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories.Interfaces
{
    public interface IRoomRepository : IRepository<Room, Guid>
    {
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Room>> GetRoomsByTypeAsync(string roomType);
        Task<Room> GetByRoomNumberAsync(string roomNumber);
    }
}
