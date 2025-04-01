using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Room
{
    public class RoomCreateDTO
    {
        [Required]
        [MaxLength(10)]
        public string RoomNumber { get; set; }
        [Required]
        [MaxLength(2)]
        public string ReservedRoomType { get; set; }
        [Required]
        [MaxLength(2)]
        public string AssignedRoomType { get; set; }
        [Required]
        public byte Capacity { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double BaseRate { get; set; }
    }
}
