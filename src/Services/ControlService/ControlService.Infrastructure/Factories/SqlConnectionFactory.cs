using System.Data;
using Control.Domain.Interfaces;
using Control.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Control.Infrastructure.Factories;

internal sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        string? connectionString = configuration.GetConnectionString(nameof(ControlDbContext));
        return new NpgsqlConnection(connectionString);
    }
}
