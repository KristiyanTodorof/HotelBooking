using HotelBooking.Domain.BaseModels;
using HotelBooking.Infrastructure.Repositories;
using HotelBooking.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Model_Binders
{
    public class EntityModelBinder<TEntity> : IModelBinder where TEntity : BaseEntity<Guid>
    {
        private readonly IRepository<TEntity,Guid> _repository;

        public EntityModelBinder(IRepository<TEntity, Guid> repository)
        {
            this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
           if (bindingContext == null)
           {
                throw new ArgumentNullException(nameof(bindingContext));
           }
           var modelName = bindingContext.ModelName;
           var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None) 
            {
                return;
            }
            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            if (!Guid.TryParse(value, out var id)) 
            {
                bindingContext.ModelState.TryAddModelError(modelName, $"The value '{value} was not in valid GUID format.'");
                return;
            }

            var entity = await _repository.GetByIdAsync(id);

            if (entity == null) 
            {
                bindingContext.ModelState.AddModelError(modelName, $"Entity with ID '{id} was not found.'");
                return;
            }

            bindingContext.Result = ModelBindingResult.Success(entity);
        }
    }
}
