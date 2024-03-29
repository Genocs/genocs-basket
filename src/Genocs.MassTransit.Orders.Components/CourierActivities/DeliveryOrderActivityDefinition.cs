﻿using MassTransit;

namespace Genocs.MassTransit.Orders.Components.CourierActivities
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
