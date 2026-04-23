using System;
using System.Threading.Tasks;
using Identity.Domain.Entities;

namespace Identity.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task AddAsync(User user);
        Task<bool> ExistsAsync(string email);
        Task SaveChangesAsync();
    }
}
