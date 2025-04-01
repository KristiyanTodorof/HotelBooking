using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.BookingDetails
{
    public class BookingDetailsCreateDTO
    {
        [Required]
        [Range(1, 20)]
        public byte Adults { get; set; }
        [Range(1, 10)]
        public byte Children { get; set; }
        [Range(1, 5)]
        public byte Babies { get; set; }
        [Required]
        [StringLength(20)]
        public string Meal { get; set; }
        [Range(0, 5)]
        public byte RequiredCarParkingSpaces { get; set; }
        [Range(0, 10)]
        public byte TotalOfSpecialRequests { get; set; }
    }
}
