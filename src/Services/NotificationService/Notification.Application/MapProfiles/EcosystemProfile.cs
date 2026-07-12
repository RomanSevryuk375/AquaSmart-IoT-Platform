using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemUpdated;

namespace Notification.Application.MapProfiles;

public sealed class EcosystemProfile : Profile
{
    public EcosystemProfile()
    {
        CreateMap<Features.Ecosystems.Commands.SyncEcosystemUpdated.SyncEcosystemUpdatedCommand, SyncEcosystemCreatedCommand>();

        CreateMap<EcosystemCreatedEvent, SyncEcosystemCreatedCommand>();

        CreateMap<EcosystemUpdatedEvent, SyncEcosystemUpdatedCommand>();

        CreateMap<EcosystemDeletedEvent, SyncEcosystemDeletedCommand>();
    }
}

