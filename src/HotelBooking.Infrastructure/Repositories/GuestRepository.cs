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
    public class GuestRepository : Repository<Guest, Guid>, IGuestRepository
    {
        public GuestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Guest> GetByEmailAsync(string email)
        {
            return await _context.Guests
                .FirstOrDefaultAsync(g => g.Email == email);
        }

        public async Task<Guest> GetByUserIdAsync(Guid userId)
        {
            return await _context.Guests
                .FirstOrDefaultAsync(g => g.ApplicationUserId == userId);
        }

        public async Task<IEnumerable<Guest>> GetFrequentGuestAsync(int minimumStays)
        {
            return await _context.Guests
               .Where(g => g.PreviousBookingsNotCancelled >= minimumStays)
               .Include(g => g.Bookings)
               .ToListAsync();
        }
    }
}
