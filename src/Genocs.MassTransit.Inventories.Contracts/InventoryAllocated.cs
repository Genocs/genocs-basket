using System;

namespace Genocs.MassTransit.Inventories.Contracts
{
    public interface InventoryAllocated
    {
        Guid AllocationId { get; }
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}
