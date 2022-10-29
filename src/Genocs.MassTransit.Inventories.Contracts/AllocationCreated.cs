using System;

namespace Genocs.MassTransit.Inventories.Contracts
{
    public interface AllocationCreated
    {
        Guid AllocationId { get; }
        TimeSpan HoldDuration { get; }
    }
}
