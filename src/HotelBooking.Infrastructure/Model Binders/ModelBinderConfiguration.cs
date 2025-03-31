using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Model_Binders
{
    public static class ModelBinderConfiguration
    {
        public static IMvcBuilder AddCustomModelBinders(this IMvcBuilder builder)
        {
            // Add custom model binder providers in the correct order
            // Order matters - providers are checked in the order they are added
            builder.AddMvcOptions(options =>
            {
                // Add the GUID model binder provider
                options.ModelBinderProviders.Insert(0, new GuidModelBinderProvider());

                // Add the DateTime model binder provider
                options.ModelBinderProviders.Insert(1, new DateTimeModelBinderProvider());

                // Add the entity model binder provider
                options.ModelBinderProviders.Insert(2, new EntityModelBinderProvider());
            });

            return builder;
        }
    }
}
