using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.Persistence;

namespace Telemetry.Infrastructure.Factories;

internal sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        string? connectionString = configuration.GetConnectionString(nameof(TelemetryDbContext));
        return new NpgsqlConnection(connectionString);
    }
}
