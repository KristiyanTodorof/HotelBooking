using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Guest
{
    public class GuestUpdateDTO
    {
        [MaxLength(100)]
        public string Name { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(50)]
        public string Country { get; set; }

        public bool? IsRepeatedGuest { get; set; }

        [MaxLength(20)]
        public string CustomerType { get; set; }
    }
}
