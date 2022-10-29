using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface PaymentNotAuthorized
    {
        Guid PaymentOrderId { get; }
        string PaymentCardNumber { get; }
        DateTime Timestamp { get; }
    }
}
