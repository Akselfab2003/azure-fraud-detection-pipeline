using Fraud.Data.Entities;

namespace Fraud.Data.Repositories
{
    public interface ITransactionRepository
    {
        Task<TransactionEntity> AddTransactionAsync(TransactionEntity transaction);
        Task<IEnumerable<TransactionEntity>> GetAllTransactionsAsync();
        Task<TransactionEntity?> GetTransactionByIdAsync(Guid transactionId);
        Task<IEnumerable<TransactionEntity>> GetTransactionsByCustomer(Guid customerId, DateTimeOffset? from, DateTimeOffset? to);
    }
}