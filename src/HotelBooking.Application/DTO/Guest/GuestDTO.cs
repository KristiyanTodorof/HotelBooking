using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Guest
{
    public class GuestDTO
    {
        public Guid GuestId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public bool IsRepeatedGuest { get; set; }
        public int PreviousCnacellations { get; set; }
        public int PreviousBookingsNotCancelled { get; set; }
        public string CustomerType { get; set; }
    }
}
