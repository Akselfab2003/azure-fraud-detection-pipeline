using Fraud.Data.Context;
using Microsoft.Extensions.Logging;

namespace Fraud.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ILogger<UnitOfWork> _logger;
        private readonly FraudDbContext _context;

        public UnitOfWork(ILogger<UnitOfWork> logger, FraudDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// This method saves all changes made in the context to the database. It wraps the SaveChangesAsync call in a try-catch block to log any exceptions that occur during the save operation.
        /// If an exception is caught, it logs the error and rethrows the exception to be handled by the calling code.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving changes to the database.");
                throw;
            }
        }
    }
}
