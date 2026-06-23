using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Device.Infrastructure.Factories;

internal sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        string? connectionString = configuration.GetConnectionString("SystemDbContext");
        return new NpgsqlConnection(connectionString);
    }
}
