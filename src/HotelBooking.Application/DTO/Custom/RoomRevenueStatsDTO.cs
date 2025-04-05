using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class RoomRevenueStatsDTO
    {
        public double TotalRevenue { get; set; }
        public Dictionary<string, double> RevenueByRoomType { get; set; }
        public List<RoomRevenueItemDTO> TopPerformingRooms { get; set; }
        public double AverageRoomRate { get; set; }
    }
}
