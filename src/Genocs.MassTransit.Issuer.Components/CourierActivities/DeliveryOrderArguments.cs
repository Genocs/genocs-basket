using System;

namespace Genocs.MassTransit.Issuer.Components.CourierActivities
{
    public interface DeliveryOrderArguments
    {
        Guid OrderId { get; }
        string ShippingAddress { get; }
    }
}
