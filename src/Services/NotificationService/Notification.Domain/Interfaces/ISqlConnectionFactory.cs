using System.Data;

namespace Notification.Domain.Interfaces;

public interface ISqlConnectionFactory
{
    public IDbConnection CreateConnection();
}
