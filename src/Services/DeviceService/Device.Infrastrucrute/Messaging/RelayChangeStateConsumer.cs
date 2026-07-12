using AutoMapper;
using Contracts.Events.RelayEvents;
using Device.Application.Features.RelayCommands.Command.SetRelayState;
using MediatR;

namespace Device.Infrastructure.Messaging;

internal sealed class RelayChangeStateConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<ChangeRelayStateEvent, SetRelayStateCommand>(sender, mapper);

