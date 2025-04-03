using AutoMapper;
using HotelBooking.Application.DTO.Booking;
using HotelBooking.Application.DTO.BookingDetails;
using HotelBooking.Application.DTO.Guest;
using HotelBooking.Application.DTO.Payment;
using HotelBooking.Application.DTO.Room;
using HotelBooking.Application.DTO.SalesChannel;
using HotelBooking.Application.DTO.User;
using HotelBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Mapping
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest.Name))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.ReservationStatusDate));

            CreateMap<BookingCreateDTO, Booking>()
                .ForMember(dest => dest.ReservationStatusDate, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
                .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.ReservationStatus, opt => opt.MapFrom(src => "Confirmed"));

            CreateMap<BookingUpdateDTO, Booking>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Guest mappings
            CreateMap<Guest, GuestDTO>();
            CreateMap<GuestCreateDTO, Guest>();
            CreateMap<GuestUpdateDTO, Guest>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Room mappings
            CreateMap<Room, RoomDTO>();
            CreateMap<RoomCreateDTO, Room>();
            CreateMap<RoomUpdateDTO, Room>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // BookingDetails mappings
            CreateMap<BookingDetails, BookingDetailsDTO>();
            CreateMap<BookingDetailsCreateDTO, BookingDetails>();
            CreateMap<BookingDetailsUpdateDTO, BookingDetails>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Payment mappings
            CreateMap<Payment, PaymentDTO>()
                .ForMember(dest => dest.CreditCard, opt => opt.MapFrom(src => src.MaskedCreditCardNumber));
            CreateMap<PaymentCreateDTO, Payment>()
                .ForMember(dest => dest.CreditCard, opt => opt.MapFrom(src => src.CreditCard));

            // SalesChannel mappings
            CreateMap<SalesChannel, SalesChannelDTO>();
            CreateMap<SalesChannelCreateDTO, SalesChannel>();
            CreateMap<SalesChannelUpdateDTO, SalesChannel>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // User mappings
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<UserCreateDTO, ApplicationUser>();
            CreateMap<UserUpdateDTO, ApplicationUser>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
