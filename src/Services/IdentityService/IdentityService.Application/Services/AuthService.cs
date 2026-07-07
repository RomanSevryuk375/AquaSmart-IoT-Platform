// Ignore Spelling: Dto

using Contracts.Enums;
using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Services;

public class AuthService(
    UserManager<User> userManager,
    ISubscriptionRepository subscriptionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork,
    IMyHasher myHasher,
    IUserContext userContext) : IAuthService
{
    public async Task<Result<LoginResponseDto>> RegisterUserAsync(
        RegisterUserRequestDto registerDto,
        CancellationToken cancellationToken)
    {
        User? existingUser = await userManager.FindByEmailAsync(registerDto.Email);

        if (existingUser is not null)
        {
            return Result<LoginResponseDto>.Failure(Error.Conflict("Email.Busy",
                "{registerDto.Email} is busy."));
        }

        Result<User> createdResult = User.Create(
            NewId.NextGuid(),
            registerDto.Name,
            registerDto.Email,
            registerDto.PhoneNumber,
            Guid.Parse(SubscriptionType.Free),
            registerDto.TimaZone);
        if (createdResult.IsFailure)
        {
            return Result<LoginResponseDto>.Failure(createdResult.Error);
        }
        User user = createdResult.Value;

        Subscription? subscription = await subscriptionRepository
            .GetByIdAsync(user.SubscriptionId, cancellationToken);

        IReadOnlyList<string> permissions = subscription?.Permissions ?? [];

        IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            string error = string.Join(", ", result.Errors.Select(x => x.Description));
            return Result<LoginResponseDto>.Failure(Error.Conflict("Login.Failure",
                    $"Failed to register user {createdResult.Value.Id}: " +
                    $"{string.Join(", ", result.Errors.Select(x => x.Description))}"));
        }

        string accessToken = jwtProvider.GenerateToken(user, permissions);
        Result<string> refreshToken = await CreateAndPersistRefreshTokenAsync(user.Id, cancellationToken);

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
        User? existingUser = await userManager.FindByEmailAsync(loginUser.Email);

        if (existingUser is null)
        {
            return Result<LoginResponseDto>
                .Failure(Error.NotFound(
                    "User.NotFound",
                    $"{nameof(User)} with email {loginUser.Email} not found."));
        }

        Subscription? subscription = await subscriptionRepository
            .GetByIdAsync(existingUser!.SubscriptionId, cancellationToken);

        IReadOnlyList<string> permissions = subscription?.Permissions ?? [];

        bool isPasswordCorrect = await userManager
            .CheckPasswordAsync(existingUser, loginUser.Password);

        if (!isPasswordCorrect)
        {
            return Result<LoginResponseDto>
                .Failure(Error.Conflict(
                    "User.Conflict",
                    "Invalid password."));
        }

        string accessToken = jwtProvider.GenerateToken(existingUser, permissions);
        Result<string> refreshToken = await CreateAndPersistRefreshTokenAsync(existingUser.Id, cancellationToken);

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
        string[] parts = request.RefreshToken.Split('.');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out Guid tokenId))
        {
            return Result<LoginResponseDto>
                .Failure(Error.Validation(
                    "Token.Invalid",
                    "Invalid format"));
        }

        RefreshToken? tokenEntity = await refreshTokenRepository.GetByIdAsync(tokenId, cancellationToken);

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

        User? existingUser = await userManager
            .FindByIdAsync(tokenEntity.UserId.ToString());

        if (existingUser is null)
        {
            return Result<LoginResponseDto>
                .Failure(Error.NotFound(
                    "User.NotFound",
                    $"{nameof(User)} with email {tokenEntity.UserId} not found."));
        }

        Subscription? subscription = await subscriptionRepository
            .GetByIdAsync(existingUser.SubscriptionId, cancellationToken);
        IReadOnlyList<string> permissions = subscription?.Permissions ?? [];

        string accessToken = jwtProvider
            .GenerateToken(existingUser, permissions);

        Result<string> newRefreshToken = await
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
        string rawSecret = jwtProvider.GenerateRefreshToken();

        Result<RefreshToken> createdResult = RefreshToken.Create(
            userId,
            myHasher.Generate(rawSecret)
        );

        await refreshTokenRepository.AddAsync(createdResult.Value, ct);

        return Result<string>.Success($"{createdResult.Value.Id}.{rawSecret}");
    }

    private bool InvalidRefreshToken(RefreshToken tokenEntity, string[] parts)
    {
        return tokenEntity is null ||
            !myHasher.Verify(parts[1], tokenEntity.TokenHash) ||
            tokenEntity.IsRevoked ||
            tokenEntity.ExpiredAt < DateTime.UtcNow;
    }
}
