using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Model_Binders
{
    public class DateTimeModelBinder : IModelBinder
    {
        private readonly string[] _dateFormats = new[] 
        {
            "yyyy-MM-dd",             // ISO 8601 (2023-10-15)
            "MM/dd/yyyy",             // US format (10/15/2023)
            "dd/MM/yyyy",             // European format (15/10/2023)
            "dd-MMM-yyyy",            // 15-Oct-2023
            "yyyy-MM-ddTHH:mm:ss",    // ISO 8601 with time (2023-10-15T14:30:00)
            "yyyy-MM-ddTHH:mm:ssZ"    // ISO 8601 with time and timezone (2023-10-15T14:30:00Z)
        };

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if(bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None) 
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value)) 
            {
                if (!bindingContext.ModelMetadata.IsRequired) 
                {
                    
                    if (bindingContext.ModelType == typeof(DateTime?)) 
                    {
                        bindingContext.Result = ModelBindingResult.Success(null);
                    }
                    else
                    {
                        bindingContext.Result = ModelBindingResult.Success(DateTimeOffset.MinValue);
                    }
                }
                return Task.CompletedTask;
            }
            if (DateTime.TryParseExact(
                value,
                _dateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateTime))
            {
                bindingContext.Result = ModelBindingResult.Success(dateTime);
                return Task.CompletedTask;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                bindingContext.Result = ModelBindingResult.Success(dateTime);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"The value '{value}' is not a valid date format."
            );

            return Task.CompletedTask;

        }
    }
}
