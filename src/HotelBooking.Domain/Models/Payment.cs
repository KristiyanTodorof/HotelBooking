using HotelBooking.Domain.BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HotelBooking.Domain.Models
{
    public class Payment : BaseEntity<Guid>
    {
        [Required]
        [MaxLength(20)]
        public string DepositType { get; set; }
        [MaxLength(19)]
        [JsonIgnore]
        public string CreditCard { get; set; }
        [NotMapped] 
        public string MaskedCreditCardNumber
        {
            get
            {
                if (string.IsNullOrEmpty(CreditCard)) 
                {
                    return null;
                }
                    
                string lastFourDigits = CreditCard.Replace(" ", "").Substring(Math.Max(0, CreditCard.Replace(" ", "").Length - 4));
                return $"************{lastFourDigits}";
            }
        }
        [MaxLength(10)]
        public string CardExpiryDate { get; set; }

        // Relationships
        public virtual Booking Booking { get; set; }
    }
}
