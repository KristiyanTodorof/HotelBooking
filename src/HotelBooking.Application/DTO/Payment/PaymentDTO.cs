using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Payment
{
    public class PaymentDTO
    {
        public Guid Id { get; set; }
        public string DepositType { get; set; }
        public string CreditCard { get; set; }
        public string CardExpiryDate { get; set; }
    }
}
