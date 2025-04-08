using AutoMapper;
using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.SalesChannel;
using HotelBooking.Application.Pagination;
using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Services
{
    public class SalesChannelService : ISalesChannelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SalesChannelService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }
        public async Task<SalesChannelDTO> CreateSalesChannelAsync(SalesChannelCreateDTO salesChannelCreateDTO)
        {
            if (!string.IsNullOrEmpty(salesChannelCreateDTO.Agent))
            {
                var existingChannel = await _unitOfWork.SalesChannels.GetByAgentAsync(salesChannelCreateDTO.Agent);
                if (existingChannel != null)
                {
                    throw new InvalidOperationException($"A sales channel with agent '{salesChannelCreateDTO.Agent}' already exists");
                }
            }
            var salesChannel = _mapper.Map<SalesChannel>(salesChannelCreateDTO);
            await _unitOfWork.SalesChannels.AddAsync(salesChannel);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<SalesChannelDTO>(salesChannel);
        }

        public async Task DeleteSalesChannelAsync(Guid id)
        {
            var salesChannel = await _unitOfWork.SalesChannels.GetByIdAsync(id);
            if (salesChannel == null)
            {
                throw new KeyNotFoundException($"Sales channel with ID {id} not found");
            }

            // Check if the sales channel is used in any bookings
            var hasBookings = await _unitOfWork.Bookings.AnyAsync(b => b.SalesChannelId == id);
            if (hasBookings)
            {
                throw new InvalidOperationException("Cannot delete a sales channel that is used in bookings");
            }

            await _unitOfWork.SalesChannels.DeleteAsync(salesChannel);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<SalesChannelDTO> GetSalesChannelByAgentAsync(string agent)
        {
            var salesChannel = await _unitOfWork.SalesChannels.GetByAgentAsync(agent);

            if (salesChannel == null)
            {
                throw new KeyNotFoundException($"Sales channel with agent '{agent}' not found");
            }

            return _mapper.Map<SalesChannelDTO>(salesChannel);
        }

        public async Task<SalesChannelDTO> GetSalesChannelByIdAsync(Guid id)
        {
            var salesChannel = await _unitOfWork.SalesChannels.GetByIdAsync(id);
            if (salesChannel == null) 
            {
                throw new KeyNotFoundException($"Sales channel with ID {id} not found");
            }
            return _mapper.Map<SalesChannelDTO>(salesChannel);
        }

        public async Task<PaginatedResponse<SalesChannelDTO>> GetAllSalesChannelsAsync(int pageIndex, int pageSize)
        {
            var channelsQuery = _unitOfWork.SalesChannels.Query()
                .OrderBy(sc => sc.MarketSegment)
                .ThenBy(sc => sc.DistributionChannel);

            var paginatedChannels = await PaginationHelper.CreateAsync(channelsQuery, pageIndex, pageSize);

            var channelDtos = _mapper.Map<List<SalesChannelDTO>>(paginatedChannels.Items);

            return new PaginatedResponse<SalesChannelDTO>
            {
                Items = channelDtos,
                PageIndex = paginatedChannels.PageIndex,
                PageSize = paginatedChannels.PageSize,
                TotalCount = paginatedChannels.TotalCount,
                TotalPages = paginatedChannels.TotalPages
            };
        }

        public async Task<PaginatedResponse<SalesChannelDTO>> GetSalesChannelsByMarketSegmentAsync(string segment, int pageIndex, int pageSize)
        {
            var channelsQuery = _unitOfWork.SalesChannels.Query()
                .Where(sc => sc.MarketSegment == segment)
                .OrderBy(sc => sc.DistributionChannel);

            var paginatedChannels = await PaginationHelper.CreateAsync(channelsQuery, pageIndex, pageSize);

            var channelDtos = _mapper.Map<List<SalesChannelDTO>>(paginatedChannels.Items);

            return new PaginatedResponse<SalesChannelDTO>
            {
                Items = channelDtos,
                PageIndex = paginatedChannels.PageIndex,
                PageSize = paginatedChannels.PageSize,
                TotalCount = paginatedChannels.TotalCount,
                TotalPages = paginatedChannels.TotalPages
            };
        }

        public async Task<SalesChannelStatsDTO> GetSalesChannelStatsAsync()
        {
            var allChannels = await _unitOfWork.SalesChannels.GetAllAsync();
            var channelsList = allChannels.ToList();

            // Get all bookings
            var allBookings = await _unitOfWork.Bookings.Query()
                .Include(b => b.SalesChannel)
                .ToListAsync();

            // Calculate stats
            int totalChannels = channelsList.Count;

            // Group channels by market segment
            var channelsByMarketSegment = channelsList
                .GroupBy(c => c.MarketSegment)
                .ToDictionary(g => g.Key, g => g.Count());

            // Group channels by distribution type
            var channelsByDistributionType = channelsList
                .GroupBy(c => c.DistributionChannel)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate booking stats by channel
            var bookingStatsByChannel = new Dictionary<string, BookingStatsDTO>();

            foreach (var channel in channelsList)
            {
                var channelBookings = allBookings.Where(b => b.SalesChannelId == channel.Id).ToList();

                if (channelBookings.Any())
                {
                    int totalBookings = channelBookings.Count;
                    int cancelledBookings = channelBookings.Count(b => b.IsCancelled);
                    double cancellationRate = (double)cancelledBookings / totalBookings * 100;

                    // Calculate total revenue
                    double totalRevenue = 0;
                    foreach (var booking in channelBookings.Where(b => !b.IsCancelled))
                    {
                        var totalNights = booking.StaysInWeekendNights + booking.StaysInWeekNights;
                        totalRevenue += booking.AverageDailyRate * totalNights;
                    }

                    // Calculate average booking value
                    double averageBookingValue = channelBookings.Count > 0
                        ? totalRevenue / channelBookings.Count(b => !b.IsCancelled)
                        : 0;

                    bookingStatsByChannel.Add(channel.MarketSegment + " - " + channel.DistributionChannel, new BookingStatsDTO
                    {
                        TotalBookings = totalBookings,
                        CancelledBookings = cancelledBookings,
                        CancellataionRate = Math.Round(cancellationRate, 2),
                        TotalRevenue = Math.Round(totalRevenue, 2),
                        AverageBookingValue = Math.Round(averageBookingValue, 2)
                    });
                }
            }

            return new SalesChannelStatsDTO
            {
                TotalChannels = totalChannels,
                ChannelsByMarketSegment = channelsByMarketSegment,
                ChannelsByDistributionType = channelsByDistributionType,
                BookingStatsByChannel = bookingStatsByChannel
            };
        }

        public async Task UpdateSalesChannelAsync(Guid id, SalesChannelUpdateDTO salesChannelUpdateDTO)
        {
            var salesChannel = await _unitOfWork.SalesChannels.GetByIdAsync(id);
            if (salesChannel == null)
            {
                throw new KeyNotFoundException($"Sales channel with ID {id} not found");
            }

            // Check for duplicate agent if being updated
            if (!string.IsNullOrEmpty(salesChannelUpdateDTO.Agent) &&
                salesChannelUpdateDTO.Agent != salesChannel.Agent)
            {
                var existingChannel = await _unitOfWork.SalesChannels.GetByAgentAsync(salesChannelUpdateDTO.Agent);
                if (existingChannel != null && existingChannel.Id != id)
                {
                    throw new InvalidOperationException($"A sales channel with agent '{salesChannelUpdateDTO.Agent}' already exists");
                }
            }

            _mapper.Map(salesChannelUpdateDTO, salesChannel);
            await _unitOfWork.SalesChannels.UpdateAsync(salesChannel);
            await _unitOfWork.CompleteAsync();
        }
    }
}
