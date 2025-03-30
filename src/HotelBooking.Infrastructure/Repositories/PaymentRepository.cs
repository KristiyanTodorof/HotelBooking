using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Data;
using HotelBooking.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories
{
    public class PaymentRepository : Repository<Payment, Guid>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Payment> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.Booking.Id == bookingId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
               .Where(p => p.Booking.ReservationStatusDate >= new DateTimeOffset(startDate) &&
                          p.Booking.ReservationStatusDate <= new DateTimeOffset(endDate))
               .ToListAsync();
        }
    }
}
