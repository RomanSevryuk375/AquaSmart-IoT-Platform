using System.Data;

namespace IdentityService.Domain.Interfaces;

public interface ISqlConnectionFactory
{
    public IDbConnection CreateConnection();
}
