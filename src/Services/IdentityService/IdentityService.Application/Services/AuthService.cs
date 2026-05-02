using Contracts.Enums;
using Contracts.Events.UserEvents;
using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Services;

public class AuthService(
    UserManager<UserEntity> userManager,
    ISubscriptionRepository subscriptionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPublishEndpoint publishEndpoint,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork,
    IMyHasher myHasher,
    IUserContext userContext) : IAuthService
{
    public async Task<Result<LoginResponseDto>> RegisterUserAsync(
        RegisterUserRequestDto registerDto,
        CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(registerDto.Email);

        if (existingUser is not null)
        {
            return Result<LoginResponseDto>
                .Failure(Error.Conflict("Email.Busy", "{registerDto.Email} is busy."));
        }

        var (user, errors) = UserEntity.Create(
            registerDto.Name,
            registerDto.Email,
            registerDto.PhoneNumber,
            Guid.Parse(SubscriptionEnum.Free),
            registerDto.TimaZone);

        if (user is null)
        {
            return Result<LoginResponseDto>
                .Failure(Error.Conflict(
                    "User.Invalid",
                   $"Failed to create {nameof(UserEntity)}: {string.Join(", ", errors)}"));
        }

        var subscription = await subscriptionRepository
            .GetByIdAsync(user.SubscriptionId, cancellationToken);

        var permissions = subscription?.Permissions ?? [];

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(x => x.Description));
            return Result<LoginResponseDto>
                .Failure(Error.Conflict(
                    "Login.Failure",
                    $"Failed to register user {user.Id}: " +
                    $"{string.Join(", ", result.Errors.Select(x => x.Description))}"));
        }

        await publishEndpoint.Publish(new UserCreatedEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            CreatedAt = user.CreatedAt,
            TimeZone = user.TimeZone,
        }, cancellationToken);

        var accessToken = jwtProvider.GenerateToken(user, permissions);
        var refreshToken = await CreateAndPersistRefreshTokenAsync(user.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LoginResponseDto>.Success(
            new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Value,
            });
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(
        LoginUserRequestDto loginUser,
        CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(loginUser.Email);

        if (existingUser is null)
        {
            return Result<LoginResponseDto>
                .Failure(Error.NotFound(
                    "User.NotFound",
                    $"{nameof(UserEntity)} with email {loginUser.Email} not found."));
        }

        var subscription = await subscriptionRepository
            .GetByIdAsync(existingUser!.SubscriptionId, cancellationToken);

        var permissions = subscription?.Permissions ?? [];

        bool isPasswordCorrect = await userManager
            .CheckPasswordAsync(existingUser, loginUser.Password);

        if (!isPasswordCorrect)
        {
            return Result<LoginResponseDto>
                .Failure(Error.Conflict(
                    "User.Conflict",
                    "Invalid password."));
        }

        var accessToken = jwtProvider.GenerateToken(existingUser, permissions);
        var refreshToken = await CreateAndPersistRefreshTokenAsync(existingUser.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LoginResponseDto>.Success(
            new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Value,
            });
    }

    public async Task<Result<LoginResponseDto>> LoginWithRefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        var parts = request.RefreshToken.Split('.');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var tokenId))
        {
            return Result<LoginResponseDto>
                .Failure(Error.Validation(
                    "Token.Invalid",
                    "Invalid format"));
        }

        var tokenEntity = await refreshTokenRepository.GetByIdAsync(tokenId, cancellationToken);

        if (tokenEntity is not null && tokenEntity.IsUsed)
        {
            await refreshTokenRepository.DeleteTokensByUserIdAsync(tokenEntity.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<LoginResponseDto>
                .Failure(Error.Conflict(
                    "Security.Breach",
                    "Token reuse detected. All sessions revoked."));
        }

        if (InvalidRefreshToken(tokenEntity!, parts))
        {
            return Result<LoginResponseDto>
                .Failure(Error.Validation(
                    "Token.Invalid",
                    "Token is invalid or expired"));
        }

        tokenEntity!.MarkAsUsed();
        await refreshTokenRepository
            .UpdateTokenAsync(tokenEntity, cancellationToken);

        var existingUser = await userManager
            .FindByIdAsync(tokenEntity.UserId.ToString());

        if (existingUser is null)
        {
            return Result<LoginResponseDto>
                .Failure(Error.NotFound(
                    "User.NotFound",
                    $"{nameof(UserEntity)} with email {tokenEntity.UserId} not found."));
        }

        var subscription = await subscriptionRepository
            .GetByIdAsync(existingUser.SubscriptionId, cancellationToken);
        var permissions = subscription?.Permissions ?? [];

        var accessToken = jwtProvider
            .GenerateToken(existingUser, permissions);

        var newRefreshToken = await
            CreateAndPersistRefreshTokenAsync(existingUser.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LoginResponseDto>.Success(
            new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Value,
            });
    }

    public async Task<Result> LogoutAsync(CancellationToken cancellationToken)
    {
        await refreshTokenRepository
            .DeleteTokensByUserIdAsync(userContext.UserId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<string>> CreateAndPersistRefreshTokenAsync(
        Guid userId,
        CancellationToken ct)
    {
        var rawSecret = jwtProvider.GenerateRefreshToken();

        var refreshTokenEntity = RefreshTokenEntity.Create(
            userId,
            myHasher.Generate(rawSecret)
        );

        await refreshTokenRepository
            .AddTokenAsync(refreshTokenEntity, ct);

        return Result<string>
            .Success($"{refreshTokenEntity.Id}.{rawSecret}");
    }

    private bool InvalidRefreshToken(RefreshTokenEntity tokenEntity, string[] parts)
    {
        return tokenEntity is null ||
            !myHasher.Verify(parts[1], tokenEntity.TokenHash) ||
            tokenEntity.IsRevoked ||
            tokenEntity.ExpiredAt < DateTime.UtcNow;
    }
}