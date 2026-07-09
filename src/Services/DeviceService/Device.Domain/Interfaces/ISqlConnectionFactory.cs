using System.Data;

namespace Device.Domain.Interfaces;

public interface ISqlConnectionFactory
{
    public IDbConnection CreateConnection();
}
