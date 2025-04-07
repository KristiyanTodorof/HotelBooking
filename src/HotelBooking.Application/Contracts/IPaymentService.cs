using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Payment;
using HotelBooking.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Contracts
{
    public interface IPaymentService
    {
        Task<PaginatedResponse<PaymentDTO>> GetAllPaymentsAsync(int pageIndex, int pageSize);
        Task<PaymentDTO> GetPaymentByIdAsync(Guid id);
        Task<PaymentDTO> GetPaymentByBookingIdAsync(Guid bookingId);
        Task<PaginatedResponse<PaymentDTO>> GetPaymentByDateRangeAsync(DateTime startDate, DateTime endDate, 
            int pageIndex, int pageSize);
        Task<PaymentDTO> CreatePaymentAsync(PaymentCreateDTO paymentCreateDTO);
        Task AssignPaymentToBookingAsync(Guid paymentId, Guid bookingId);
        Task DeletePaymentAsync(Guid id);
        Task<PaymentStatsDTO> GetPaymentStatsAsync(DateTime startDate, DateTime endDate);
    }
}
