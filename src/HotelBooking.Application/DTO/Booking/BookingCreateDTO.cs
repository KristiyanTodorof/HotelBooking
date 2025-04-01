using HotelBooking.Application.DTO.BookingDetails;
using HotelBooking.Application.DTO.Payment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Booking
{
    public class BookingCreateDTO
    {
        [Required]
        [MaxLength(100)]
        public string Hotel { get; set; }

        [Required]
        public int LeadTime { get; set; }

        [Required]
        [Range(2025, 2100)]
        public int ArrivalDateYear { get; set; }

        [Required]
        [MaxLength(10)]
        public string ArrivalDateMonth { get; set; }

        [Required]
        [Range(1, 31)]
        public byte ArrivalDateDayOfMonth { get; set; }

        [Required]
        [Range(1,53)]
        public byte ArrivalDateWeekNumber { get; set; }

        [Required]
        [Range(0, 7)]
        public byte StaysInWeekendNights { get; set; }

        [Required]
        [Range(0, 30)]
        public byte StaysInWeekNights { get; set; }

        [Required]
        public Guid GuestId { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        [Required]
        public Guid SalesChannelId { get; set; }

        [Required]
        public BookingDetailsCreateDTO BookingDetails { get; set; }

        public PaymentCreateDTO Payment { get; set; }
    }
}
