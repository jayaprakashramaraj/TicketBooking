using System;
using System.Threading.Tasks;
using Notification.Domain.Repositories;
using StackExchange.Redis;

namespace Notification.Infrastructure.Persistence
{
    public class RedisTicketRepository : ITicketRepository
    {
        private readonly IDatabase _database;
        private const string TicketPrefix = "ticket:";

        public RedisTicketRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task SaveTicketAsync(Guid bookingId, byte[] pdfContent)
        {
            await _database.StringSetAsync($"{TicketPrefix}{bookingId}", pdfContent, TimeSpan.FromHours(24));
        }

        public async Task<byte[]?> GetTicketAsync(Guid bookingId)
        {
            var data = await _database.StringGetAsync($"{TicketPrefix}{bookingId}");
            return data.IsNull ? null : (byte[])data!;
        }
    }
}
