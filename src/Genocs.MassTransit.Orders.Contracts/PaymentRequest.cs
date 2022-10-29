using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface PaymentRequest
    {
        Guid PaymentOrderId { get; }
        Guid OrderId { get; }
        string CustomerNumber { get; }
        string PaymentCardNumber { get; }
        DateTime Timestamp { get; }
    }
}
