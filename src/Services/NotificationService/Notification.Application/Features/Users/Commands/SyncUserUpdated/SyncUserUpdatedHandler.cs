using AutoMapper;
using Contracts.Results;
using MediatR;
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
            SyncUserUpdatedCommand command = mapper.Map<SyncUserUpdatedCommand>(request);
            Result result = await sender.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }
        }

        Result? updateResult = currentUser!.UpdateContacts(request.Email, request.PhoneNumber);
        if (updateResult is not null)
        {
            return Result.Failure(updateResult.Error);
        }

        return Result.Success();
    }
}
