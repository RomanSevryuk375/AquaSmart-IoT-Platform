using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.SignalR;

[Authorize]
public sealed class TelemetryHub(IEcosystemRepository ecosystemRepository) : Hub<IRawTelemetryBoardClient>
{
    [HubMethodName("SubscribeToRawTelemetry")]
    public async Task SubscribeToRawTelemetryAsync(string ecosystemId)
    {
        if (!Guid.TryParse(ecosystemId, out Guid parsedEcosystemId))
        {
            throw new HubException("Invalid ecosystem ID format.");
        }

        Guid userId = Context.User?.GetUserId() ?? Guid.Empty;
        if (userId == Guid.Empty)
        {
            throw new HubException("Unauthorized.");
        }

        bool isOwner = await ecosystemRepository.UserOwnsEcosystemAsync(
            parsedEcosystemId,
            userId,
            Context.ConnectionAborted);
        if (!isOwner)
        {
            throw new HubException("Access denied.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ecosystemId, Context.ConnectionAborted);
    }

    [HubMethodName("UnsubscribeFromRawTelemetry")]
    public async Task UnsubscribeFromRawTelemetryAsync(string ecosystemId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ecosystemId, Context.ConnectionAborted);
}
