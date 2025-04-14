using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class TokenResponseDTO
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
