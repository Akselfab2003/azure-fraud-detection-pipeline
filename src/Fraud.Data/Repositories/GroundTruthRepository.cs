using Fraud.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fraud.Data.Repositories
{
    public class GroundTruthRepository : IGroundTruthRepository
    {
        private readonly ILogger<GroundTruthRepository> _logger;

        private readonly FraudDbContext _dbContext;

        public GroundTruthRepository(ILogger<GroundTruthRepository> logger, FraudDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task AddGroundTruthAsync(Guid transactionId, bool isFraud, string pattern)
        {
            var groundTruthEntity = new Entities.GroundTruthEntity
            {
                TransactionId = transactionId,
                IsFraud = isFraud,
                Pattern = pattern
            };
            await _dbContext.GroundTruth.AddAsync(groundTruthEntity);
        }

        public async Task<Entities.GroundTruthEntity?> GetGroundTruthAsync(Guid transactionId)
        {
            return await _dbContext.GroundTruth.FindAsync(transactionId);
        }

        public async Task<List<Entities.GroundTruthEntity>> GetAllGroundTruthAsync()
            => await _dbContext.GroundTruth.ToListAsync();
    }
}
