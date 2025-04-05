using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class OccupancyStatsDTO
    {
        public double OccupancyRate { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public double AverageDailyRate { get; set; }
        public double RevPAR { get; set; } // Revenue Per Available Room
        public Dictionary<string, int> OccupancyByRoomType { get; set; }
    }
}
