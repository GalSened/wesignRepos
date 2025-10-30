using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HistoryIntegratorService.Binders
{
    public class EnumListModelBinder<T> : IModelBinder where T : struct, Enum
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                // Return null if the parameter is not provided in the query string
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                // Return null if the parameter is provided but empty
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            try
            {
                // Parse the comma-separated string into a list of enums
                var values = value.Split(',')
                                  .Select(x => Enum.Parse<T>(x, true))
                                  .ToList();

                bindingContext.Result = ModelBindingResult.Success(values);
            }
            catch (Exception)
            {
                bindingContext.ModelState.TryAddModelError(modelName, $"Invalid {modelName} value.");
            }

            return Task.CompletedTask;
        }
    }
}
