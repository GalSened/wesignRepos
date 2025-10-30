using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Binders
{
    public class GuidListModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var guids = value.Split(',')
                             .Select(g => Guid.TryParse(g, out var guid) ? guid : (Guid?)null)
                             .Where(g => g.HasValue)
                             .Select(g => g.Value)
                             .ToList();

            bindingContext.Result = ModelBindingResult.Success(guids);
            return Task.CompletedTask;
        }
    }
}
