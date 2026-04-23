using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Domain.Entities;

namespace Catalog.Domain.Repositories
{
    public interface IShowRepository
    {
        Task<Show?> GetByIdAsync(Guid id);
        Task<IEnumerable<Show>> SearchAsync(DateTime? startTime, DateTime? endTime);
        Task AddAsync(Show show);
        Task UpdateAsync(Show show);
        Task DeleteAsync(Guid id);
        Task<bool> AnyAsync();
        Task AddManyAsync(IEnumerable<Show> shows);
    }
}
