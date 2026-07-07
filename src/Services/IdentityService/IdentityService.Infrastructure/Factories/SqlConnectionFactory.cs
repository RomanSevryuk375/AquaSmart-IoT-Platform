using System.Data;
using IdentityService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IdentityService.Infrastructure.Factories;

internal sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        string? connectionString = configuration.GetConnectionString(nameof(IdentityDbContext));
        return new NpgsqlConnection(connectionString);
    }
}
