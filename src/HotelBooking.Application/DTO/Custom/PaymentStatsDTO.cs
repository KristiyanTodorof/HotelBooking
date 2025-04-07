using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.DTO.Custom
{
    public class PaymentStatsDTO
    {
        public double TotalPayments { get; set; }
        public int PaymentCount { get; set; }
        public Dictionary<string, double> PaymentByDepositType { get; set; }
        public double AveragePaymentAmount { get; set; }
        public Dictionary<string, int> PaymentCountByMonth { get; set; }
    }
}
