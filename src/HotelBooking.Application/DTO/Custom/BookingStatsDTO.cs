using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class BookingStatsDTO
    {
        public int TotalBookings { get; set; }
        public int CancelledBookings { get; set; }
        public double CancellataionRate { get; set; }
        public double TotalRevenue { get; set; }
        public double AverageBookingValue { get; set; }
    }
}
