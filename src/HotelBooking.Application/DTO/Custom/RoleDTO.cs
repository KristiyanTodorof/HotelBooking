using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class RoleDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool CanManageBookings { get; set; }
        public bool CanManageRooms { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanAccessReports { get; set; }
        public bool CanManagePayments { get; set; }
    }
}
