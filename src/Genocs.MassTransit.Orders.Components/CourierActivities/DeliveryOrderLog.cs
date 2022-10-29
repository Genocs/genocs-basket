using System;

namespace Genocs.MassTransit.Orders.Components.CourierActivities
{
    public interface DeliveryOrderLog
    {
        string AuthorizationCode { get; }
    }
}
