using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Model_Binders
{
    public class GuidModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
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
                    bindingContext.Result = ModelBindingResult.Success(Guid.Empty); 
                    return Task.CompletedTask;
                }
            }
            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"The value '{value}' is not a valid GUID format."
            );

            return Task.CompletedTask;
        }
    }
}
