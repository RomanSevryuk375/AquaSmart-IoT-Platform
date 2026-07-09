using System.Data;

namespace Telemetry.Domain.Interfaces;

public interface ISqlConnectionFactory
{
    public IDbConnection CreateConnection();
}
