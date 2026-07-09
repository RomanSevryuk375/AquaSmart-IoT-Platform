using System.Data;
using Microsoft.Extensions.Configuration;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.Persistence;
using Npgsql;

namespace Notification.Infrastructure.Factories;

internal sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        string? connectionString = configuration.GetConnectionString(nameof(NotificationDbContext));
        return new NpgsqlConnection(connectionString);
    }
}
