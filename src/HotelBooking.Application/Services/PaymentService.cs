using AutoMapper;
using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Payment;
using HotelBooking.Application.Pagination;
using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }
        public async Task AssignPaymentToBookingAsync(Guid paymentId, Guid bookingId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found");
            }

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new KeyNotFoundException("Booking not found");
            }

            // Check if the booking already has a payment
            if (booking.PaymentId.HasValue && booking.PaymentId.Value != paymentId)
            {
                throw new InvalidOperationException("The booking already has a different payment assigned");
            }

            try
            {
                // Start transaction
                await _unitOfWork.BeginTransactionAsync();

                // Update booking with payment reference
                booking.PaymentId = paymentId;
                await _unitOfWork.Bookings.UpdateAsync(booking);

                // Complete and commit
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PaymentDTO> CreatePaymentAsync(PaymentCreateDTO paymentCreateDTO)
        {
            var payment = _mapper.Map<Payment>(paymentCreateDTO);

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task DeletePaymentAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found");
            }

            try
            {
                // Check if the payment is associated with any bookings
                var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(b => b.PaymentId == id);

                if (booking != null)
                {
                    // Start transaction
                    await _unitOfWork.BeginTransactionAsync();

                    // Remove the payment association from the booking
                    booking.PaymentId = null;
                    await _unitOfWork.Bookings.UpdateAsync(booking);

                    // Delete the payment
                    await _unitOfWork.Payments.DeleteAsync(payment);

                    // Complete and commit
                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                else
                {
                    // If not associated with any booking, just delete
                    await _unitOfWork.Payments.DeleteAsync(payment);
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PaginatedResponse<PaymentDTO>> GetAllPaymentsAsync(int pageIndex, int pageSize)
        {
            var paymentsQuery = _unitOfWork.Payments.Query();

            var paginatedPayments = await PaginationHelper.CreateAsync(paymentsQuery, pageIndex, pageSize);

            var paymentDtos = _mapper.Map<List<PaymentDTO>>(paginatedPayments.Items);

            return new PaginatedResponse<PaymentDTO>
            {
                Items = paymentDtos,
                PageIndex = paginatedPayments.PageIndex,
                PageSize = paginatedPayments.PageSize,
                TotalCount = paginatedPayments.TotalCount,
                TotalPages = paginatedPayments.TotalPages
            };
        }

        public async Task<PaymentDTO> GetPaymentByBookingIdAsync(Guid bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new KeyNotFoundException("Booking not found");
            }

            if (!booking.PaymentId.HasValue)
            {
                throw new KeyNotFoundException("No payment found for this booking");
            }

            var payment = await _unitOfWork.Payments.GetByIdAsync(booking.PaymentId.Value);
            if (payment == null)
            {
                throw new KeyNotFoundException("Payment record not found");
            }

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaginatedResponse<PaymentDTO>> GetPaymentByDateRangeAsync(DateTime startDate, DateTime endDate, int pageIndex, int pageSize)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            var payments = await _unitOfWork.Payments.GetPaymentsByDateRangeAsync(startDate, endDate);

            // Apply manual pagination
            var totalCount = payments.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // Adjust page index if it exceeds total pages
            if (pageIndex > totalPages && totalCount > 0)
            {
                pageIndex = totalPages;
            }

            var paginatedPayments = payments
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paymentDtos = _mapper.Map<List<PaymentDTO>>(paginatedPayments);

            return new PaginatedResponse<PaymentDTO>
            {
                Items = paymentDtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<PaymentDTO> GetPaymentByIdAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);

            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment with ID {id} not found");
            }

            return _mapper.Map<PaymentDTO>(payment);

        }

        public async Task<PaymentStatsDTO> GetPaymentStatsAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _unitOfWork.Bookings.Query()
                .Include(b => b.Payment)
                .Where(b => !b.IsCancelled && b.PaymentId.HasValue)
                .ToListAsync();

            // Filter bookings by reservation date in the specified range
            var bookingsInRange = bookings.Where(b =>
                b.ReservationStatusDate.DateTime >= startDate &&
                b.ReservationStatusDate.DateTime <= endDate).ToList();

            // Calculate total payments
            double totalPayments = 0;
            var paymentsByDepositType = new Dictionary<string, double>();
            var paymentCountByMonth = new Dictionary<string, int>();

            foreach (var booking in bookingsInRange)
            {
                if (booking.Payment == null) continue;

                // Calculate payment amount (using average daily rate * total nights)
                var totalNights = booking.StaysInWeekendNights + booking.StaysInWeekNights;
                var paymentAmount = booking.AverageDailyRate * totalNights;

                // Add to total payments
                totalPayments += paymentAmount;

                // Add to payments by deposit type
                var depositType = booking.Payment.DepositType;
                if (!paymentsByDepositType.ContainsKey(depositType))
                {
                    paymentsByDepositType[depositType] = 0;
                }
                paymentsByDepositType[depositType] += paymentAmount;

                // Add to payments by month
                var month = booking.ReservationStatusDate.DateTime.ToString("MMMM", CultureInfo.InvariantCulture);
                if (!paymentCountByMonth.ContainsKey(month))
                {
                    paymentCountByMonth[month] = 0;
                }
                paymentCountByMonth[month]++;
            }

            // Calculate average payment amount
            var paymentCount = bookingsInRange.Count;
            var averagePaymentAmount = paymentCount > 0 ? totalPayments / paymentCount : 0;

            return new PaymentStatsDTO
            {
                TotalPayments = Math.Round(totalPayments, 2),
                PaymentCount = paymentCount,
                PaymentByDepositType = paymentsByDepositType.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Math.Round(kvp.Value, 2)),
                AveragePaymentAmount = Math.Round(averagePaymentAmount, 2),
                PaymentCountByMonth = paymentCountByMonth
            };
        }
    }
}
