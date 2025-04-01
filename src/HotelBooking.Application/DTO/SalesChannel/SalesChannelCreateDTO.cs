using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.SalesChannel
{
    public class SalesChannelCreateDTO
    {
        [Required]
        [MaxLength(50)]
        public string MarketSegment { get; set; }

        [Required]
        [MaxLength(50)]
        public string DistributionChannel { get; set; }

        [MaxLength(50)]
        public string Agent { get; set; }

        [MaxLength(100)]
        public string Company { get; set; }
    }
}
