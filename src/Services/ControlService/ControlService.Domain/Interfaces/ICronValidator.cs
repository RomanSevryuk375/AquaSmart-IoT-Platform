// Ignore Spelling: cron Validator

namespace Control.Domain.Interfaces;

public interface ICronValidator
{
    public bool IsValid(string cronExpression);
}
