using HotelBooking.Application.DTO.Booking;
using HotelBooking.Application.DTO.Custom;
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
        Task<BookingDTO> GetBookingByIdAsync(Guid id);
        Task<PaginatedResponse<BookingDTO>> GetBookingByGuestAsync(Guid guestId, int pageIndex, int pageSize);
        Task<PaginatedResponse<BookingDTO>> GetBookingByDateRangeAsync(DateTime startDate, DateTime endDate, int pageIndex, int pageSize);
        Task<PaginatedResponse<BookingDTO>> GetActiveBookingsAsync(int pageIndex, int pageSize);
        Task<BookingDTO> CreateBookingAsync(BookingCreateDTO bookingCreateDTO);
        Task<BookingDTO> UpdateBookingAsync(Guid id, BookingUpdateDTO bookingUpdateDTO);
        Task CancelBookingAsync(Guid id);
        Task DeleteBookingAsync(Guid id);
        Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<BookingDTO>> GetUpcomingCheckInsAsync(DateTime date);
        Task<IEnumerable<BookingDTO>> GetUpcomingCheckOutsAsync(DateTime date);
        Task<OccupancyStatsDTO> GetOccupancyStatsAsync(DateTime startDate, DateTime endDate);

    }
}
