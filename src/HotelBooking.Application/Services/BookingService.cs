using AutoMapper;
using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Booking;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.Pagination;
using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }
        public async Task CancelBookingAsync(Guid id)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {id} not found");
            }

            if (booking.IsCancelled)
            {
                throw new InvalidOperationException("Booking is already cancelled");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                booking.IsCancelled = true;
                booking.ReservationStatus = "Cancelled";
                await _unitOfWork.Bookings.UpdateAsync(booking);

                // Update guest's booking history
                var guest = await _unitOfWork.Guests.GetByIdAsync(booking.GuestId);
                if (guest != null)
                {
                    guest.PreviousCancellations += 1;
                    guest.PreviousBookingsNotCancelled -= 1;

                    // Update IsRepeatedGuest flag
                    if (guest.PreviousBookingsNotCancelled <= 0)
                    {
                        guest.IsRepeatedGuest = false;
                    }

                    await _unitOfWork.Guests.UpdateAsync(guest);
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<BookingDTO> CreateBookingAsync(BookingCreateDTO bookingCreateDTO)
        {
            // Validate the guest exists
            var guest = await _unitOfWork.Guests.GetByIdAsync(bookingCreateDTO.GuestId);
            if (guest == null)
            {
                throw new KeyNotFoundException($"Guest with ID {bookingCreateDTO.GuestId} not found");
            }

            // Validate the room exists
            var room = await _unitOfWork.Rooms.GetByIdAsync(bookingCreateDTO.RoomId);
            if (room == null)
            {
                throw new KeyNotFoundException($"Room with ID {bookingCreateDTO.RoomId} not found");
            }

            // Validate the sales channel exists
            var salesChannel = await _unitOfWork.SalesChannels.GetByIdAsync(bookingCreateDTO.SalesChannelId);
            if (salesChannel == null)
            {
                throw new KeyNotFoundException($"Sales channel with ID {bookingCreateDTO.SalesChannelId} not found");
            }

            // Check room availability for the specified dates
            var arrivalDate = new DateTime(
                bookingCreateDTO.ArrivalDateYear,
                DateTime.ParseExact(bookingCreateDTO.ArrivalDateMonth, "MMMM", CultureInfo.InvariantCulture).Month,
                bookingCreateDTO.ArrivalDateDayOfMonth);

            var departureDate = arrivalDate.AddDays(bookingCreateDTO.StaysInWeekendNights + bookingCreateDTO.StaysInWeekNights);

            var isAvailable = await IsRoomAvailableAsync(bookingCreateDTO.RoomId, arrivalDate, departureDate);
            if (!isAvailable)
            {
                throw new InvalidOperationException($"Room is not available for the selected dates");
            }

            try
            {
                // Start a transaction for creating both booking and booking details
                await _unitOfWork.BeginTransactionAsync();

                // Create booking details first
                var bookingDetails = _mapper.Map<BookingDetails>(bookingCreateDTO.BookingDetails);
                await _unitOfWork.BookingsDetails.AddAsync(bookingDetails);
                await _unitOfWork.CompleteAsync();

                // Create the booking with reference to the details
                var booking = _mapper.Map<Booking>(bookingCreateDTO);
                booking.BookingDetailsId = bookingDetails.Id;

                // Calculate and set the average daily rate based on room base rate
                booking.AverageDailyRate = CalculateAverageDailyRate(room.BaseRate, bookingCreateDTO);

                // Handle payment if provided
                if (bookingCreateDTO.Payment != null)
                {
                    var payment = _mapper.Map<Payment>(bookingCreateDTO.Payment);
                    await _unitOfWork.Payments.AddAsync(payment);
                    await _unitOfWork.CompleteAsync();
                    booking.PaymentId = payment.Id;
                }

                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.CompleteAsync();

                // Commit the transaction
                await _unitOfWork.CommitTransactionAsync();

                // Update guest's booking history
                guest.PreviousBookingsNotCancelled += 1;
                if (guest.PreviousBookingsNotCancelled > 0)
                {
                    guest.IsRepeatedGuest = true;
                }
                await _unitOfWork.Guests.UpdateAsync(guest);
                await _unitOfWork.CompleteAsync();

                // Return the newly created booking
                var result = await _unitOfWork.Bookings.Query()
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .Include(b => b.BookingDetails)
                    .Include(b => b.Payment)
                    .Include(b => b.SalesChannel)
                    .FirstOrDefaultAsync(b => b.Id == booking.Id);

                return _mapper.Map<BookingDTO>(result);
            }
            catch (Exception)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteBookingAsync(Guid id)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {id} not found");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Get related entities to delete
                var bookingDetails = await _unitOfWork.BookingsDetails.GetByIdAsync(booking.BookingDetailsId);

                Payment payment = null;
                if (booking.PaymentId.HasValue)
                {
                    payment = await _unitOfWork.Payments.GetByIdAsync(booking.PaymentId.Value);
                }

                // Update guest's booking history
                var guest = await _unitOfWork.Guests.GetByIdAsync(booking.GuestId);
                if (guest != null)
                {
                    if (booking.IsCancelled)
                    {
                        guest.PreviousCancellations = Math.Max(0, guest.PreviousCancellations - 1);
                    }
                    else
                    {
                        guest.PreviousBookingsNotCancelled = Math.Max(0, guest.PreviousBookingsNotCancelled - 1);
                    }

                    // Update IsRepeatedGuest flag
                    if (guest.PreviousBookingsNotCancelled <= 0)
                    {
                        guest.IsRepeatedGuest = false;
                    }

                    await _unitOfWork.Guests.UpdateAsync(guest);
                    await _unitOfWork.CompleteAsync();
                }

                // Delete the booking and related entities
                await _unitOfWork.Bookings.DeleteAsync(booking);

                if (bookingDetails != null)
                {
                    await _unitOfWork.BookingsDetails.DeleteAsync(bookingDetails);
                }

                if (payment != null)
                {
                    await _unitOfWork.Payments.DeleteAsync(payment);
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PaginatedResponse<BookingDTO>> GetActiveBookingsAsync(int pageIndex, int pageSize)
        {
            var bookingsQuery = _unitOfWork.Bookings.Query()
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Include(b => b.BookingDetails)
                .Include(b => b.Payment)
                .Include(b => b.SalesChannel)
                .Where(b => !b.IsCancelled && b.ReservationStatus == "Confirmed")
                .OrderByDescending(b => b.ReservationStatusDate);

            var paginatedBookings = await PaginationHelper.CreateAsync(bookingsQuery, pageIndex, pageSize);

            var bookingDtos = _mapper.Map<List<BookingDTO>>(paginatedBookings.Items);

            return new PaginationHelper.PaginatedResponse<BookingDTO>
            {
                Items = bookingDtos,
                PageIndex = paginatedBookings.PageIndex,
                PageSize = paginatedBookings.PageSize,
                TotalCount = paginatedBookings.TotalCount,
                TotalPages = paginatedBookings.TotalPages
            };
        }

        public async Task<PaginatedResponse<BookingDTO>> GetAllBookingsAsync(int pageIndex, int pageSize)
        {
            var bookingsQuery = _unitOfWork.Bookings.Query()
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Include(b => b.BookingDetails)
                .Include(b => b.Payment)
                .Include(b => b.SalesChannel)
                .OrderByDescending(b => b.ReservationStatusDate);

            var paginatedBookings = await PaginationHelper.CreateAsync(bookingsQuery, pageIndex, pageSize);

            var bookingDtos = _mapper.Map<List<BookingDTO>>(paginatedBookings.Items);

            return new PaginatedResponse<BookingDTO>
            {
                Items = bookingDtos,
                PageIndex = paginatedBookings.PageIndex,
                PageSize = paginatedBookings.PageSize,
                TotalCount = paginatedBookings.TotalCount,
                TotalPages = paginatedBookings.TotalPages
            };
        }

        public async Task<PaginatedResponse<BookingDTO>> GetBookingByDateRangeAsync(DateTime startDate, DateTime endDate, int pageIndex, int pageSize)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            var bookingsQuery = _unitOfWork.Bookings.Query()
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Include(b => b.BookingDetails)
                .Include(b => b.Payment)
                .Include(b => b.SalesChannel)
                .Where(b =>
                    // Convert stored string date to DateTime for comparison
                    (GetArrivalDate(b) >= startDate && GetArrivalDate(b) <= endDate) ||
                    (GetDepartureDate(b) >= startDate && GetDepartureDate(b) <= endDate) ||
                    (GetArrivalDate(b) <= startDate && GetDepartureDate(b) >= endDate)
                )
                .OrderByDescending(b => b.ReservationStatusDate);

            var paginatedBookings = await PaginationHelper.CreateAsync(bookingsQuery, pageIndex, pageSize);

            var bookingDtos = _mapper.Map<List<BookingDTO>>(paginatedBookings.Items);

            return new PaginationHelper.PaginatedResponse<BookingDTO>
            {
                Items = bookingDtos,
                PageIndex = paginatedBookings.PageIndex,
                PageSize = paginatedBookings.PageSize,
                TotalCount = paginatedBookings.TotalCount,
                TotalPages = paginatedBookings.TotalPages
            };
        }

        public async Task<PaginatedResponse<BookingDTO>> GetBookingByGuestAsync(Guid guestId, int pageIndex, int pageSize)
        {
            // Check if guest exists
            var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
            if (guest == null)
            {
                throw new KeyNotFoundException($"Guest with ID {guestId} not found");
            }

            var bookingsQuery = _unitOfWork.Bookings.Query()
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Include(b => b.BookingDetails)
                .Include(b => b.Payment)
                .Include(b => b.SalesChannel)
                .Where(b => b.GuestId == guestId)
                .OrderByDescending(b => b.ReservationStatusDate);

            var paginatedBookings = await PaginationHelper.CreateAsync(bookingsQuery, pageIndex, pageSize);

            var bookingDtos = _mapper.Map<List<BookingDTO>>(paginatedBookings.Items);

            return new PaginationHelper.PaginatedResponse<BookingDTO>
            {
                Items = bookingDtos,
                PageIndex = paginatedBookings.PageIndex,
                PageSize = paginatedBookings.PageSize,
                TotalCount = paginatedBookings.TotalCount,
                TotalPages = paginatedBookings.TotalPages
            };
        }

        public async Task<BookingDTO> GetBookingByIdAsync(Guid id)
        {
            var booking = await _unitOfWork.Bookings.Query()
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Include(b => b.BookingDetails)
                .Include(b => b.Payment)
                .Include(b => b.SalesChannel)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {id} not found");
            }

            return _mapper.Map<BookingDTO>(booking);
        }

        public async Task<OccupancyStatsDTO> GetOccupancyStatsAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Get all rooms
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();
            var totalRooms = allRooms.Count();

            // Get all active bookings in the date range
            var activeBookings = await _unitOfWork.Bookings.Query()
                .Include(b => b.Room)
                .Where(b => !b.IsCancelled && b.ReservationStatus == "Confirmed")
                .ToListAsync();

            // Calculate total days in the range
            var totalDays = (endDate - startDate).Days + 1;

            // Initialize room occupancy tracking
            var roomOccupancyDays = new Dictionary<Guid, int>();
            var roomTypeOccupancy = new Dictionary<string, int>();

            // Initialize total revenue
            double totalRevenue = 0;

            // For each day in the range
            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                // For each room, check if it's occupied on this day
                foreach (var room in allRooms)
                {
                    bool isOccupied = activeBookings.Any(b =>
                    {
                        var bookingArrivalDate = GetArrivalDate(b);
                        var bookingDepartureDate = GetDepartureDate(b);

                        return b.RoomId == room.Id &&
                               (day >= bookingArrivalDate && day < bookingDepartureDate);
                    });

                    if (isOccupied)
                    {
                        // Update room occupancy counter
                        if (!roomOccupancyDays.ContainsKey(room.Id))
                        {
                            roomOccupancyDays[room.Id] = 0;
                        }
                        roomOccupancyDays[room.Id]++;

                        // Update room type occupancy counter
                        if (!roomTypeOccupancy.ContainsKey(room.ReservedRoomType))
                        {
                            roomTypeOccupancy[room.ReservedRoomType] = 0;
                        }
                        roomTypeOccupancy[room.ReservedRoomType]++;

                        // Add room rate to total revenue
                        var booking = activeBookings.FirstOrDefault(b => b.RoomId == room.Id);
                        if (booking != null)
                        {
                            totalRevenue += booking.AverageDailyRate;
                        }
                    }
                }
            }

            // Calculate occupied room-nights
            var occupiedRoomNights = roomOccupancyDays.Values.Sum();

            // Calculate total available room-nights in the period
            var totalRoomNights = totalRooms * totalDays;

            // Calculate occupancy rate
            var occupancyRate = totalRoomNights > 0 ? (double)occupiedRoomNights / totalRoomNights * 100 : 0;

            // Calculate average daily rate
            var averageDailyRate = occupiedRoomNights > 0 ? totalRevenue / occupiedRoomNights : 0;

            // Calculate RevPAR (Revenue Per Available Room)
            var revPAR = totalRoomNights > 0 ? totalRevenue / totalRoomNights : 0;

            return new OccupancyStatsDTO
            {
                OccupancyRate = Math.Round(occupancyRate, 2),
                TotalRooms = totalRooms,
                OccupiedRooms = roomOccupancyDays.Count,
                AverageDailyRate = Math.Round(averageDailyRate, 2),
                RevPAR = Math.Round(revPAR, 2),
                OccupancyByRoomType = roomTypeOccupancy
            };
        }

        public async Task<IEnumerable<BookingDTO>> GetUpcomingCheckInsAsync(DateTime date)
        {
            // Get all check-ins for the specified date
            var checkIns = await _unitOfWork.Bookings.Query()
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Include(b => b.BookingDetails)
                .Include(b => b.Payment)
                .Include(b => b.SalesChannel)
                .Where(b => !b.IsCancelled &&
                           b.ReservationStatus == "Confirmed" &&
                           b.ArrivalDateYear == date.Year &&
                           b.ArrivalDateMonth == date.ToString("MMMM") &&
                           b.ArrivalDateDayOfMonth == date.Day)
                .OrderBy(b => b.ArrivalDateDayOfMonth)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BookingDTO>>(checkIns);
        }

        public async Task<IEnumerable<BookingDTO>> GetUpcomingCheckOutsAsync(DateTime date)
        {
            // Get all active bookings
            var activeBookings = await _unitOfWork.Bookings.Query()
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Include(b => b.BookingDetails)
                .Include(b => b.Payment)
                .Include(b => b.SalesChannel)
                .Where(b => !b.IsCancelled &&
                           b.ReservationStatus == "Confirmed" ||
                           b.ReservationStatus == "CheckedIn")
                .ToListAsync();

            // Filter bookings where departure date matches the specified date
            var checkOuts = activeBookings.Where(b =>
            {
                var arrivalDate = GetArrivalDate(b);
                var departureDate = GetDepartureDate(b);

                return departureDate.Date == date.Date;
            })
            .OrderBy(b => b.ArrivalDateDayOfMonth)
            .ToList();

            return _mapper.Map<IEnumerable<BookingDTO>>(checkOuts);
        }

        public async Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Get all bookings for the specified room
            var roomBookings = await _unitOfWork.Bookings.Query()
                .Where(b => b.RoomId == roomId &&
                           !b.IsCancelled &&
                           b.ReservationStatus == "Confirmed")
                .ToListAsync();

            // Check if any booking overlaps with the requested dates
            foreach (var booking in roomBookings)
            {
                var bookingArrivalDate = GetArrivalDate(booking);
                var bookingDepartureDate = GetDepartureDate(booking);

                // Check for date overlap
                if ((startDate <= bookingDepartureDate && endDate >= bookingArrivalDate))
                {
                    return false; // Room is not available due to overlap
                }
            }

            return true; // Room is available
        }

        public async Task<BookingDTO> UpdateBookingAsync(Guid id, BookingUpdateDTO bookingUpdateDTO)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {id} not found");
            }

            try
            {
                // Start transaction
                await _unitOfWork.BeginTransactionAsync();

                // Check if room is being changed
                if (bookingUpdateDTO.RoomId.HasValue && bookingUpdateDTO.RoomId.Value != booking.RoomId)
                {
                    // Validate the new room exists
                    var newRoom = await _unitOfWork.Rooms.GetByIdAsync(bookingUpdateDTO.RoomId.Value);
                    if (newRoom == null)
                    {
                        throw new KeyNotFoundException($"Room with ID {bookingUpdateDTO.RoomId.Value} not found");
                    }

                    // Check if the new room is available for the booking dates
                    var arrivalDate = GetArrivalDate(booking);
                    var departureDate = GetDepartureDate(booking);

                    var isAvailable = await IsRoomAvailableForBookingChange(bookingUpdateDTO.RoomId.Value, arrivalDate, departureDate, id);
                    if (!isAvailable)
                    {
                        throw new InvalidOperationException($"The selected room is not available for the booking dates");
                    }

                    // Update base rate if room changed
                    if (bookingUpdateDTO.AverageDailyRate == null)
                    {
                        bookingUpdateDTO.AverageDailyRate = newRoom.BaseRate;
                    }
                }

                // Update booking
                _mapper.Map(bookingUpdateDTO, booking);
                await _unitOfWork.Bookings.UpdateAsync(booking);

                // Update booking details if provided
                if (bookingUpdateDTO.BookingDetails != null)
                {
                    var bookingDetails = await _unitOfWork.BookingsDetails.GetByIdAsync(booking.BookingDetailsId);
                    if (bookingDetails != null)
                    {
                        _mapper.Map(bookingUpdateDTO.BookingDetails, bookingDetails);
                        await _unitOfWork.BookingsDetails.UpdateAsync(bookingDetails);
                    }
                }

                await _unitOfWork.CompleteAsync();

                // Commit the transaction
                await _unitOfWork.CommitTransactionAsync();

                // Return Task.CompletedTask to ensure a value is returned
                return null;
            }
            catch (Exception)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #region Helper Methods

        private DateTime GetArrivalDate(Booking booking)
        {
            var month = DateTime.ParseExact(booking.ArrivalDateMonth, "MMMM", CultureInfo.InvariantCulture).Month;
            return new DateTime(booking.ArrivalDateYear, month, booking.ArrivalDateDayOfMonth);
        }

        private DateTime GetDepartureDate(Booking booking)
        {
            var arrivalDate = GetArrivalDate(booking);
            return arrivalDate.AddDays(booking.StaysInWeekendNights + booking.StaysInWeekNights);
        }

        private double CalculateAverageDailyRate(double baseRate, BookingCreateDTO bookingDto)
        {
            // Base calculation on room's base rate
            double totalRate = baseRate;

            // Apply adjustments based on booking details
            if (bookingDto.BookingDetails != null)
            {
                // Adjust for meal plan
                switch (bookingDto.BookingDetails.Meal)
                {
                    case "HB": // Half Board
                        totalRate += 15; // Add $15 per night for half board
                        break;
                    case "FB": // Full Board
                        totalRate += 30; // Add $30 per night for full board
                        break;
                }

                // Adjust for special requests
                totalRate += bookingDto.BookingDetails.TotalOfSpecialRequests * 5; // $5 per special request

                // Adjust for additional guests
                var extraGuests = bookingDto.BookingDetails.Adults - 2;
                if (extraGuests > 0)
                {
                    totalRate += extraGuests * 25; // $25 per additional adult
                }

                // Adjust for children
                totalRate += bookingDto.BookingDetails.Children * 15; // $15 per child
            }

            // Calculate average daily rate
            return Math.Round(totalRate, 2);
        }

        private async Task<bool> IsRoomAvailableForBookingChange(Guid roomId, DateTime startDate, DateTime endDate, Guid currentBookingId)
        {
            // Get all bookings for the specified room
            var roomBookings = await _unitOfWork.Bookings.Query()
                .Where(b => b.RoomId == roomId &&
                           b.Id != currentBookingId &&
                           !b.IsCancelled &&
                           b.ReservationStatus == "Confirmed")
                .ToListAsync();

            // Check if any booking overlaps with the requested dates
            foreach (var booking in roomBookings)
            {
                var bookingArrivalDate = GetArrivalDate(booking);
                var bookingDepartureDate = GetDepartureDate(booking);

                // Check for date overlap
                if ((startDate <= bookingDepartureDate && endDate >= bookingArrivalDate))
                {
                    return false; // Room is not available due to overlap
                }
            }

            return true; // Room is available
        }

        #endregion
    }
}
