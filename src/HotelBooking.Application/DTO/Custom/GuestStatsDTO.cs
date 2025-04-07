using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class GuestStatsDTO
    {
        public int TotalGuests { get; set; }
        public int RepeatedGuests { get; set; }
        public double RepeatedGuestPercentage { get; set; }
        public Dictionary<string, int> GuestsByCountry { get; set; }
        public Dictionary<string, int> GuestsByType { get; set; }
        public int GuestsWithCancellations { get; set; }
        public int LinkedUserAccounts { get; set; }
    }
}
