using System.Numerics;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.Domain.Common;

public static class DomainGuard
{
    public static Result RequireNotEmpty<TEntity>(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure(Error.Validation<TEntity>($"{fieldName} cannot be empty."));
        }

        return Result.Success();
    }

    public static Result<string> RequireText<TEntity>(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<string>.Failure(Error.Validation<TEntity>($"{fieldName} is required."));
        }

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            return Result<string>.Failure(Error.Validation<TEntity>(
                $"{fieldName} cannot exceed {maxLength} characters."));
        }

        return Result<string>.Success(trimmed);
    }

    public static Result<string?> OptionalText<TEntity>(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<string?>.Success(null);
        }

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            return Result<string?>.Failure(Error.Validation<TEntity>(
                $"{fieldName} cannot exceed {maxLength} characters."));
        }

        return Result<string?>.Success(trimmed);
    }

    public static Result RequireEnum<TEntity, TEnum>(TEnum value, string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            return Result.Failure(Error.Validation<TEntity>($"Invalid {fieldName}."));
        }

        return Result.Success();
    }

    public static void CheckNotEmpty(Guid value, string fieldName, List<string> errors)
    {
        if (value == Guid.Empty)
        {
            errors.Add($"{fieldName} cannot be empty.");
        }
    }

    public static string CheckText(string? value, int maxLength, string fieldName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} is required.");
            return string.Empty;
        }

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            errors.Add($"{fieldName} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static string? CheckOptionalText(string? value, int maxLength, string fieldName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            errors.Add($"{fieldName} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static void CheckEnum<TEnum>(TEnum value, string fieldName, List<string> errors)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            errors.Add($"Invalid {fieldName}.");
        }
    }

    public static T? Collect<T>(Result<T> result, List<string> errors)
    {
        if (result.IsFailure)
        {
            errors.Add(result.Error.Message);
            return default;
        }

        return result.Value;
    }

    public static Result<T> ValidationFailure<T>(IReadOnlyCollection<string> errors) =>
        Result<T>.Failure(Error.Validation<T>(string.Join(", ", errors)));

    public static Result RequireNonNegative<TEntity>(decimal value, string fieldName) =>
        RequireNonNegativeCore<TEntity, decimal>(value, fieldName);

    public static Result RequireNonNegative<TEntity>(int value, string fieldName) =>
        RequireNonNegativeCore<TEntity, int>(value, fieldName);

    public static Result RequirePositive<TEntity>(decimal value, string fieldName) =>
        RequirePositiveCore<TEntity, decimal>(value, fieldName);

    public static Result RequirePositive<TEntity>(int value, string fieldName) =>
        RequirePositiveCore<TEntity, int>(value, fieldName);

    public static Result RequirePositive<TEntity>(long value, string fieldName) =>
        RequirePositiveCore<TEntity, long>(value, fieldName);

    public static Result RequireInRange<TEntity>(decimal value, decimal min, decimal max, string fieldName) =>
        RequireInRangeCore<TEntity, decimal>(value, min, max, fieldName);

    public static Result RequireInRange<TEntity>(int value, int min, int max, string fieldName) =>
        RequireInRangeCore<TEntity, int>(value, min, max, fieldName);

    public static void CheckNonNegative<TNumber>(TNumber value, string fieldName, List<string> errors)
        where TNumber : INumber<TNumber>
    {
        if (value < TNumber.Zero)
        {
            errors.Add($"{fieldName} cannot be negative.");
        }
    }

    public static void CheckPositive<TNumber>(TNumber value, string fieldName, List<string> errors)
        where TNumber : INumber<TNumber>
    {
        if (value <= TNumber.Zero)
        {
            errors.Add($"{fieldName} must be greater than zero.");
        }
    }

    public static void CheckInRange<TNumber>(TNumber value, TNumber min, TNumber max, string fieldName, List<string> errors)
        where TNumber : INumber<TNumber>
    {
        if (value < min || value > max)
        {
            errors.Add($"{fieldName} must be between {min} and {max}.");
        }
    }

    private static Result RequireNonNegativeCore<TEntity, TNumber>(TNumber value, string fieldName)
        where TNumber : INumber<TNumber>
    {
        if (value < TNumber.Zero)
        {
            return Result.Failure(Error.Validation<TEntity>($"{fieldName} cannot be negative."));
        }

        return Result.Success();
    }

    private static Result RequirePositiveCore<TEntity, TNumber>(TNumber value, string fieldName)
        where TNumber : INumber<TNumber>
    {
        if (value <= TNumber.Zero)
        {
            return Result.Failure(Error.Validation<TEntity>($"{fieldName} must be greater than zero."));
        }

        return Result.Success();
    }

    private static Result RequireInRangeCore<TEntity, TNumber>(TNumber value, TNumber min, TNumber max, string fieldName)
        where TNumber : INumber<TNumber>
    {
        if (value < min || value > max)
        {
            return Result.Failure(Error.Validation<TEntity>($"{fieldName} must be between {min} and {max}."));
        }

        return Result.Success();
    }
}
