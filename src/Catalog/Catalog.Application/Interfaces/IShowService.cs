using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces
{
    public interface IShowService
    {
        Task<ShowDto?> GetShowByIdAsync(Guid id);
        Task<IEnumerable<ShowDto>> SearchShowsAsync(DateTime? time);
        Task<ShowDto> CreateShowAsync(ShowDto showDto);
        Task<bool> UpdateShowAsync(Guid id, ShowDto showDto);
        Task<bool> DeleteShowAsync(Guid id);
        Task SeedDataAsync();
    }
}
