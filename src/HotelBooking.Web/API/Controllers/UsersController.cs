using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Password;
using HotelBooking.Application.DTO.User;
using HotelBooking.Web.API.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HotelBooking.Web.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Policy = "CanManageUsers")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            this._userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = users.TotalCount,
                    pageSize = users.PageSize,
                    currentPage = users.PageIndex,
                    totalPages = users.TotalPages,
                    hasPrevious = users.PageIndex > 1,
                    hasNext = users.PageIndex < users.TotalPages
                }));

                return Ok(users.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }

        }

        [HttpGet("role/{roleName}")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsersByRole(
            string roleName,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10
            )
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(roleName, pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = users.TotalCount,
                    pageSize = users.PageSize,
                    currentPage = users.PageIndex,
                    totalPages = users.TotalPages,
                    hasPrevious = users.PageIndex > 1,
                    hasNext = users.PageIndex < users.TotalPages
                }));

                return Ok(users.Items);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser(UserCreateDTO userCreateDTO)
        {
            try
            {
                var user = await _userService.CreateUserAsync(userCreateDTO);
                return Created(nameof(GetUser), new {id = user.Id}, user);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
            
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserUpdateDTO userUpdateDTO)
        {
            try
            {
                await _userService.UpdateUserAsync(id, userUpdateDTO);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex) 
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpPost("{userId}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(Guid userId, ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                if (userId != CurrentUserId && !UserHasRole("Admin"))
                {
                    return Forbid();
                }
                await _userService.ChangePasswordAsync(userId,
                    changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception ex) 
            {
                return Error(ex.Message);
            }
        }
        [HttpPost("{userId}/ reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(Guid userId, [FromBody] string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(newPassword))
                {
                    return BadRequest("New password is required");
                }
                await _userService.ResetPasswordAsync(userId, newPassword);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpPost("{userId}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(Guid userId)
        {
            try
            {
                await _userService.ToggleUserStatusAsync(userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetAllRoles()
        {
            try
            {
                var roles = await _userService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost("{userId}/roles/{roleName}")]
        public async Task<IActionResult> AssignRoleToUser(Guid userId, string roleName)
        {
            try
            {
                await _userService.AssignRoleToUserAsync(userId, roleName);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete("{userId}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRoleFromUser(Guid userId, string roleName)
        {
            try
            {
                await _userService.RemoveRoleFromUserAsync(userId, roleName);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
