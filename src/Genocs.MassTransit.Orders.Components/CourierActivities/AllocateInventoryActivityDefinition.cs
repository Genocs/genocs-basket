using MassTransit;

namespace Genocs.MassTransit.Orders.Components.CourierActivities
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
