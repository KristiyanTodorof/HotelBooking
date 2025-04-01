using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.SalesChannel
{
    public class SalesChannelDTO
    {
        public Guid Id { get; set; }
        public string MarketSegment { get; set; }
        public string DistributionChannel { get; set; }
        public string Agent { get; set; }
        public string Company { get; set; }
    }
}
