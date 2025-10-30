using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System;
using WeSign.Binders;

namespace WeSign.Providers
{
    public class GuidListModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(List<Guid>))
            {
                return new GuidListModelBinder();
            }

            return null;
        }
    }
}
