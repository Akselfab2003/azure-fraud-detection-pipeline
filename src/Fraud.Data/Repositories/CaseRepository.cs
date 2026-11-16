using Fraud.Data.Context;
using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fraud.Data.Repositories
{
    public class CaseRepository : ICaseRepository
    {
        private readonly ILogger<CaseRepository> _logger;
        private readonly FraudDbContext _context;

        public CaseRepository(ILogger<CaseRepository> logger, FraudDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        public async Task AddCaseAsync(CaseEntity caseEntity)
        {
            await _context.Cases.AddAsync(caseEntity);
            await _context.SaveChangesAsync();
        }


        public async Task<CaseEntity?> GetCaseByIdAsync(Guid caseId)
        {
            return await _context.Cases.FindAsync(caseId);
        }

        public async Task<IEnumerable<CaseEntity>> GetAllCasesAsync()
            => await _context.Cases.ToListAsync();
    }
}
