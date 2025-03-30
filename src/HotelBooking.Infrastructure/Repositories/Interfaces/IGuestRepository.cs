using HotelBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories.Interfaces
{
    public interface IGuestRepository : IRepository<Guest, Guid>
    {
        Task<Guest> GetByEmailAsync(string email);
        Task<IEnumerable<Guest>> GetFrequentGuestAsync(int minimumStays);
        Task<Guest> GetByUserIdAsync(Guid userId);
    }
}
