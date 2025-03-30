using HotelBooking.Domain.BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Domain.Models
{
    public class Booking : BaseEntity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Hotel { get; set; }
        public bool IsCancelled { get; set; }
        [Range(0, int.MaxValue)]
        public int LeadTime { get; set; }
        [Range(2025,2100)]
        public int ArrivalDateYear { get; set; }
        [Required]
        [MaxLength(10)]
        public string ArrivalDateMonth { get; set; }
        [Range(1,31)]
        public byte ArrivalDateDayOfMonth { get; set; }
        [Range(1,53)]
        public byte ArrivalDateWeekNumber { get; set; }
        [Range(0,7)]
        public byte StaysInWeekendNights { get; set; }
        [Range (0,30)]
        public byte StaysInWeekNights { get; set; }
        [Range(0,10)]
        public byte BookingChanges { get; set; }
        [Range(0,int.MaxValue)]
        public int DaysInWaitingList { get; set; }
        [Range(0,double.MaxValue)]
        public double AverageDailyRate { get; set; }
        [Required]
        [MaxLength(20)]
        public string ReservationStatus { get; set; }
        public DateTimeOffset ReservationStatusDate { get; set; } = DateTimeOffset.UtcNow;

        // Relationships
        public Guid GuestId { get; set; }

        [ForeignKey("GuestId")]
        public virtual Guest Guest { get; set; }

        public Guid BookingDetailsId { get; set; }

        [ForeignKey("BookingDetailsId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public Guid RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        public Guid? PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; }

        public Guid SalesChannelId { get; set; }

        [ForeignKey("SalesChannelId")]
        public virtual SalesChannel SalesChannel { get; set; }
    }
}
