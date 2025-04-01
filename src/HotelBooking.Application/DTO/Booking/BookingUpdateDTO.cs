using HotelBooking.Application.DTO.BookingDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Booking
{
    public class BookingUpdateDTO
    {
        public string Hotel {  get; set; }
        public bool? IsCancelled { get; set; }
        public string ReservationStatus { get; set; }
        public double? AverageDailyRate { get; set; }
        public byte? BookingChanges { get; set; }
        public Guid? RoomId { get; set; }
        public BookingDetailsUpdateDTO BookingDetails { get; set; }
    }
}
