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
    public class BookingDetailsRepository : Repository<BookingDetails, Guid>, IBookingDetailsRepository
    {
        public BookingDetailsRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BookingDetails> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.BookingDetails
               .FirstOrDefaultAsync(bd => bd.Booking.Id == bookingId);
        }
    }
}
