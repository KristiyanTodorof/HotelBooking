using HotelBooking.Infrastructure.Data;
using HotelBooking.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed = false;

        private IBookingRepository _bookingRepository;
        private IBookingDetailsRepository _bookingDetailsRepository;
        private IGuestRepository _guestRepository;
        private IPaymentRepository _paymentRepository;
        private IRoomRepository _roomRepository;
        private ISalesChannelRepository _salesChannelRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            this._context = context;
        }

        public IBookingRepository Bookings => _bookingRepository ??= new BookingRepository(_context);
        public IBookingDetailsRepository BookingsDetails => _bookingDetailsRepository ??= new BookingDetailsRepository(_context);
        public IGuestRepository Guests => _guestRepository ??= new GuestRepository(_context);
        public IPaymentRepository Payments => _paymentRepository ??= new PaymentRepository(_context);
        public IRoomRepository Rooms => _roomRepository ??= new RoomRepository(_context);
        public ISalesChannelRepository SalesChannels => _salesChannelRepository ??= new SalesChannelRepository(_context);


        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _transaction?.CommitAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction?.RollbackAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
