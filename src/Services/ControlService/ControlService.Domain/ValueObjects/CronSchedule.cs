// Ignore Spelling: Cron Validator

using Contracts.Constants;
using Contracts.Results;
using Control.Domain.Interfaces;

namespace Control.Domain.ValueObjects;

public sealed record CronSchedule
{
    public string Value { get; }

    private CronSchedule(string value)
    {
        Value = value;
    }

    public static Result<CronSchedule> Create(string value, ICronValidator cronValidator)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<CronSchedule>.Failure(Error.Validation<CronSchedule>(
                ControlValidationMessages.CronExpressionCannotBeEmpty));
        }

        if (!cronValidator.IsValid(value))
        {
            return Result<CronSchedule>.Failure(Error.Validation<CronSchedule>(
                string.Format(ControlValidationMessages.InvalidCronExpressionFormat, value)));
        }

        return Result<CronSchedule>.Success(new CronSchedule(value));
    }

    public static CronSchedule Load(string value) => new CronSchedule(value);
    public override string ToString() => Value;
}
