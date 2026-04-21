using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.API.Services
{
    public class RedisSeatReservationService : ISeatReservationService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisSeatReservationService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task<bool> ReserveSeatsAsync(Guid showId, List<string> seatNumbers, Guid bookingId, TimeSpan expiry)
        {
            // Use a Lua script to ensure atomic reservation of ALL requested seats.
            // If even one seat is taken, the whole reservation fails.
            var keys = seatNumbers.Select(s => (RedisKey)$"show:{showId}:seat:{s}").ToArray();

            // Optimization: Since StackExchange.Redis doesn't have a direct 'SET multiple NX' 
            // We use a small Lua script.
            
            var luaScript = @"
                local reserved = {}
                for i=1,#KEYS do
                    if redis.call('SET', KEYS[i], ARGV[1], 'NX', 'EX', ARGV[2]) then
                        table.insert(reserved, KEYS[i])
                    else
                        -- Rollback: if any seat is taken, release the ones we just locked
                        for _, key in ipairs(reserved) do
                            redis.call('DEL', key)
                        end
                        return 0
                    end
                end
                return 1";

            var result = await _db.ScriptEvaluateAsync(luaScript, keys, new RedisValue[] { bookingId.ToString(), (long)expiry.TotalSeconds });

            return (int)result == 1;
        }

        public async Task ReleaseSeatsAsync(Guid showId, List<string> seatNumbers)
        {
            var keys = seatNumbers.Select(s => (RedisKey)$"show:{showId}:seat:{s}").ToArray();
            await _db.KeyDeleteAsync(keys);
        }
    }
}
