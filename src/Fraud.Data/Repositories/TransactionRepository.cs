using Fraud.Data.Context;
using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fraud.Data.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ILogger<TransactionRepository> _logger;
        private readonly FraudDbContext _context;

        public TransactionRepository(ILogger<TransactionRepository> logger, FraudDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task AddTransactionAsync(TransactionEntity transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<TransactionEntity?> GetTransactionByIdAsync(Guid transactionId)
        {
            return await _context.Transactions.FindAsync(transactionId);
        }

        public async Task<IEnumerable<TransactionEntity>> GetAllTransactionsAsync()
            => await _context.Transactions.ToListAsync();

    }
}
