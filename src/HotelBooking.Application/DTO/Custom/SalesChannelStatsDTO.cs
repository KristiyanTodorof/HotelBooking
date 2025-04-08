using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class SalesChannelStatsDTO
    {
        public int TotalChannels { get; set; }
        public Dictionary<string, int> ChannelsByMarketSegment { get; set; }
        public Dictionary<string, int> ChannelsByDistributionType { get; set; }
        public Dictionary<string, BookingStatsDTO> BookingStatsByChannel { get; set; }
    }
}
