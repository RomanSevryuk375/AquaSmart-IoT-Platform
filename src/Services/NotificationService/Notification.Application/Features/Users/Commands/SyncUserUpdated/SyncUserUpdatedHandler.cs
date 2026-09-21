using AutoMapper;
using BuildingBlocks.Domain.Results;
using MediatR;
using Notification.Application.Features.Users.Commands.SyncUserCreated;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Users.Commands.SyncUserUpdated;

public sealed class SyncUserUpdatedHandler(
    IUserRepository userRepository,
    ISender sender, IMapper mapper) : IRequestHandler<SyncUserUpdatedCommand, Result>
{
    public async Task<Result> Handle(SyncUserUpdatedCommand request, CancellationToken cancellationToken)
    {
        User? currentUser = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (currentUser is null)
        {
            SyncUserCreatedCommand createCommand = mapper.Map<SyncUserCreatedCommand>(request);
            return await sender.Send(createCommand, cancellationToken);
        }

        Result updateResult = currentUser.UpdateContacts(request.Email, request.PhoneNumber);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        return Result.Success();
    }
}
