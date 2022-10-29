using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface PaymentNotCaptured
    {
        Guid PaymentOrderId { get; }
        string PaymentCardNumber { get; }
        DateTime Timestamp { get; }
    }
}
