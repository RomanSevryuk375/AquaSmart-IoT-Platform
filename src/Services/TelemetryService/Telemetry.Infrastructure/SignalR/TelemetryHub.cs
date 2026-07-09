using Microsoft.AspNetCore.SignalR;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.SignalR;

public sealed class TelemetryHub : Hub<IRawTelemetryBoardClient>
{
    public async Task SubscribeToRawTelemetry(string ecosystemId) => await Groups.AddToGroupAsync(Context.ConnectionId, ecosystemId);

    public async Task UnsubscribeFromRawTelemetry(string ecosystemId) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, ecosystemId);
}
