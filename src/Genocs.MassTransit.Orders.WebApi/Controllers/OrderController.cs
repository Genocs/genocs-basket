using Genocs.MassTransit.Orders.Contracts;
using MassTransit;
using MassTransit.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Genocs.MassTransit.Orders.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;

    private readonly ISendEndpointProvider _sendEndpointProvider;

    private readonly IRequestClient<OrderStatus> _checkOrderClient;

    public OrderController(ILogger<OrderController> logger,
        ISendEndpointProvider sendEndpointProvider,
        IRequestClient<OrderStatus> checkOrderClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sendEndpointProvider = sendEndpointProvider ?? throw new ArgumentNullException(nameof(sendEndpointProvider));
        _checkOrderClient = checkOrderClient ?? throw new ArgumentNullException(nameof(checkOrderClient));
    }


    [HttpGet(Name = "")]
    public async Task<IActionResult> Get(Guid orderId)
    {
        var (orderStatus, notFound) = await _checkOrderClient.GetResponse<OrderStatus, OrderNotFound>(new
        {
            orderId
        });

        if (orderStatus.IsCompletedSuccessfully)
        {
            var response = await orderStatus;
            return Ok(response.Message);
        }
        else
        {
            var response = await notFound;
            return NotFound(response.Message);
        }
    }


    /// <summary>
    /// The entry point API used to request the order
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="customerNumber"></param>
    /// <param name="paymentCardNumber"></param>
    /// <returns></returns>
    [HttpPost(Name = "")]
    public async Task<IActionResult> Post(Guid orderId, string customerNumber, string paymentCardNumber, string shippingAddress)
    {
        _logger.LogCritical("Foolish Log!!!");
        using var source = new ActivitySource("ExampleTracer");
        // A span
        using var activity = source.StartActivity("Placeholder to call Service B");

        // Simple tag
        activity?.AddTag("orderId", orderId);


        //Genocs.MassTransit.Contracts:OrderSubmitted
        //var interfaceType = typeof(OrderRequest);

        // The message is sent to the queue, it is possible to send the message to a rabbit exchange as well
        // As well ase you can use a service to creat the URI based on the contract namespace
        // {KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}
        var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"exchange:Genocs.MassTransit.Orders.Contracts:OrderRequest"));

        await endpoint.Send<OrderRequest>(new
        {
            OrderId = orderId,
            InVar.Timestamp,
            CustomerNumber = customerNumber,
            PaymentCardNumber = paymentCardNumber,
            ShippingAddress = shippingAddress
        });

        return Ok(orderId);
    }

}