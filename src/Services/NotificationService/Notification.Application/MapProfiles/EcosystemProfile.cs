using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;

namespace Notification.Application.MapProfiles;

public sealed class EcosystemProfile : Profile
{
    public EcosystemProfile()
    {
        CreateMap<Features.Ecosystems.Commands.SyncEcosystemUpdated.SyncEcosystemUpdatedCommand, SyncEcosystemCreatedCommand>();

        CreateMap<EcosystemCreatedEvent, SyncEcosystemCreatedCommand>();

        CreateMap<EcosystemUpdatedEvent, Features.Ecosystems.Commands.SyncEcosystemUpdated.SyncEcosystemUpdatedCommand>();

        CreateMap<EcosystemDeletedEvent, Features.Ecosystems.Commands.SyncEcosystemDeleted.SyncEcosystemUpdatedCommand>();
    }
}

