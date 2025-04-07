using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.User;
using HotelBooking.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Contracts
{
    public interface IUserService
    {
        Task<PaginatedResponse<UserDTO>> GetAllUsersAsync(int pageIndex, int pageSize);
        Task<UserDTO> GetUserByIdAsync(Guid id);
        Task<UserDTO> GetCurrentUserAsync();
        Task<PaginatedResponse<UserDTO>> GetUsersByRoleAsync(string roleName, int pageIndex, int pageSize);
        Task<UserDTO> CreateUserAsync(UserCreateDTO userCreateDTO);
        Task UpdateUserAsync(Guid id, UserUpdateDTO userUpdateDTO);
        Task DeleteUserAsync(Guid id);
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task ResetPasswordAsync(Guid userId, string newPassword);
        Task ToggleUserStatusAsync(Guid userId);
        Task<IEnumerable<RoleDTO>> GetAllRolesAsync();
        Task AssignRoleToUserAsync(Guid userId, string roleName);
        Task RemoveRoleFromUserAsync(Guid userId, string roleName);
    }
}
