using Fraud.Contracts;

namespace Fraud.Scoring.Infrastructure
{
    public interface ITransactionStore
    {
        Task<List<TransactionCreated>> ReadTransactionsFromCache(Guid CustomerId, CancellationToken ct = default);
        Task WriteTransactionToCache(TransactionCreated tx, CancellationToken ct = default);
    }
}