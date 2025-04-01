using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Payment
{
    public class PaymentCreateDTO
    {
        [Required]
        [MaxLength(20)]
        public string DepositType { get; set; }
        [CreditCard]
        [MaxLength(19)]
        public string CreditCard { get; set; }
        [MaxLength(10)]
        public string CardExpiryDate { get; set; }
    }
}
