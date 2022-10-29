using Genocs.MassTransit.Orders.Contracts;
using MassTransit;
using System.Threading.Tasks;

namespace Genocs.MassTransit.Orders.Components.Consumers
{
    public class FaultConsumer :
            IConsumer<Fault<FulfillOrder>>
    {
        public Task Consume(ConsumeContext<Fault<FulfillOrder>> context)
        {
            return Task.CompletedTask;
        }
    }
}
