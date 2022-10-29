using System;

namespace Genocs.MassTransit.Inventories.Contracts
{
    public interface AllocationReleaseRequested
    {
        Guid AllocationId { get; }
        string Reason { get; }
    }
}
