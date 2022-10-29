using System;

namespace Genocs.MassTransit.Inventories.Contracts
{
    public interface AllocationConfirmed
    {
        Guid AllocationId { get; }
    }
}
