using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Guest;
using HotelBooking.Domain.Models;
using HotelBooking.Web.API.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Web.API.Controllers
{
    [ApiController]
    [Route("api/guests")]
    public class GuestController : BaseApiController
    {
      private readonly IGuestService _guestService;
        public GuestController(IGuestService guestService)
        {
            this._guestService = guestService;
        }
        [HttpGet]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<GuestDTO>>> GetGuests(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var guests = await _guestService.GetAllGuestsAsync(pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = guests.TotalCount,
                    pageSize = guests.PageSize,
                    currentPage = guests.PageIndex,
                    totalPages = guests.TotalPages,
                    hasPrevious = guests.PageIndex > 1,
                    hasNext = guests.PageIndex < guests.TotalPages
                }));
                return Ok(guests.Items);
            }
            catch (Exception ex)
            {

                return Error(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy ="CanManageBookings")]
        public async Task<ActionResult<GuestDTO>> GetGuest(Guid id)
        {
            try
            {
                var guest = await _guestService.GetGuestByIdAsync(id);

                if (!UserHasRole("Admin") && !UserHasRole("Manager") && !UserHasRole("Receptionist"))
                {
                    var guestByUser = await _guestService.GetGuestByUserIdAsync(CurrentUserId.Value);
                    if (guestByUser.GuestId != id)
                    {
                        return Forbid();
                    }
                }
                return Ok(guest);
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

        [HttpGet("email/{email}")]
        [Authorize(Policy ="CanManageBookings")]
        public async Task<ActionResult<GuestDTO>> GetGuestByEmail(string email)
        {
            try
            {
                var guest = await _guestService.GetGuestByEmailAsync(email);
                return Ok(guest);
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
        public async Task<ActionResult<GuestDTO>> GetCurrentGuestProfile()
        {
            try
            {
                if (!CurrentUserId.HasValue)
                {
                    return Unauthorized("User is not authenticated");
                }
                var guest = await _guestService.GetGuestByUserIdAsync(CurrentUserId.Value);
                return Ok(guest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("No guest profile found for the current user ");
            }
            catch (Exception ex) 
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("frequent")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<GuestDTO>>> GetFrequentGuests(
            [FromQuery] int minumumStays = 3,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var guests = await _guestService.GetFrequentGuestsAsync(minumumStays, pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = guests.TotalCount,
                    pageSize = guests.PageSize,
                    currentPage = guests.PageIndex,
                    totalPages = guests.TotalPages,
                    hasPrevious = guests.PageIndex > 1,
                    hasNext = guests.PageIndex < guests.TotalPages
                }));

                return Ok(guests.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<GuestDTO>> CreateGuest(GuestCreateDTO guestCreateDTO)
        {
            try
            {
                var guest = await _guestService.CreateGuestAsync(guestCreateDTO);
                return Created(nameof(GetGuest), new { id = guest.GuestId }, guest);
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

        [HttpPut]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<IActionResult> UpdateGuest(Guid id, GuestUpdateDTO guestUpdateDTO)
        {
            try
            {
                if(!UserHasRole("Admin") && !UserHasRole("Manager") && !UserHasRole("Receptionist"))
                {
                    var guestByUser = await _guestService.GetGuestByUserIdAsync(CurrentUserId.Value);
                    if (guestByUser.GuestId != id)
                    {
                        return Forbid();
                    }
                }

                await _guestService.UpdateGuestAsync(id, guestUpdateDTO);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch(Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy ="CanManageUsers")]
        public async Task<IActionResult> DeleteGuest(Guid id)
        {
            try
            {
                await _guestService.DeleteGuestAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost("{guestId}/link-user/{userId}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> LnikUserToGuest(Guid guestId, Guid userId)
        {
            try
            {
                await _guestService.LinkUserToGuestAsync(guestId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("stats")]
        [Authorize(Policy = "CanAccessReports")]
        public async Task<ActionResult<GuestStatsDTO>> GetGuestStats()
        {
            try
            {
                var stats = await _guestService.GetGuestStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("search")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<GuestDTO>>> SearchGuests(
            [FromQuery] string searchTerm = null,
            [FromQuery] string country = null,
            [FromQuery] bool? isRepeatedGuest = null,
            [FromQuery] string customerType = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var guests = await _guestService.SearchGuestsAsync(
                    searchTerm, country, isRepeatedGuest, customerType, pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = guests.TotalCount,
                    pageSize = guests.PageSize,
                    currentPage = guests.PageIndex,
                    totalPages = guests.TotalPages,
                    hasPrevious = guests.PageIndex > 1,
                    hasNext = guests.PageIndex < guests.TotalPages
                }));

                return Ok(guests.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

    }
}
