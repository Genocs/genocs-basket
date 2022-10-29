using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface OrderFulfillmentFaulted
    {
        Guid OrderId { get; }
    }
}
