// Ignore Spelling: Validator cron

using Control.Domain.Interfaces;
using Cronos;

namespace Control.Infrastructure.Services;

public sealed class CronValidator : ICronValidator
{
    public bool IsValid(string cronExpression)
    {
        try
        {
            CronExpression.Parse(cronExpression, CronFormat.Standard);
            return true;
        }
        catch (CronFormatException)
        {
            return false;
        }
    }
}
