using MassTransit;

namespace Genocs.MassTransit.Issuer.Components.CourierActivities
{
    public class DeliveryOrderActivityDefinition :
        ActivityDefinition<DeliveryOrderActivity, DeliveryOrderArguments, DeliveryOrderLog>
    {
        public DeliveryOrderActivityDefinition()
        {
            ConcurrentMessageLimit = 20;
        }
    }
}
