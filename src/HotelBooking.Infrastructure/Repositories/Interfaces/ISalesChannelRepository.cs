using HotelBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories.Interfaces
{
    public interface ISalesChannelRepository : IRepository<SalesChannel, Guid>
    {
        Task<IEnumerable<SalesChannel>> GetByMarketSegmentAsync(string marketSegment);
        Task<SalesChannel> GetByAgentAsync(string agent);
    }
}
