using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface OrderFulfillmentCompleted
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
    }
}
