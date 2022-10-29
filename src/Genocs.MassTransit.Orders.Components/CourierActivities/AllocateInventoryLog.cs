using System;

namespace Genocs.MassTransit.Orders.Components.CourierActivities
{
    public interface AllocateInventoryLog
    {
        Guid AllocationId { get; }
    }
}
