using Fraud.Contracts;
using Fraud.Data.Repositories;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring.Rules
{
    public class NewMerchantBurstRule(IOptions<ScoringOptions> options, ITransactionRepository transactionRepository) : IRule
    {
        private ScoringOptions Options => options.Value;
        private const string RULECODE = "NewMerchantBurst";
        private const string DESCRIPTION = "A run of purchases at several merchants the customer has never used, in a short window.";

        public async Task<RuleHit?> Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now)
        {
            if (state.RecentTransactionTimes.Count < Options.NewMerchantBurstThreshold) // too few transactions to tell if this is a burst of new merchants
                return null;

            var windowStart = now.AddSeconds(-Options.NewMerchantBurstWindowSeconds);
            var recentTransactionWithInWindow = await transactionRepository.GetTransactionsByCustomer(transaction.CustomerId, windowStart, now);

            var numberOfNewMerchants = recentTransactionWithInWindow.Select(t => t.MerchantId)
                                                                    .Distinct()
                                                                    .Count();

            if (numberOfNewMerchants >= Options.NewMerchantBurstThreshold)
            {
                return new RuleHit(RULECODE, Options.NewMerchantBurstScore, DESCRIPTION);
            }

            return null;
        }
    }
}
