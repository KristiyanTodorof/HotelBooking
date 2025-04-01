using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.User
{
    public class UserUpdateDTO
    {
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Title { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsStaff { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        [MaxLength(100)]
        public string Position { get; set; }

        public List<string> Roles { get; set; }
    }
}
