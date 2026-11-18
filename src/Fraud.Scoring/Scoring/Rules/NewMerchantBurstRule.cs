using Fraud.Contracts;
using Fraud.Data.Repositories;
using Fraud.Scoring.Infrastructure;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring.Rules
{
    /// <summary>
    /// Fires when, within the burst window, the customer buys at <see cref="ScoringOptions.NewMerchantBurstThreshold"/>
    /// or more different merchants they had never used before the window. The current transaction counts.
    /// </summary>
    public class NewMerchantBurstRule(IOptions<ScoringOptions> options, ITransactionStore transactionStore, ITransactionRepository transactionRepository) : IRule
    {
        private const string REASONCODE = "NEW_MERCHANT_BURST";
        private const string DESCRIPTION = "A run of purchases at several merchants the customer has never used, in a short window.";

        private ScoringOptions Options => options.Value;

        public async Task<RuleHit?> Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now)
        {
            // With little history every merchant looks new, so this rule needs an established customer
            // (same idea as MinHistory on the amount rule). This also skips the database query for new customers.
            if (state.TransactionCount < Options.MinHistory)
                return null;

            var windowStart = now.AddSeconds(-Options.NewMerchantBurstWindowSeconds);
            var transactions = await transactionStore.ReadTransactionsFromCache(transaction.CustomerId);
            if (transactions is null || transactions.Count == 0)
            {
                transactions = await transactionRepository.GetTransactionsByCustomer(transaction.CustomerId, from: null, to: windowStart).ContinueWith(t =>
                {
                    return t.Result.Select(s =>
                    {
                        return new TransactionCreated(s.Id, s.CustomerId, s.MerchantId, s.MerchantCategory, s.Amount, s.Currency, s.Country, s.Channel, s.OccurredAt);
                    }).ToList();
                });
            }

            var inWindow = transactions.Where(t => t.OccurredAt >= windowStart && t.OccurredAt <= now).OfType<TransactionCreated>().ToList();

            var before = await transactionRepository.GetTransactionsByCustomer(transaction.CustomerId, null, windowStart.AddTicks(-1));

            var knownMerchants = before.Select(t => t.MerchantId).ToHashSet();

            var newMerchantsInWindow = inWindow.Select(t => t.MerchantId)
                                               .Append(transaction.MerchantId)   // the current transaction is not stored yet
                                               .Where(id => !knownMerchants.Contains(id))
                                               .Distinct()
                                               .Count();

            return newMerchantsInWindow >= Options.NewMerchantBurstThreshold
                ? new RuleHit(REASONCODE, Options.NewMerchantBurstScore, DESCRIPTION)
                : null;
        }
    }
}
