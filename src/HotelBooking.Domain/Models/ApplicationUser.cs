using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Domain.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Title { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Additional navigation properties for hotel-specific functionality
        [ForeignKey("GuestId")]
        public virtual Guest Guest { get; set; }

        public Guid? GuestId { get; set; }

        // Track which users are staff members
        public bool IsStaff { get; set; } = false;

        [MaxLength(100)]
        public string Department { get; set; }

        [MaxLength(100)]
        public string Position { get; set; }
    }
}
