using MassTransit;

namespace Genocs.MassTransit.Issuer.Components.CourierActivities
{
    public class AllocateInventoryActivityDefinition :
        ActivityDefinition<AllocateInventoryActivity, AllocateInventoryArguments, AllocateInventoryLog>
    {
        public AllocateInventoryActivityDefinition()
        {
            ConcurrentMessageLimit = 10;
        }
    }
}
