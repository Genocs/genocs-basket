using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface OrderRejected
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        string CustomerNumber { get; }
        string Reason { get; }
    }
}
