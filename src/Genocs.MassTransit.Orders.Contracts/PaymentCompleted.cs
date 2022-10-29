using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface PaymentCompleted
    {
        Guid PaymentOrderId { get; }
        string PaymentCardNumber { get; }
        DateTime Timestamp { get; }
    }
}
