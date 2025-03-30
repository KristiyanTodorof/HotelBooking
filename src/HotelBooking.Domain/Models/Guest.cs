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
    public class Guest : BaseEntity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        [MaxLength(50)]
        public bool IsRepeatedGuest { get; set; }
        [Range(0, int.MaxValue)]
        public int PreviousCancellations { get; set; }
        [Range(0, int.MaxValue)]
        public int PreviousBookingsNotCancelled { get; set; }
        [Required]
        [MaxLength(20)]
        public string CustomerType { get; set; }

        // Relationships
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public Guid? ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
