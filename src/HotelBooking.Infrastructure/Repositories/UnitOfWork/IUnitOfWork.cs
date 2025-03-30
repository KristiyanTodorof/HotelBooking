using HotelBooking.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IBookingRepository Bookings { get; }
        IBookingDetailsRepository BookingsDetails { get; }
        IGuestRepository Guests { get; }
        IPaymentRepository Payments { get; }
        IRoomRepository Rooms { get; }
        ISalesChannelRepository SalesChannels { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
