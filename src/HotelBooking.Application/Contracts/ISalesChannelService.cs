using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.SalesChannel;
using HotelBooking.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Contracts
{
    public interface ISalesChannelService
    {
        Task<PaginatedResponse<SalesChannelDTO>> GetAllSalesChannelsAsync(int pageIndex, int pageSize);
        Task<SalesChannelDTO> GetSalesChannelByIdAsync(Guid id);
        Task<PaginatedResponse<SalesChannelDTO>> GetSalesChannelsByMarketSegmentAsync(string segment,
            int pageIndex, int pageSize);
        Task<SalesChannelDTO> GetSalesChannelByAgentAsync(string agent);
        Task<SalesChannelDTO> CreateSalesChannelAsync(SalesChannelCreateDTO salesChannelCreateDTO);
        Task UpdateSalesChannelAsync(Guid id, SalesChannelUpdateDTO salesChannelUpdateDTO);
        Task DeleteSalesChannelAsync(Guid id);
        Task<SalesChannelStatsDTO> GetSalesChannelStatsAsync();
    }
}
