using System;

namespace Genocs.MassTransit.Inventories.Contracts
{
    public interface AllocateInventory
    {
        Guid AllocationId { get; }
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}
