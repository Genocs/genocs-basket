using System;

namespace Genocs.MassTransit.Orders.Components.CourierActivities
{
    public interface AllocateInventoryArguments
    {
        Guid OrderId { get; }
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}
