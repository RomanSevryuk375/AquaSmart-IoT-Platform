using System.Reflection;
using IdentityService.API.Endpoints.Profile;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure;

namespace Identity.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    internal static readonly Assembly DomainAssembly = typeof(User).Assembly;
    internal static readonly Assembly ApplicationAssembly = typeof(LoginResponseDto).Assembly;
    internal static readonly Assembly InfrastructureAssembly = typeof(IdentityDbContext).Assembly;
    internal static readonly Assembly ApiAssembly = typeof(GetMyProfileEndpoint).Assembly;
}
