using System;

namespace Genocs.MassTransit.Inventories.Contracts
{
    public interface AllocationHoldDurationExpired
    {
        Guid AllocationId { get; }
    }
}
