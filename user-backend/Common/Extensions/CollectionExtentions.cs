using System;
using System.Collections.Generic;
using System.Linq;
using Twilio.Rest.Api.V2010.Account.Usage.Record;

namespace Common.Extensions
{
    public static class CollectionExtentions
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration ?? Enumerable.Empty<T>())
            {
                action(item);
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

       

    }
}
