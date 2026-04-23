using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Transaction?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Transactions.FirstOrDefaultAsync(t => t.BookingId == bookingId);
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
