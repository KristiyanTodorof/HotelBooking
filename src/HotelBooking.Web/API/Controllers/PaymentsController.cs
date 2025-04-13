using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Payment;
using HotelBooking.Web.API.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HotelBooking.Web.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize(Policy = "CanManagePayments")]
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            this._paymentService = paymentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetPayments(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var payments = await _paymentService.GetAllPaymentsAsync(pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = payments.TotalCount,
                    pageSize = payments.PageSize,
                    currentPage = payments.PageIndex,
                    totalPages = payments.TotalPages,
                    hasPrevious = payments.PageIndex > 1,
                    hasNext = payments.PageIndex < payments.TotalPages
                }));

                return Ok(payments.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDTO>> GetPayment(Guid id)
        {
            try
            {
                var guest = await _paymentService.GetPaymentByIdAsync(id);
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

        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<PaymentDTO>> GetPaymentByBooking(Guid bookingId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByBookingIdAsync(bookingId);
                return Ok(payment);
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

        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetPaymentsByDateRange(
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

                var payments = await _paymentService.GetPaymentByDateRangeAsync(
                    startDate, endDate, pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = payments.TotalCount,
                    pageSize = payments.PageSize,
                    currentPage = payments.PageIndex,
                    totalPages = payments.TotalPages,
                    hasPrevious = payments.PageIndex > 1,
                    hasNext = payments.PageIndex < payments.TotalPages
                }));
                return Ok(payments.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult<PaymentDTO>> CreatePayment(PaymentCreateDTO paymentCreateDTO)
        {
            try
            {
                var payment = await _paymentService.CreatePaymentAsync(paymentCreateDTO);

                return Created(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignPaymentToBooking(
            [FromQuery] Guid paymentId,
            [FromQuery] Guid bookingId)
        {
            try
            {
                await _paymentService.AssignPaymentToBookingAsync(paymentId, bookingId);
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

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePayment(Guid paymentId)
        {
            try
            {
                await _paymentService.DeletePaymentAsync(paymentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpGet("stats")]
        [Authorize(Policy = "CanAccessReports")]
        public async Task<ActionResult<PaymentDTO>> GetPaymentStats(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date must be before end date");
                }
                var payment = await _paymentService.GetPaymentStatsAsync(startDate, endDate);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
