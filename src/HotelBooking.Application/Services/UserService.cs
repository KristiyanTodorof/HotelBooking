using AutoMapper;
using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Room;
using HotelBooking.Application.DTO.User;
using HotelBooking.Application.Pagination;
using HotelBooking.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._mapper = mapper;
            this._contextAccessor = contextAccessor;
        }
        public async Task AssignRoleToUserAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                throw new KeyNotFoundException($"Role '{roleName}' does not exist");
            }

            // Check if the user is already in the role
            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (isInRole)
            {
                return; // User is already in the role
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to change password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task<UserDTO> CreateUserAsync(UserCreateDTO userCreateDTO)
        {
            // Check if email is already in use
            var existingUser = await _userManager.FindByEmailAsync(userCreateDTO.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Email '{userCreateDTO.Email}' is already in use");
            }

            // Create the user
            var user = _mapper.Map<ApplicationUser>(userCreateDTO);
            user.UserName = userCreateDTO.Email; // Use email as username
            user.CreatedAt = DateTimeOffset.UtcNow;

            var result = await _userManager.CreateAsync(user, userCreateDTO.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Assign roles if specified
            if (userCreateDTO.Roles != null && userCreateDTO.Roles.Any())
            {
                foreach (var role in userCreateDTO.Roles)
                {
                    // Check if role exists
                    var roleExists = await _roleManager.RoleExistsAsync(role);
                    if (!roleExists)
                    {
                        // If role doesn't exist, delete the user and throw
                        await _userManager.DeleteAsync(user);
                        throw new KeyNotFoundException($"Role '{role}' does not exist");
                    }
                }

                // Add roles
                result = await _userManager.AddToRolesAsync(user, userCreateDTO.Roles);
                if (!result.Succeeded)
                {
                    // If adding roles fails, delete the user
                    await _userManager.DeleteAsync(user);
                    throw new InvalidOperationException($"Failed to assign roles: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Default to Guest role if no roles specified
                await _userManager.AddToRoleAsync(user, "Guest");
            }

            // Return the created user
            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return userDto;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            // Check if user is the current user
            var currentUserId = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id.ToString() == currentUserId)
            {
                throw new InvalidOperationException("You cannot delete your own account");
            }

            // Delete the user
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task<IEnumerable<RoleDTO>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return _mapper.Map<IEnumerable<RoleDTO>>(roles);
        }

        public async Task<PaginatedResponse<UserDTO>> GetAllUsersAsync(int pageIndex, int pageSize)
        {
            var usersQuery = _userManager.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName);

            // Apply pagination
            var totalCount = await usersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            if (pageIndex > totalPages && totalCount > 0)
            {
                pageIndex = totalPages;
            }

            var paginatedUsers = await usersQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs and add roles
            var userDtos = new List<UserDTO>();
            foreach (var user in paginatedUsers)
            {
                var userDto = _mapper.Map<UserDTO>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(userDto);
            }

            return new PaginatedResponse<UserDTO>
            {
                Items = userDtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<UserDTO> GetCurrentUserAsync()
        {
            var userId = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("Current user not found");
            }

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return userDto;
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return userDto;
        }

        public async Task<PaginatedResponse<UserDTO>> GetUsersByRoleAsync(string roleName, int pageIndex, int pageSize)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role '{roleName}' not found");
            }

            // Get all users in the role
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

            // Apply manual pagination
            var totalCount = usersInRole.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            if (pageIndex > totalPages && totalCount > 0)
            {
                pageIndex = totalPages;
            }

            var paginatedUsers = usersInRole
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to DTOs and add roles
            var userDtos = new List<UserDTO>();
            foreach (var user in paginatedUsers)
            {
                var userDto = _mapper.Map<UserDTO>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(userDto);
            }

            return new PaginatedResponse<UserDTO>
            {
                Items = userDtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                throw new KeyNotFoundException($"Role '{roleName}' does not exist");
            }

            // Check if the user is in the role
            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!isInRole)
            {
                return; // User is not in the role
            }

            // Get all roles for the user
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Count <= 1)
            {
                throw new InvalidOperationException("User must have at least one role");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to remove role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task ToggleUserStatusAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Check if user is the current user
            var currentUserId = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id.ToString() == currentUserId)
            {
                throw new InvalidOperationException("You cannot disable your own account");
            }

            // Toggle active status
            user.IsActive = !user.IsActive;

            // Save changes
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update user status: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task UpdateUserAsync(Guid id, UserUpdateDTO userUpdateDTO)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            // Check if email is being updated and already exists
            if (!string.IsNullOrEmpty(userUpdateDTO.Email) &&
                userUpdateDTO.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(userUpdateDTO.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    throw new InvalidOperationException($"Email '{userUpdateDTO.Email}' is already in use");
                }

                // Update email and username
                user.Email = userUpdateDTO.Email;
                user.UserName = userUpdateDTO.Email;
                user.NormalizedEmail = userUpdateDTO.Email.ToUpper();
                user.NormalizedUserName = userUpdateDTO.Email.ToUpper();
            }

            // Update the user properties
            _mapper.Map(userUpdateDTO, user);

            // Save the changes
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Handle role updates if specified
            if (userUpdateDTO.Roles != null)
            {
                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove roles not in the new list
                var rolesToRemove = currentRoles.Except(userUpdateDTO.Roles).ToList();
                if (rolesToRemove.Any())
                {
                    result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to remove roles: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // Add new roles
                var rolesToAdd = userUpdateDTO.Roles.Except(currentRoles).ToList();
                if (rolesToAdd.Any())
                {
                    // Verify all roles exist
                    foreach (var role in rolesToAdd)
                    {
                        var roleExists = await _roleManager.RoleExistsAsync(role);
                        if (!roleExists)
                        {
                            throw new KeyNotFoundException($"Role '{role}' does not exist");
                        }
                    }

                    result = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to add roles: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}
