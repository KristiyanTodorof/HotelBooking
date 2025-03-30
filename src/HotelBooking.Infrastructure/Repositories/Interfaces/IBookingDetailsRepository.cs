using HotelBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories.Interfaces
{
    public interface IBookingDetailsRepository : IRepository<BookingDetails, Guid>
    {
        Task<BookingDetails> GetByBookingIdAsync(Guid bookingId);
    }
}
