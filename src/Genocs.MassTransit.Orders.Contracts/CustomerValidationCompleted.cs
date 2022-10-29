using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface CustomerValidationCompleted
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        string CustomerNumber { get; }
    }
}
