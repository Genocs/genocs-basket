using System;

namespace Genocs.MassTransit.Orders.Contracts
{
    public interface CheckPayment
    {
        Guid PaymentOrderId { get; }
    }

    public interface PaymentStatus
    {
        Guid PaymentOrderId { get; }
        string CustomerNumber { get; }
        string Status { get; }
        int ReadyStatus { get; }
    }

    public interface PaymentNotFound
    {
        Guid PaymentOrderId { get; }
    }
}
