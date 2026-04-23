using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Repositories
{
    public class ShowRepository : IShowRepository
    {
        private readonly IMongoCollection<Show> _shows;

        public ShowRepository(IMongoDatabase database)
        {
            _shows = database.GetCollection<Show>("Shows");
        }

        public async Task<Show?> GetByIdAsync(Guid id)
        {
            return await _shows.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Show>> SearchAsync(DateTime? startTime, DateTime? endTime)
        {
            var filter = Builders<Show>.Filter.Empty;
            if (startTime.HasValue && endTime.HasValue)
            {
                filter = Builders<Show>.Filter.And(
                    Builders<Show>.Filter.Gte(s => s.StartTime, startTime.Value),
                    Builders<Show>.Filter.Lte(s => s.StartTime, endTime.Value)
                );
            }
            return await _shows.Find(filter).ToListAsync();
        }

        public async Task AddAsync(Show show)
        {
            await _shows.InsertOneAsync(show);
        }

        public async Task UpdateAsync(Show show)
        {
            await _shows.ReplaceOneAsync(s => s.Id == show.Id, show);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _shows.DeleteOneAsync(s => s.Id == id);
        }

        public async Task<bool> AnyAsync()
        {
            return await _shows.Find(Builders<Show>.Filter.Empty).AnyAsync();
        }

        public async Task AddManyAsync(IEnumerable<Show> shows)
        {
            await _shows.InsertManyAsync(shows);
        }
    }
}
