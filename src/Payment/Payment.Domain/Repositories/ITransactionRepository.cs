using System;
using System.Threading.Tasks;
using Payment.Domain.Entities;

namespace Payment.Domain.Repositories
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
        Task SaveChangesAsync();
    }
}
