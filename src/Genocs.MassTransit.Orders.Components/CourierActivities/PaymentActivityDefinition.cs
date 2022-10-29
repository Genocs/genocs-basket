using Genocs.MassTransit.Issuer.Components.CourierActivities;
using MassTransit;

namespace Genocs.MassTransit.Orders.Components.CourierActivities
{
    public class PaymentActivityDefinition :
        ActivityDefinition<PaymentActivity, PaymentArguments, PaymentLog>
    {
        public PaymentActivityDefinition()
        {
            ConcurrentMessageLimit = 20;
        }
    }
}
