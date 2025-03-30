using HotelBooking.Domain.BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Domain.Models
{
    public class SalesChannel : BaseEntity<Guid>
    {
        [Required]
        [MaxLength(50)]
        public string MarketSegment { get; set; }
        [Required]
        [MaxLength(50)]
        public string DistributionChannel { get; set; }
        [MaxLength(50)]
        public string Agent { get; set; }
        [MaxLength(100)]
        public string Company { get; set; }

        // Relationships
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
