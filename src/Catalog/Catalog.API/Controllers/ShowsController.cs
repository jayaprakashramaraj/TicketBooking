using Microsoft.AspNetCore.Mvc;
using Catalog.API.Domain;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowsController : ControllerBase
    {
        private readonly IMongoCollection<Show> _showsCollection;

        public ShowsController(IMongoDatabase database)
        {
            _showsCollection = database.GetCollection<Show>("Shows");
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Show>>> Search(DateTime? time)
        {
            var filter = time.HasValue 
                ? Builders<Show>.Filter.And(
                    Builders<Show>.Filter.Gte(s => s.StartTime, time.Value),
                    Builders<Show>.Filter.Lte(s => s.StartTime, time.Value.AddHours(4))
                  )
                : Builders<Show>.Filter.Empty;

            var results = await _showsCollection.Find(filter).ToListAsync();
            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Show>> GetById(Guid id)
        {
            var show = await _showsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (show == null) return NotFound();
            return Ok(show);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Show show)
        {
            if (show.Id == Guid.Empty) show.Id = Guid.NewGuid();
            await _showsCollection.InsertOneAsync(show);
            return CreatedAtAction(nameof(GetById), new { id = show.Id }, show);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Show show)
        {
            var result = await _showsCollection.ReplaceOneAsync(s => s.Id == id, show);
            if (result.MatchedCount == 0) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _showsCollection.DeleteOneAsync(s => s.Id == id);
            if (result.DeletedCount == 0) return NotFound();
            return NoContent();
        }
    }
}
