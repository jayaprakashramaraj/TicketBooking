using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Services
{
    public class ShowService : IShowService
    {
        private readonly IShowRepository _showRepository;

        public ShowService(IShowRepository showRepository)
        {
            _showRepository = showRepository;
        }

        public async Task<ShowDto?> GetShowByIdAsync(Guid id)
        {
            var show = await _showRepository.GetByIdAsync(id);
            if (show == null) return null;

            return new ShowDto
            {
                Id = show.Id,
                MovieName = show.MovieName,
                TheaterName = show.TheaterName,
                StartTime = show.StartTime,
                Price = show.Price
            };
        }

        public async Task<IEnumerable<ShowDto>> SearchShowsAsync(DateTime? time)
        {
            DateTime? startTime = time;
            DateTime? endTime = time?.AddHours(4);

            var shows = await _showRepository.SearchAsync(startTime, endTime);
            return shows.Select(s => new ShowDto
            {
                Id = s.Id,
                MovieName = s.MovieName,
                TheaterName = s.TheaterName,
                StartTime = s.StartTime,
                Price = s.Price
            });
        }

        public async Task<ShowDto> CreateShowAsync(ShowDto showDto)
        {
            var show = new Show
            {
                Id = showDto.Id == Guid.Empty ? Guid.NewGuid() : showDto.Id,
                MovieName = showDto.MovieName,
                TheaterName = showDto.TheaterName,
                StartTime = showDto.StartTime,
                Price = showDto.Price
            };

            await _showRepository.AddAsync(show);
            showDto.Id = show.Id;
            return showDto;
        }

        public async Task<bool> UpdateShowAsync(Guid id, ShowDto showDto)
        {
            var existingShow = await _showRepository.GetByIdAsync(id);
            if (existingShow == null) return false;

            existingShow.MovieName = showDto.MovieName;
            existingShow.TheaterName = showDto.TheaterName;
            existingShow.StartTime = showDto.StartTime;
            existingShow.Price = showDto.Price;

            await _showRepository.UpdateAsync(existingShow);
            return true;
        }

        public async Task<bool> DeleteShowAsync(Guid id)
        {
            var existingShow = await _showRepository.GetByIdAsync(id);
            if (existingShow == null) return false;

            await _showRepository.DeleteAsync(id);
            return true;
        }

        public async Task SeedDataAsync()
        {
            if (!await _showRepository.AnyAsync())
            {
                var shows = new List<Show>
                {
                    new Show { Id = Guid.NewGuid(), MovieName = "Inception", TheaterName = "Grand Cinema", StartTime = DateTime.Today.AddHours(18), Price = 15.0m },
                    new Show { Id = Guid.NewGuid(), MovieName = "The Dark Knight", TheaterName = "Grand Cinema", StartTime = DateTime.Today.AddHours(21), Price = 18.0m },
                    new Show { Id = Guid.NewGuid(), MovieName = "Interstellar", TheaterName = "Star Plex", StartTime = DateTime.Today.AddHours(19), Price = 16.0m },
                    new Show { Id = Guid.NewGuid(), MovieName = "Tenet", TheaterName = "IMAX Center", StartTime = DateTime.Today.AddHours(20), Price = 20.0m }
                };
                await _showRepository.AddManyAsync(shows);
            }
        }
    }
}
