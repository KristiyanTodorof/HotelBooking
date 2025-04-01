using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Room
{
    public class RoomUpdateDTO
    {
        [MaxLength(10)]
        public string RoomNumber { get; set; }

        [MaxLength(2)]
        public string ReservedRoomType { get; set; }

        [MaxLength(2)]
        public string AssignedRoomType { get; set; }

        [Range(1, 10)]
        public byte? Capacity { get; set; }

        [Range(0, double.MaxValue)]
        public double? BaseRate { get; set; }
    }
}
