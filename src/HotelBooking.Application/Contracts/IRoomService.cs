using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Room;
using HotelBooking.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Contracts
{
    public interface IRoomService
    {
        Task<PaginatedResponse<RoomDTO>> GetAllRoomsAsync(int pageIndex, int pageSize);
        Task<RoomDTO> GetRoomByIdAsync(Guid id);
        Task<RoomDTO> GetRoomByNumberAsync(string roomNumber);
        Task<PaginatedResponse<RoomDTO>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate,
            int pageIndex, int pageSize);
        Task<PaginatedResponse<RoomDTO>> GetRoomsByTypeAsync(string roomType, int pageIndex, int pageSize);
        Task<RoomDTO> CreateRoomAsync(RoomCreateDTO roomCreateDTO);
        Task UpdateRoomAsync( Guid id, RoomUpdateDTO roomUpdateDTO);
        Task DeleteRoomAsync(Guid id);
        Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetAvailabilityByRoomTypeAsync(DateTime startDate, 
            DateTime endDate);
        Task<RoomRevenueStatsDTO> GetRoomRevenueStatsAsync(DateTime startDate, DateTime endDate);
    }
}
