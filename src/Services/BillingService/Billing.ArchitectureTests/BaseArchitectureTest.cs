using System.Reflection;
using Billing.Domain.Entities;

namespace Billing.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    internal static readonly Assembly DomainAssembly = typeof(BillingCustomer).Assembly;
}
