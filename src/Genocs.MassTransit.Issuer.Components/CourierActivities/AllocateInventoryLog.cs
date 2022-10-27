using System;

namespace Genocs.MassTransit.Issuer.Components.CourierActivities
{
    public interface AllocateInventoryLog
    {
        Guid AllocationId { get; }
    }
}
