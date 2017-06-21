using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Thanks to https://dzimchuk.net/adal-distributed-token-cache-in-asp-net-core.
/// </summary>
namespace ArticleWebApp.Utils
{
    public class DistributedTokenCache : TokenCache
    {
        private readonly IDistributedCache cache;
        private readonly string userId;

        public DistributedTokenCache(IDistributedCache cache, string userId)
        {
            this.cache = cache;
            this.userId = userId;

            BeforeAccess = OnBeforeAccess;
            AfterAccess = OnAfterAccess;
        }

        private void OnBeforeAccess(TokenCacheNotificationArgs args)
        {
            var userTokenCachePayload = cache.Get(CacheKey);

            if (userTokenCachePayload != null)
            {
                Deserialize(userTokenCachePayload);
            }
        }

        private void OnAfterAccess(TokenCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                if (this.Count > 0)
                {

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(14)
                    };

                    cache.Set(CacheKey, Serialize(), cacheOptions);
                }
                else
                {
                    cache.Remove(CacheKey);
                }

                HasStateChanged = false;
            }
        }

        private string CacheKey => $"TokenCache_{userId}";
    }
}
