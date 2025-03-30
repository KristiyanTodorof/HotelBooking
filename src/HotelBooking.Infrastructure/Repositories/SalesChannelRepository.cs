using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Data;
using HotelBooking.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories
{
    public class SalesChannelRepository : Repository<SalesChannel, Guid>, ISalesChannelRepository
    {
        public SalesChannelRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<SalesChannel> GetByAgentAsync(string agent)
        {
            return await _context.SalesChannels
                .FirstOrDefaultAsync(sc => sc.Agent == agent);
        }

        public async Task<IEnumerable<SalesChannel>> GetByMarketSegmentAsync(string marketSegment)
        {
            return await _context.SalesChannels
                .Where(sc => sc.MarketSegment == marketSegment)
                .ToListAsync();
        }
    }
}
