using AutoMapper;
using Contracts.Abstractions;
using Contracts.Results;
using MediatR;

namespace Device.Infrastructure.Messaging;

internal abstract class MediatRIntegrationEventConsumer<TEvent, TCommand>(ISender sender, IMapper mapper)
    : IConsumer<TEvent> where TEvent : class where TCommand : ICommand
{
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        TCommand command = mapper.Map<TCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
