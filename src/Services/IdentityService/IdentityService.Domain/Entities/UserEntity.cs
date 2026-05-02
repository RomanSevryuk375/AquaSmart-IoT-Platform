using Contracts.Abstractions;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace IdentityService.Domain.Entities;

public class UserEntity : IdentityUser<Guid>, IEntity
{
    private UserEntity() { }

    private UserEntity(
        string name, 
        string email, 
        string phonenumber, 
        Guid subscriptionId,
        string timeZone)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PhoneNumber = phonenumber;
        UserName = email; 
        SubscriptionId = subscriptionId;
        SubscriptionEndDate = DateTime.UtcNow; 
        CreatedAt = DateTime.UtcNow;
        TimeZone = timeZone;
    }

    public string Name { get; private set; } = string.Empty;
    public Guid SubscriptionId { get; private set; }
    public DateTime SubscriptionEndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string TimeZone { get; private set; }

    public static (UserEntity? user, List<string> errors) Create(
        string name, 
        string email, 
        string phoneNumber,
        Guid subscriptionId,
        string timeZone)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Name is required");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            errors.Add("Phone number is required");
        }

        if (string.IsNullOrWhiteSpace(timeZone))
        {
            errors.Add("Time zone is required");
        }

        if (!Regex.IsMatch(phoneNumber, @"^(\+375|80)(29|44|33|25)\d{7}$"))
        {
            errors.Add("Phone number should be in format +375XXXXXXXXX or 80XXXXXXXXX");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var user = new UserEntity(
            name.Trim(),
            email.Trim(),
            phoneNumber.Trim(),
            subscriptionId,
            timeZone.Trim());

        return (user, errors);
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetTimeZone(string timeZone)
    {
        TimeZone = timeZone;
    }

    public void SetSubscription(Guid subscriptionId, int durationDays)
    {
        SubscriptionId = subscriptionId;
        SubscriptionEndDate = DateTime.UtcNow.AddDays(durationDays);
    }
}