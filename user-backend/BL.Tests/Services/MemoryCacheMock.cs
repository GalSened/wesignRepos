using Common.Models;
using Common.Models.Configurations;
using Common.Models.Programs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Tests.Services
{
    public sealed class MemoryCacheMock : IMemoryCache
    {

        public ICacheEntry CreateEntry(object key)
        {
            return new NullCacheEntry() { Key = key };
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {

        }

        public bool TryGetValue(object key, out object value)
        {
         

            string sampleGuid = "00000000-0000-0000-0000-000000000001";
            string differentSampleId = "00000000-0000-0000-0000-000000000002";
            string userInMemoryGuid = "00000000-0000-0000-0000-000000000003";
            string userInMemorySigner1NotInMemoryGuid = "00000000-0000-0000-0000-000000000004";
            string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
            string  GROUP_GUID = "0472EE37-3C50-47CE-8ECE-B5455A5CC468";

            User user = new User() { Id = Guid.Parse(userInMemoryGuid), CompanyId = Guid.Parse(sampleGuid)};
            CompanySigner1Details companySigner1Details = new CompanySigner1Details() { CertId = "Changed" };
            if (key.ToString() == $"{GUID}_{GROUP_GUID}" || key.ToString() == $"{sampleGuid}_{ GROUP_GUID}")
                value = null;
            else if (key.ToString() == $"{userInMemoryGuid}_{GROUP_GUID}" )
            {
                user.CompanyId = Guid.Parse(differentSampleId);
                value = user;
            }
            else if (key.ToString() == $"{userInMemorySigner1NotInMemoryGuid}_{GROUP_GUID}"  )
                value = user;
            else
                value = companySigner1Details;

            return false;
        }

        private sealed class NullCacheEntry : ICacheEntry
        {
            public DateTimeOffset? AbsoluteExpiration { get; set; }
            public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

            public IList<IChangeToken> ExpirationTokens { get; set; }

            public object Key { get; set; }

            public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }

            public CacheItemPriority Priority { get; set; }
            public long? Size { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }
            public object Value { get; set; }

            public void Dispose()
            {

            }
        }
    }
}
