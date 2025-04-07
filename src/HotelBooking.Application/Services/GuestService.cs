using AutoMapper;
using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Guest;
using HotelBooking.Application.Pagination;
using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Services
{
    public class GuestService : IGuestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public GuestService(IUnitOfWork unitOfWork, IMapper mapper, 
            UserManager<ApplicationUser> userManager)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._userManager = userManager;
        }
        public async Task<GuestDTO> CreateGuestAsync(GuestCreateDTO guestCreateDTO)
        {
            // Check if email already exists
            var existingGuest = await _unitOfWork.Guests.GetByEmailAsync(guestCreateDTO.Email);
            if (existingGuest != null)
            {
                throw new InvalidOperationException($"A guest with email {guestCreateDTO.Email} already exists");
            }

            // If ApplicationUserId is provided, verify it exists
            if (guestCreateDTO.ApplicationUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(guestCreateDTO.ApplicationUserId.Value.ToString());
                if (user == null)
                {
                    throw new KeyNotFoundException("The specified user does not exist");
                }

                // Check if this user is already linked to another guest
                var existingUserGuest = await _unitOfWork.Guests.GetByUserIdAsync(guestCreateDTO.ApplicationUserId.Value);
                if (existingUserGuest != null)
                {
                    throw new InvalidOperationException("The specified user is already linked to another guest");
                }
            }

            var guest = _mapper.Map<Guest>(guestCreateDTO);
            await _unitOfWork.Guests.AddAsync(guest);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<GuestDTO>(guest);
        }

        public async Task DeleteGuestAsync(Guid id)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(id);
            if (guest == null)
            {
                throw new KeyNotFoundException($"Guest with ID {id} not found");
            }

            var hasBookings = await _unitOfWork.Bookings.AnyAsync(b => b.GuestId == id);
            if (hasBookings)
            {
                throw new InvalidOperationException("Cannot delete a guest with associated bookings");
            }

            await _unitOfWork.Guests.DeleteAsync(guest);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaginatedResponse<GuestDTO>> GetAllGuestsAsync(int pageIndex, int pageSize)
        {
            var guestsQuery = _unitOfWork.Guests.Query()
                .OrderBy(g => g.Name);

            var paginatedGuests = await PaginationHelper.CreateAsync(guestsQuery, pageIndex, pageSize);

            var guestDtos = _mapper.Map<List<GuestDTO>>(paginatedGuests.Items);

            return new PaginatedResponse<GuestDTO>
            {
                Items = guestDtos,
                PageIndex = paginatedGuests.PageIndex,
                PageSize = paginatedGuests.PageSize,
                TotalCount = paginatedGuests.TotalCount,
                TotalPages = paginatedGuests.TotalPages
            };
        }

        public async Task<PaginatedResponse<GuestDTO>> GetFrequentGuestsAsync(int minimumStays, int pageIndex, int pageSize)
        {
            var guestsQuery = _unitOfWork.Guests.Query()
               .Where(g => g.PreviousBookingsNotCancelled >= minimumStays)
               .OrderByDescending(g => g.PreviousBookingsNotCancelled);

            var paginatedGuests = await PaginationHelper.CreateAsync(guestsQuery, pageIndex, pageSize);

            var guestDtos = _mapper.Map<List<GuestDTO>>(paginatedGuests.Items);

            return new PaginatedResponse<GuestDTO>
            {
                Items = guestDtos,
                PageIndex = paginatedGuests.PageIndex,
                PageSize = paginatedGuests.PageSize,
                TotalCount = paginatedGuests.TotalCount,
                TotalPages = paginatedGuests.TotalPages
            };
        }

        public async Task<GuestDTO> GetGuestByEmailAsync(string email)
        {
            var guest = await _unitOfWork.Guests.GetByEmailAsync(email);

            if (guest == null)
            {
                throw new KeyNotFoundException($"Guest with email {email} not found");
            }

            return _mapper.Map<GuestDTO>(guest);
        }

        public async Task<GuestDTO> GetGuestByIdAsync(Guid id)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(id);

            if (guest == null)
            {
                throw new KeyNotFoundException($"Guest with ID {id} not found");
            }

            return _mapper.Map<GuestDTO>(guest);
        }

        public async Task<GuestDTO> GetGuestByUserIdAsync(Guid userId)
        {
            var guest = await _unitOfWork.Guests.GetByUserIdAsync(userId);

            if (guest == null)
            {
                throw new KeyNotFoundException($"No guest profile found for user with ID {userId}");
            }

            return _mapper.Map<GuestDTO>(guest);
        }

        public async Task<GuestStatsDTO> GetGuestStatsAsync()
        {
            var allGuests = await _unitOfWork.Guests.GetAllAsync();
            var guestsList = allGuests.ToList();

            // Calculate basic statistics
            int totalGuests = guestsList.Count;
            int repeatedGuests = guestsList.Count(g => g.IsRepeatedGuest);
            double repeatedGuestPercentage = totalGuests > 0
                ? Math.Round((double)repeatedGuests / totalGuests * 100, 2)
                : 0;

            // Group guests by country
            var guestsByCountry = guestsList
                .GroupBy(g => string.IsNullOrEmpty(g.Country) ? "Unknown" : g.Country)
                .ToDictionary(g => g.Key, g => g.Count());

            // Group guests by customer type
            var guestsByType = guestsList
                .GroupBy(g => g.CustomerType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Count guests with cancellations
            int guestsWithCancellations = guestsList.Count(g => g.PreviousCancellations > 0);

            // Count guests with linked user accounts
            int linkedUserAccounts = guestsList.Count(g => g.ApplicationUserId.HasValue);

            return new GuestStatsDTO
            {
                TotalGuests = totalGuests,
                RepeatedGuests = repeatedGuests,
                RepeatedGuestPercentage = repeatedGuestPercentage,
                GuestsByCountry = guestsByCountry,
                GuestsByType = guestsByType,
                GuestsWithCancellations = guestsWithCancellations,
                LinkedUserAccounts = linkedUserAccounts
            };
        }

        public async Task LinkUserToGuestAsync(Guid guestId, Guid userId)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
            if (guest == null)
            {
                throw new KeyNotFoundException("Guest not found");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var existingUserGuest = await _unitOfWork.Guests.GetByUserIdAsync(userId);
            if (existingUserGuest != null && existingUserGuest.Id != guestId)
            {
                throw new InvalidOperationException("The user is already linked to another guest");
            }

            guest.ApplicationUserId = userId;
            await _unitOfWork.Guests.UpdateAsync(guest);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaginatedResponse<GuestDTO>> SearchGuestsAsync(string searchTerm,
            string country, bool? isRepeatedGuest, 
            string customerType, int pageIndex, int pageSize)
        {
            var query = _unitOfWork.Guests.Query();

            // Apply search term filter (on name or email)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(g =>
                    g.Name.ToLower().Contains(searchTerm) ||
                    g.Email.ToLower().Contains(searchTerm) ||
                    (g.PhoneNumber != null && g.PhoneNumber.Contains(searchTerm)));
            }

            // Apply country filter
            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(g => g.Country == country);
            }

            // Apply repeated guest filter
            if (isRepeatedGuest.HasValue)
            {
                query = query.Where(g => g.IsRepeatedGuest == isRepeatedGuest.Value);
            }

            // Apply customer type filter
            if (!string.IsNullOrEmpty(customerType))
            {
                query = query.Where(g => g.CustomerType == customerType);
            }

            // Order by name
            query = query.OrderBy(g => g.Name);

            // Apply pagination
            var paginatedGuests = await PaginationHelper.CreateAsync(query, pageIndex, pageSize);

            var guestDtos = _mapper.Map<List<GuestDTO>>(paginatedGuests.Items);

            return new PaginatedResponse<GuestDTO>
            {
                Items = guestDtos,
                PageIndex = paginatedGuests.PageIndex,
                PageSize = paginatedGuests.PageSize,
                TotalCount = paginatedGuests.TotalCount,
                TotalPages = paginatedGuests.TotalPages
            };
        }

        public async Task UpdateGuestAsync(Guid id, GuestUpdateDTO guestUpdateDTO)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(id);
            if (guest == null) 
            {
                throw new KeyNotFoundException($"Guest with ID {id} not found");
            }

            if(!string.IsNullOrEmpty(guestUpdateDTO.Email) && guestUpdateDTO.Email != guest.Email)
            {
                var existingGuest = await _unitOfWork.Guests.GetByEmailAsync(guestUpdateDTO.Email);
                if (existingGuest != null && existingGuest.Id != id)
                {
                    throw new InvalidOperationException($"A guest with email {guestUpdateDTO.Email} already exists");
                }
            }
            _mapper.Map(guestUpdateDTO, guest);
            await _unitOfWork.Guests.UpdateAsync(guest);
            await _unitOfWork.CompleteAsync();
        }
    }
}
