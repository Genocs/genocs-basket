using System;

namespace Genocs.MassTransit.Issuer.Components.CourierActivities
{
    public interface DeliveryOrderLog
    {
        string AuthorizationCode { get; }
    }
}
