using HotelBooking.Application.DTO.Booking;
using HotelBooking.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Contracts
{
    public interface IBookingService
    {
        Task<PaginatedResponse<BookingDTO>> GetAllBookingsAsync(int pageIndex, int pageSize);

    }
}
