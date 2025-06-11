using ResearchScrapper.Api.Models;
using StackExchange.Redis;

namespace ResearchScrapper.Api.Service
{
    public class RedisIpAbuseService : IIpAbuseService
    {
        private readonly IDatabase _redis;
        private const int MaxAttempts = 3;
        private static readonly TimeSpan BanDuration = TimeSpan.FromDays(14);

        public RedisIpAbuseService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task<bool> IsBlockedAsync(string ip)
        {
            var key = GetBlockedKey(ip);
            return await _redis.KeyExistsAsync(key);
        }

        public async Task RegisterInvalidAttemptAsync(string ip)
        {
            var invalidKey = GetInvalidKey(ip);
            var blockedKey = GetBlockedKey(ip);

            var count = await _redis.StringIncrementAsync(invalidKey);

            if (count == 1)
            {
                await _redis.KeyExpireAsync(invalidKey, BanDuration);
            }

            if (count >= MaxAttempts)
            {
                await _redis.StringSetAsync(blockedKey, true, BanDuration);
            }
        }

        private static string GetInvalidKey(string ip) => $"invalid:{ip}";
        private static string GetBlockedKey(string ip) => $"blocked:{ip}";
    }
}