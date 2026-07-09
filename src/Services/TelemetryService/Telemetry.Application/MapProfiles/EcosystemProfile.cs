using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Telemetry.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;
using Telemetry.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

namespace Telemetry.Application.MapProfiles;

public sealed class EcosystemProfile : Profile
{
    public EcosystemProfile()
    {
        CreateMap<EcosystemCreatedEvent, SyncEcosystemCreatedCommand>();

        CreateMap<EcosystemDeletedEvent, SyncEcosystemDeletedCommand>();
    }
}
