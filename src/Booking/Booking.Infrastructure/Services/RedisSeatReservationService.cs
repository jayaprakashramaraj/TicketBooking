using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Booking.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.Services
{
    public class RedisSeatReservationService : ISeatReservationService
    {
        private readonly IConnectionMultiplexer? _redis;
        private readonly ILogger<RedisSeatReservationService> _logger;

        public RedisSeatReservationService(IConnectionMultiplexer? redis, ILogger<RedisSeatReservationService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<bool> ReserveSeatsAsync(Guid showId, List<string> seatNumbers, Guid bookingId, TimeSpan expiry)
        {
            if (_redis == null || !_redis.IsConnected)
            {
                _logger.LogWarning("Redis is not connected. Skipping distributed lock for seats.");
                return true; // Fallback: Allow booking to proceed to DB phase
            }

            try
            {
                var db = _redis.GetDatabase();
                var keys = seatNumbers.Select(s => (RedisKey)$"show:{showId}:seat:{s}").ToArray();

                var luaScript = @"
                    local reserved = {}
                    for i=1,#KEYS do
                        if redis.call('SET', KEYS[i], ARGV[1], 'NX', 'EX', ARGV[2]) then
                            table.insert(reserved, KEYS[i])
                        else
                            -- Rollback
                            for _, key in ipairs(reserved) do
                                redis.call('DEL', key)
                            end
                            return 0
                        end
                    end
                    return 1";

                var result = await db.ScriptEvaluateAsync(luaScript, keys, new RedisValue[] { bookingId.ToString(), (long)expiry.TotalSeconds });
                return (int)result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while communicating with Redis for seat reservation.");
                return true; // Fallback: Allow booking to proceed
            }
        }

        public async Task ReleaseSeatsAsync(Guid showId, List<string> seatNumbers)
        {
            if (_redis == null || !_redis.IsConnected) return;

            try
            {
                var db = _redis.GetDatabase();
                var keys = seatNumbers.Select(s => (RedisKey)$"show:{showId}:seat:{s}").ToArray();
                await db.KeyDeleteAsync(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while releasing seats in Redis.");
            }
        }
    }
}
