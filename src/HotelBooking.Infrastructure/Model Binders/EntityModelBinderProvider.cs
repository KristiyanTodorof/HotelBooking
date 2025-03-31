using HotelBooking.Domain.BaseModels;
using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Model_Binders
{
    public class EntityModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (IsEntityType(context.Metadata.ModelType, out var entityType))
            {
                var modelBinderType = typeof(EntityModelBinder<>).MakeGenericType(entityType);

                var repository = ResolveRepository(context.Services, entityType);

                return (IModelBinder)Activator.CreateInstance(modelBinderType, repository);
            }

            return null;
        }
        private bool IsEntityType(Type modelType, out Type entityType)
        {
            entityType = null;

            if (modelType.IsClass && !modelType.IsAbstract)
            {
                var baseEntityType = typeof(BaseEntity<Guid>);
                if (baseEntityType.IsAssignableFrom(modelType))
                {
                    entityType = modelType;
                    return true;
                }
            }

            return false;
        }
        private object ResolveRepository(IServiceProvider services, Type entityType)
        {
            if (entityType == typeof(Booking))
            {
                return services.GetRequiredService<IBookingRepository>();
            }
            else if (entityType == typeof(BookingDetails))
            {
                return services.GetRequiredService<IBookingDetailsRepository>();
            }
            else if (entityType == typeof(Guest))
            {
                return services.GetRequiredService<IGuestRepository>();
            }
            else if (entityType == typeof(Payment))
            {
                return services.GetRequiredService<IPaymentRepository>();
            }
            else if (entityType == typeof(Room))
            {
                return services.GetRequiredService<IRoomRepository>();
            }
            else if (entityType == typeof(SalesChannel))
            {
                return services.GetRequiredService<ISalesChannelRepository>();
            }
            else if (entityType == typeof(ApplicationUser))
            {
                return null;
            }
            else if (entityType == typeof(ApplicationRole))
            {
                return null;
            }

            var repositoryType = typeof(IRepository<,>).MakeGenericType(entityType, typeof(Guid));
            return services.GetService(repositoryType);
        }
    }
}
