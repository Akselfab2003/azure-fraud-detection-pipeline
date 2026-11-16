using Fraud.Data.Entities;

namespace Fraud.Data.Repositories
{
    public interface IGroundTruthRepository
    {
        Task AddGroundTruthAsync(Guid transactionId, bool isFraud, string pattern);
        Task<List<GroundTruthEntity>> GetAllGroundTruthAsync();
        Task<GroundTruthEntity?> GetGroundTruthAsync(Guid transactionId);
    }
}