using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface OrderRequested
    {
        Guid OrderId { get; }
        string CustomerNumber { get; }
        string ShippingAddress { get; }
        string PaymentCardNumber { get; }
        DateTime Timestamp { get; }
    }
}
