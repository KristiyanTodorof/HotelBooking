using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.BookingDetails
{
    public class BookingDetailsDTO
    {
        public Guid Id { get; set; }
        public byte Adults { get; set; }
        public byte Children { get; set; }
        public byte Babies { get; set; }
        public string Meal { get; set; }
        public byte RequiredCarParkingSpaces { get; set; }
        public byte TotalOfSpecialRequests { get; set; }
    }
}
