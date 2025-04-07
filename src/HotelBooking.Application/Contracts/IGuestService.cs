using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Guest;
using HotelBooking.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Contracts
{
    public interface IGuestService
    {
        Task<PaginatedResponse<GuestDTO>> GetAllGuestsAsync(int pageIndex, int pageSize);
        Task<GuestDTO> GetGuestByIdAsync(Guid id);
        Task<GuestDTO> GetGuestByEmailAsync(string email);
        Task<GuestDTO> GetGuestByUserIdAsync(Guid userId);
        Task<PaginatedResponse<GuestDTO>> GetFrequentGuestsAsync(int minimumStays, 
            int pageIndex, int pageSize);
        Task<GuestDTO> CreateGuestAsync(GuestCreateDTO guestCreateDTO);
        Task UpdateGuestAsync(Guid id, GuestUpdateDTO guestUpdateDTO);
        Task DeleteGuestAsync(Guid id);
        Task LinkUserToGuestAsync(Guid guestId, Guid userId);
        Task<GuestStatsDTO> GetGuestStatsAsync();

        Task<PaginatedResponse<GuestDTO>> SearchGuestsAsync(
            string searchTerm,
            string country,
            bool? isRepeatedGuest,
            string customerType,
            int pageIndex,
            int pageSize);

    }
}
