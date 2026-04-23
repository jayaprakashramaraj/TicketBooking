using System;
using System.Threading.Tasks;
using Payment.Domain.Entities;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly PaymentDbContext _context;

        public TransactionRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
