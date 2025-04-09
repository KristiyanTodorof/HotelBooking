using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Room;
using HotelBooking.Web.API.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Web.API.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomsController : BaseApiController
    {
        private readonly IRoomService _roomService;
        public RoomsController(IRoomService roomService)
        {
            this._roomService = roomService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDTO>>> GetRooms(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync(pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = rooms.TotalCount,
                    pageSize = rooms.PageSize,
                    currentPage = rooms.PageIndex,
                    totalPages = rooms.TotalPages,
                    hasPrevious = rooms.PageIndex > 1,
                    hasNext = rooms.PageIndex < rooms.TotalPages
                }));

                return Ok(rooms.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDTO>> GetRoom(Guid id)
        {
            try
            {
                var room = await _roomService.GetRoomByIdAsync(id);
                return Ok(room);
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

        [HttpGet("number/{roomNumber}")]
        public async Task<ActionResult<RoomDTO>> GetRoomByNumber(string roomNumber)
        {
            try
            {
                var room = await _roomService.GetRoomByNumberAsync(roomNumber);
                return Ok(room);
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

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RoomDTO>>> GetAvailableRooms(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (startDate >= endDate)
                {
                    return BadRequest("Start date must be before end date");
                }

                var rooms = await _roomService.GetAvailableRoomsAsync(startDate, endDate, pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = rooms.TotalCount,
                    pageSize = rooms.PageSize,
                    currentPage = rooms.PageIndex,
                    totalPages = rooms.TotalPages,
                    hasPrevious = rooms.PageIndex > 1,
                    hasNext = rooms.PageIndex < rooms.TotalPages
                }));

                return Ok(rooms.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("type/{roomType}")]
        public async Task<ActionResult<IEnumerable<RoomDTO>>> GetRoomsByType(
            string roomType,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByTypeAsync(roomType, pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = rooms.TotalCount,
                    pageSize = rooms.PageSize,
                    currentPage = rooms.PageIndex,
                    totalPages = rooms.TotalPages,
                    hasPrevious = rooms.PageIndex > 1,
                    hasNext = rooms.PageIndex < rooms.TotalPages
                }));

                return Ok(rooms.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "CanManageRooms")]
        public async Task<ActionResult<RoomDTO>> CreateRoom(RoomCreateDTO roomCreateDto)
        {
            try
            {
                var room = await _roomService.CreateRoomAsync(roomCreateDto);
                return Created(nameof(GetRoom), new { id = room.Id }, room);
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

        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageRooms")]
        public async Task<IActionResult> UpdateRoom(Guid id, RoomUpdateDTO roomUpdateDto)
        {
            try
            {
                await _roomService.UpdateRoomAsync(id, roomUpdateDto);
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

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageRooms")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            try
            {
                await _roomService.DeleteRoomAsync(id);
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

        [HttpGet("{roomId}/availability")]
        public async Task<ActionResult<bool>> IsRoomAvailable(
            Guid roomId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                {
                    return BadRequest("Start date must be before end date");
                }

                var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, startDate, endDate);
                return Ok(isAvailable);
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

        [HttpGet("availability-by-type")]
        public async Task<ActionResult<Dictionary<string, int>>> GetAvailabilityByRoomType(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                {
                    return BadRequest("Start date must be before end date");
                }

                var availability = await _roomService.GetAvailabilityByRoomTypeAsync(startDate, endDate);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("revenue-stats")]
        [Authorize(Policy = "CanAccessReports")]
        public async Task<ActionResult<RoomRevenueStatsDTO>> GetRoomRevenueStats(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date must be before end date");
                }

                var stats = await _roomService.GetRoomRevenueStatsAsync(startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
