using Microsoft.AspNetCore.Mvc;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowsController : ControllerBase
    {
        private readonly IShowService _showService;

        public ShowsController(IShowService showService)
        {
            _showService = showService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> Search(DateTime? time)
        {
            var results = await _showService.SearchShowsAsync(time);
            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShowDto>> GetById(Guid id)
        {
            var show = await _showService.GetShowByIdAsync(id);
            if (show == null) return NotFound();
            return Ok(show);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShowDto showDto)
        {
            var createdShow = await _showService.CreateShowAsync(showDto);
            return CreatedAtAction(nameof(GetById), new { id = createdShow.Id }, createdShow);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, ShowDto showDto)
        {
            var updated = await _showService.UpdateShowAsync(id, showDto);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _showService.DeleteShowAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
