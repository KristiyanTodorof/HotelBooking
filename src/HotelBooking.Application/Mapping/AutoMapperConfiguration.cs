using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Mapping
{
    public static class AutoMapperConfiguration
    {
        public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services) 
        {
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            return services;
        }
    }
}
