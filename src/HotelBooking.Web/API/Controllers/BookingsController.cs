using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Booking;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Web.API.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Web.API.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController : BaseApiController
    {
       private readonly IBookingService _bookingService;
        public BookingsController(IBookingService bookingService)
        {
            this._bookingService = bookingService;
        }

        [HttpGet]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetAllBookings(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync(pageIndex, pageSize);

                // Add pagination headers
                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = bookings.TotalCount,
                    pageSize = bookings.PageSize,
                    currentPage = bookings.PageIndex,
                    totalPages = bookings.TotalPages,
                    hasPrevious = bookings.PageIndex > 1,
                    hasNext = bookings.PageIndex < bookings.TotalPages
                }));

                return Ok(bookings.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<BookingDTO>> GetBooking(Guid id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                return Ok(booking);
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
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<BookingDTO>> CreateBooking(BookingCreateDTO bookingCreateDto)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(bookingCreateDto);
                return Created(nameof(GetBooking), new { id = booking.Id }, booking);
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

        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<IActionResult> UpdateBooking(Guid id, BookingUpdateDTO bookingUpdateDto)
        {
            try
            {
                await _bookingService.UpdateBookingAsync(id, bookingUpdateDto);
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

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            try
            {
                await _bookingService.DeleteBookingAsync(id);
                return NoContent();
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

        [HttpGet("guest/{guestId}")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByGuest(
            Guid guestId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var bookings = await _bookingService.GetBookingByGuestAsync(guestId, pageIndex, pageSize);

                // Add pagination headers
                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = bookings.TotalCount,
                    pageSize = bookings.PageSize,
                    currentPage = bookings.PageIndex,
                    totalPages = bookings.TotalPages,
                    hasPrevious = bookings.PageIndex > 1,
                    hasNext = bookings.PageIndex < bookings.TotalPages
                }));

                return Ok(bookings.Items);
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

        [HttpPost("{id}/cancel")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            try
            {
                await _bookingService.CancelBookingAsync(id);
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

        [HttpGet("date-range")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date must be before end date");
                }

                var bookings = await _bookingService.GetBookingByDateRangeAsync(startDate, endDate, pageIndex, pageSize);

                // Add pagination headers
                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = bookings.TotalCount,
                    pageSize = bookings.PageSize,
                    currentPage = bookings.PageIndex,
                    totalPages = bookings.TotalPages,
                    hasPrevious = bookings.PageIndex > 1,
                    hasNext = bookings.PageIndex < bookings.TotalPages
                }));

                return Ok(bookings.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("active")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetActiveBookings(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var bookings = await _bookingService.GetActiveBookingsAsync(pageIndex, pageSize);

                // Add pagination headers
                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = bookings.TotalCount,
                    pageSize = bookings.PageSize,
                    currentPage = bookings.PageIndex,
                    totalPages = bookings.TotalPages,
                    hasPrevious = bookings.PageIndex > 1,
                    hasNext = bookings.PageIndex < bookings.TotalPages
                }));

                return Ok(bookings.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("check-ins")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetUpcomingCheckIns([FromQuery] DateTime date)
        {
            try
            {
                var checkIns = await _bookingService.GetUpcomingCheckInsAsync(date);
                return Ok(checkIns);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("check-outs")]
        [Authorize(Policy = "CanManageBookings")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetUpcomingCheckOuts([FromQuery] DateTime date)
        {
            try
            {
                var checkOuts = await _bookingService.GetUpcomingCheckOutsAsync(date);
                return Ok(checkOuts);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("occupancy-stats")]
        [Authorize(Policy = "CanAccessReports")]
        public async Task<ActionResult<OccupancyStatsDTO>> GetOccupancyStats(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date must be before end date");
                }

                var stats = await _bookingService.GetOccupancyStatsAsync(startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
