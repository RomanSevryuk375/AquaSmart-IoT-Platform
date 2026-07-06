using System.Data;

namespace Control.Domain.Interfaces;

public interface ISqlConnectionFactory
{
    public IDbConnection CreateConnection();
}
