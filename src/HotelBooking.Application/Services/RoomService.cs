using AutoMapper;
using HotelBooking.Application.Contracts;
using HotelBooking.Application.DTO.Custom;
using HotelBooking.Application.DTO.Room;
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
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }
        public async Task<RoomDTO> CreateRoomAsync(RoomCreateDTO roomCreateDTO)
        {
            var existingRoom = await _unitOfWork.Rooms.GetByRoomNumberAsync(roomCreateDTO.RoomNumber);
            if (existingRoom != null)
            {
                throw new InvalidOperationException($"A room with number {roomCreateDTO.RoomNumber} already exists");
            }

            var room = _mapper.Map<Room>(roomCreateDTO);

            await _unitOfWork.Rooms.AddAsync(room);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<RoomDTO>(room);
        }

        public async Task DeleteRoomAsync(Guid id)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);

            if (room == null)
            {
                throw new KeyNotFoundException($"Room with ID {id} not found");
            }

            // Check if the room has any bookings
            var hasBookings = await _unitOfWork.Bookings.AnyAsync(b => b.RoomId == id);
            if (hasBookings)
            {
                throw new InvalidOperationException("Cannot delete a room with associated bookings");
            }

            await _unitOfWork.Rooms.DeleteAsync(room);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaginatedResponse<RoomDTO>> GetAllRoomsAsync(int pageIndex, int pageSize)
        {
            var roomsQuery = _unitOfWork.Rooms.Query()
                .OrderBy(r => r.RoomNumber);

            var paginatedRooms = await PaginationHelper.CreateAsync(roomsQuery, pageIndex, pageSize);

            var roomDtos = _mapper.Map<List<RoomDTO>>(paginatedRooms.Items);

            return new PaginatedResponse<RoomDTO>
            {
                Items = roomDtos,
                PageIndex = paginatedRooms.PageIndex,
                PageSize = paginatedRooms.PageSize,
                TotalCount = paginatedRooms.TotalCount,
                TotalPages = paginatedRooms.TotalPages
            };
        }

        public async Task<Dictionary<string, int>> GetAvailabilityByRoomTypeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Get all rooms
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();

            // Group rooms by type
            var roomsByType = allRooms.GroupBy(r => r.ReservedRoomType)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Get all bookings that overlap with the date range
            var bookings = await _unitOfWork.Bookings.Query()
                .Where(b => !b.IsCancelled && b.ReservationStatus == "Confirmed")
                .ToListAsync();

            var availabilityByType = new Dictionary<string, int>();

            // Calculate availability for each room type
            foreach (var roomType in roomsByType.Keys)
            {
                var roomsOfType = roomsByType[roomType];
                int availableCount = 0;

                foreach (var room in roomsOfType)
                {
                    bool isAvailable = true;

                    foreach (var booking in bookings.Where(b => b.RoomId == room.Id))
                    {
                        var bookingStartDate = GetBookingStartDate(booking);
                        var bookingEndDate = GetBookingEndDate(booking);

                        // Check for date overlap
                        if (startDate < bookingEndDate && endDate > bookingStartDate)
                        {
                            isAvailable = false;
                            break;
                        }
                    }

                    if (isAvailable)
                    {
                        availableCount++;
                    }
                }

                availabilityByType.Add(roomType, availableCount);
            }

            return availabilityByType;
        }

        public async Task<PaginatedResponse<RoomDTO>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate, int pageIndex, int pageSize)
        {
            if (startDate >= endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Get all rooms
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();

            // Get all bookings that overlap with the date range
            var bookings = await _unitOfWork.Bookings.Query()
                .Where(b => !b.IsCancelled && b.ReservationStatus == "Confirmed")
                .ToListAsync();

            // Filter out rooms that have overlapping bookings
            var availableRooms = new List<Room>();

            foreach (var room in allRooms)
            {
                bool isAvailable = true;

                foreach (var booking in bookings.Where(b => b.RoomId == room.Id))
                {
                    var bookingStartDate = GetBookingStartDate(booking);
                    var bookingEndDate = GetBookingEndDate(booking);

                    // Check for date overlap
                    if (startDate < bookingEndDate && endDate > bookingStartDate)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                {
                    availableRooms.Add(room);
                }
            }

            // Apply pagination to the available rooms list
            var totalCount = availableRooms.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // Adjust page index if it exceeds total pages
            if (pageIndex > totalPages && totalCount > 0)
            {
                pageIndex = totalPages;
            }

            var paginatedRooms = availableRooms
                .OrderBy(r => r.RoomNumber)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var roomDtos = _mapper.Map<List<RoomDTO>>(paginatedRooms);

            return new PaginatedResponse<RoomDTO>
            {
                Items = roomDtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<RoomDTO> GetRoomByIdAsync(Guid id)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);

            if (room == null)
            {
                throw new KeyNotFoundException($"Room with ID {id} not found");
            }

            return _mapper.Map<RoomDTO>(room);
        }

        public async Task<RoomDTO> GetRoomByNumberAsync(string roomNumber)
        {
            var room = await _unitOfWork.Rooms.GetByRoomNumberAsync(roomNumber);

            if (room == null)
            {
                throw new KeyNotFoundException($"Room with number {roomNumber} not found");
            }

            return _mapper.Map<RoomDTO>(room);
        }

        public async Task<RoomRevenueStatsDTO> GetRoomRevenueStatsAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Get all rooms
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();

            // Get all bookings in the date range
            var bookings = await _unitOfWork.Bookings.Query()
                .Include(b => b.Room)
                .Where(b => !b.IsCancelled)
                .ToListAsync();

            // Filter to bookings that overlap with the date range
            var relevantBookings = bookings.Where(b =>
            {
                var bookingStartDate = GetBookingStartDate(b);
                var bookingEndDate = GetBookingEndDate(b);

                return (bookingStartDate <= endDate && bookingEndDate >= startDate);
            }).ToList();

            // Calculate total revenue
            double totalRevenue = 0;

            // Calculate revenue by room type
            var revenueByRoomType = new Dictionary<string, double>();

            // Calculate revenue by room
            var revenueByRoom = new Dictionary<Guid, RoomRevenueItemDTO>();

            foreach (var booking in relevantBookings)
            {
                if (booking.Room == null) continue;

                // Calculate the overlap days between booking and the date range
                var bookingStartDate = GetBookingStartDate(booking);
                var bookingEndDate = GetBookingEndDate(booking);

                var overlapStart = bookingStartDate < startDate ? startDate : bookingStartDate;
                var overlapEnd = bookingEndDate > endDate ? endDate : bookingEndDate;
                var overlapDays = (overlapEnd - overlapStart).Days;

                if (overlapDays <= 0) continue;

                // Calculate the revenue for this booking within the date range
                var bookingRevenue = booking.AverageDailyRate * overlapDays;

                // Add to total revenue
                totalRevenue += bookingRevenue;

                // Add to revenue by room type
                var roomType = booking.Room.ReservedRoomType;
                if (!revenueByRoomType.ContainsKey(roomType))
                {
                    revenueByRoomType[roomType] = 0;
                }
                revenueByRoomType[roomType] += bookingRevenue;

                // Add to revenue by room
                if (!revenueByRoom.ContainsKey(booking.RoomId))
                {
                    revenueByRoom[booking.RoomId] = new RoomRevenueItemDTO
                    {
                        RoomId = booking.RoomId,
                        RoomNumber = booking.Room.RoomNumber,
                        RoomType = booking.Room.ReservedRoomType,
                        Revenue = 0,
                        Bookings = 0,
                        OccupancyRate = 0
                    };
                }

                revenueByRoom[booking.RoomId].Revenue += bookingRevenue;
                revenueByRoom[booking.RoomId].Bookings++;
            }

            // Calculate total days in the range
            var totalDays = (endDate - startDate).Days + 1;

            // Calculate occupancy rates for each room
            foreach (var roomId in revenueByRoom.Keys.ToList())
            {
                var room = allRooms.FirstOrDefault(r => r.Id == roomId);
                if (room == null) continue;

                var occupiedDays = 0;

                // For each day in the date range
                for (var day = startDate; day <= endDate; day = day.AddDays(1))
                {
                    // Check if the room is occupied on this day
                    bool isOccupied = relevantBookings.Any(b =>
                    {
                        if (b.RoomId != roomId) return false;

                        var bookingStartDate = GetBookingStartDate(b);
                        var bookingEndDate = GetBookingEndDate(b);

                        return day >= bookingStartDate && day < bookingEndDate;
                    });

                    if (isOccupied)
                    {
                        occupiedDays++;
                    }
                }

                // Calculate occupancy rate
                revenueByRoom[roomId].OccupancyRate = Math.Round((double)occupiedDays / totalDays * 100, 2);
            }

            // Get top performing rooms by revenue
            var topPerformingRooms = revenueByRoom.Values
                .OrderByDescending(r => r.Revenue)
                .Take(10)
                .ToList();

            // Calculate average room rate
            double averageRoomRate = relevantBookings.Count > 0
                ? relevantBookings.Average(b => b.AverageDailyRate)
                : 0;

            return new RoomRevenueStatsDTO
            {
                TotalRevenue = Math.Round(totalRevenue, 2),
                RevenueByRoomType = revenueByRoomType.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Math.Round(kvp.Value, 2)),
                TopPerformingRooms = topPerformingRooms,
                AverageRoomRate = Math.Round(averageRoomRate, 2)
            };
        }

        public async Task<PaginatedResponse<RoomDTO>> GetRoomsByTypeAsync(string roomType, int pageIndex, int pageSize)
        {
            var roomsQuery = _unitOfWork.Rooms.Query()
                .Where(r => r.ReservedRoomType == roomType)
                .OrderBy(r => r.RoomNumber);

            var paginatedRooms = await PaginationHelper.CreateAsync(roomsQuery, pageIndex, pageSize);

            var roomDtos = _mapper.Map<List<RoomDTO>>(paginatedRooms.Items);

            return new PaginatedResponse<RoomDTO>
            {
                Items = roomDtos,
                PageIndex = paginatedRooms.PageIndex,
                PageSize = paginatedRooms.PageSize,
                TotalCount = paginatedRooms.TotalCount,
                TotalPages = paginatedRooms.TotalPages
            };
        }

        public async Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Get the room to make sure it exists
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
            if (room == null)
            {
                throw new KeyNotFoundException($"Room with ID {roomId} not found");
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
                var bookingStartDate = GetBookingStartDate(booking);
                var bookingEndDate = GetBookingEndDate(booking);

                // Check for date overlap
                if (startDate < bookingEndDate && endDate > bookingStartDate)
                {
                    return false; // Room is not available due to overlap
                }
            }

            return true; // Room is available
        }

        public async Task UpdateRoomAsync(Guid id, RoomUpdateDTO roomUpdateDTO)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);

            if (room == null)
            {
                throw new KeyNotFoundException($"Room with ID {id} not found");
            }

            // Check if room number is being updated and already exists
            if (roomUpdateDTO.RoomNumber != null &&
                roomUpdateDTO.RoomNumber != room.RoomNumber)
            {
                var existingRoom = await _unitOfWork.Rooms.GetByRoomNumberAsync(roomUpdateDTO.RoomNumber);
                if (existingRoom != null && existingRoom.Id != id)
                {
                    throw new InvalidOperationException($"A room with number {roomUpdateDTO.RoomNumber} already exists");
                }
            }

            _mapper.Map(roomUpdateDTO, room);
            await _unitOfWork.Rooms.UpdateAsync(room);
            await _unitOfWork.CompleteAsync();
        }

        #region Helper Methods

        private DateTime GetBookingStartDate(Booking booking)
        {
            var month = DateTime.ParseExact(booking.ArrivalDateMonth, "MMMM", CultureInfo.InvariantCulture).Month;
            return new DateTime(booking.ArrivalDateYear, month, booking.ArrivalDateDayOfMonth);
        }

        private DateTime GetBookingEndDate(Booking booking)
        {
            var startDate = GetBookingStartDate(booking);
            return startDate.AddDays(booking.StaysInWeekendNights + booking.StaysInWeekNights);
        }

        #endregion
    }
}
