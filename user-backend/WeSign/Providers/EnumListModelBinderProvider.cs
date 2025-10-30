using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System;
using WeSign.Binders;

namespace WeSign.Providers
{
    public class EnumListModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType.IsGenericType &&
                context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = context.Metadata.ModelType.GetGenericArguments()[0];

                if (elementType.IsEnum)
                {
                    var binderType = typeof(EnumListModelBinder<>).MakeGenericType(elementType);
                    return (IModelBinder)Activator.CreateInstance(binderType);
                }
            }

            return null;
        }
    }
}
