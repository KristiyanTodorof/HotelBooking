using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class RoomRevenueItemDTO
    {
        public string RoomNumber { get; set; }
        public Guid RoomId { get; set; }
        public string RoomType { get; set; }
        public double Revenue { get; set; }
        public int Bookings { get; set; }
        public double OccupancyRate { get; set; }
    }
}
