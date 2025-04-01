using HotelBooking.Application.DTO.BookingDetails;
using HotelBooking.Application.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Booking
{
    public class BookingDTO
    {
        public Guid Id { get; set; }
        public string Hotel { get; set; }
        public bool IsCancelled { get; set; }
        public int LeadTime { get; set; }
        public int ArrivalDateYear { get; set; }
        public string ArrivalDateMonth { get; set; }
        public byte ArrivalDateDayOfMonth { get; set; }
        public byte ArrivalDateWeekNumber { get; set; }
        public byte SraysInWeekendNights { get; set; }
        public byte StaysInWeekNights { get; set; }
        public byte BookingChanges { get; set; }
        public int DaysInWaitingList { get; set; }
        public double AverageDailyRate { get; set; }
        public string ReservationStatus { get; set; }
        public DateTimeOffset DateCreated { get; set; }

        public string GuestName { get; set; }
        public string RoomNumber { get; set; }
        public BookingDetailsDTO BookingDetails { get; set; }
        public PaymentDTO Payment { get; set; }
        public string SalesChannel { get; set; }
    }
}
