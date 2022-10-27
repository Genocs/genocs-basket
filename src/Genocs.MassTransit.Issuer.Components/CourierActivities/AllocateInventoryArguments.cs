using System;

namespace Genocs.MassTransit.Issuer.Components.CourierActivities
{
    public interface AllocateInventoryArguments
    {
        Guid OrderId { get; }
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}
