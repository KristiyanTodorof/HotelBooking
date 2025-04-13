using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.SalesChannel;
using HotelBooking.Web.API.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace HotelBooking.Web.API.Controllers
{
    [ApiController]
    [Route("api/sales-channels")]
    public class SalesChannelsController : BaseApiController
    {
        private readonly ISalesChannelService _salesChannelService;
        public SalesChannelsController(ISalesChannelService salesChannelService)
        {
            this._salesChannelService = salesChannelService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesChannelDTO>>> GetSalesChannels(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var salesChannels = await _salesChannelService.GetAllSalesChannelsAsync(
                    pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = salesChannels.TotalCount,
                    pageSize = salesChannels.PageSize,
                    currentPage = salesChannels.PageIndex,
                    totalPages = salesChannels.TotalPages,
                    hasPrevious = salesChannels.PageIndex > 1,
                    hasNext = salesChannels.PageIndex < salesChannels.TotalPages
                }));

                return Ok(salesChannels.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesChannelDTO>> GetSalesChannel(Guid id)
        {
            try
            {
                var salesChannel = await _salesChannelService.GetSalesChannelByIdAsync(id);
                return Ok(salesChannel);
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

        [HttpGet("market-segment/{segment}")]
        public async Task<ActionResult<IEnumerable<SalesChannelDTO>>> GetSalesChannelsByMarketSegment(
            string segment,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var salesChannels = await _salesChannelService.GetSalesChannelsByMarketSegmentAsync(
                    segment, pageIndex, pageSize);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    totalCount = salesChannels.TotalCount,
                    pageSize = salesChannels.PageSize,
                    currentPage = salesChannels.PageIndex,
                    totalPages = salesChannels.TotalPages,
                    hasPrevious = salesChannels.PageIndex > 1,
                    hasNext = salesChannels.PageIndex < salesChannels.TotalPages
                }));

                return Ok(salesChannels.Items);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpGet("agent/{agent}")]
        public async Task<ActionResult<IEnumerable<SalesChannelDTO>>> GetSalesChannelByAgent(string agent)
        {
            try
            {
                var salesChannel = await _salesChannelService.GetSalesChannelByAgentAsync(agent);
                return Ok(salesChannel);
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
        public async Task<ActionResult<SalesChannelDTO>> CreateSalesChannel(SalesChannelCreateDTO 
            salesChannelCreateDTO)
        {
            try
            {
                var salesChannel = await _salesChannelService.CreateSalesChannelAsync(salesChannelCreateDTO);
                return Created(nameof(salesChannel), new {id = salesChannel.Id}, salesChannel);
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
        [Authorize(Policy = "CanManageBookings")]
        public async Task<IActionResult> UpdateSalesChannel(Guid id, SalesChannelUpdateDTO updateDTO)
        {
            try
            {
                await _salesChannelService.UpdateSalesChannelAsync(id, updateDTO);
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
        [Authorize(Policy = "CanManageBookings")]
        public async Task<IActionResult> DeleteSalesChannel(Guid id)
        {
            try
            {
                await _salesChannelService.DeleteSalesChannelAsync(id);
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
        public async Task<ActionResult<SalesChannelStatsDTO>> GetSalesChannelStats()
        {
            try
            {
                var stats = await _salesChannelService.GetSalesChannelStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
