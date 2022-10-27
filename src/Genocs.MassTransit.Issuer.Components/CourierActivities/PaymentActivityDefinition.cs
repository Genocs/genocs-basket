using MassTransit;

namespace Genocs.MassTransit.Issuer.Components.CourierActivities
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
