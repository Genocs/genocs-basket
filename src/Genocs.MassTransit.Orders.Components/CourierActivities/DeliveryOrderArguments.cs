using System;

namespace Genocs.MassTransit.Orders.Components.CourierActivities
{
    public interface DeliveryOrderArguments
    {
        Guid OrderId { get; }
        string ShippingAddress { get; }
    }
}
