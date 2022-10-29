using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface PaymentProgress
    {
        Guid PaymentOrderId { get; }
        string PaymentCardNumber { get; }
        DateTime Timestamp { get; }
    }
}
