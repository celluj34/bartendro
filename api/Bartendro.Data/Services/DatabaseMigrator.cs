using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bartendro.Data.Services
{
    public interface IDatabaseMigrator
    {
        Task MigrateAsync();
    }

    internal class DatabaseMigrator : IDatabaseMigrator
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly ILogger<DatabaseMigrator> _logger;

        public DatabaseMigrator(IDatabaseContext databaseContext, ILogger<DatabaseMigrator> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async Task MigrateAsync()
        {
            try
            {
                await _databaseContext.MigrateAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred migrating the database.");

                throw;
            }
        }
    }
}