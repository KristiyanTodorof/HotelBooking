using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Room
{
    public class RoomDTO
    {
        public Guid Id { get; set; }
        public string RoomNumber { get; set; }
        public string ReservedRoomType { get; set; }
        public string AssignedRoomType { get; set; }
        public byte Capacity { get; set; }
        public double BaseRate { get; set; }
    }
}
