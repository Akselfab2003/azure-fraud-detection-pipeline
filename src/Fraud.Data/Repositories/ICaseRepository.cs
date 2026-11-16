using Fraud.Data.Entities;

namespace Fraud.Data.Repositories
{
    public interface ICaseRepository
    {
        Task AddCaseAsync(CaseEntity caseEntity);
        Task<IEnumerable<CaseEntity>> GetAllCasesAsync();
        Task<CaseEntity?> GetCaseByIdAsync(Guid caseId);
    }
}