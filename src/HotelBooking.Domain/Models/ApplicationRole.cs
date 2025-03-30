using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Domain.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        [MaxLength(200)]
        public string Description { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Additional hotel-specific permissions
        public bool CanManageBookings { get; set; } = false;

        public bool CanManageRooms { get; set; } = false;

        public bool CanManageUsers { get; set; } = false;

        public bool CanAccessReports { get; set; } = false;

        public bool CanManagePayments { get; set; } = false;
    }
}
