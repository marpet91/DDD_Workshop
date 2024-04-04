using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CancelOrder;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CreateOrderDraft;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrder;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrders;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.NewOrder;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.ShipOrder;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Route("new")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> NewOrderAsync([FromBody] NewOrderRequest request)
    {
        // Get the user info
        var userId = HttpContext.User.FindFirst("sub").Value;
        var userName = HttpContext.User.FindFirst("name").Value;

        request.UserId = userId;
        request.UserName = userName;

        var orderId = await _mediator.Send(request);
        
        return CreatedAtAction("GetOrder", new { orderId }, null);
    }
    
    [Route("cancel")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CancelOrderAsync([FromBody] CancelOrderRequest request)
    {
        var isSuccess = await _mediator.Send(request);

        if (!isSuccess)
        {
            return BadRequest();
        }

        return Ok();
    }

    [Route("ship")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ShipOrderAsync([FromBody] ShipOrderRequest model)
    {
        var isSuccess = await _mediator.Send(model);
        
        if (!isSuccess) 
        {
            return BadRequest();
        }

        return Ok();
    }

    [Route("{orderId:int}")]
    [HttpGet]
    [ProducesResponseType(typeof(OrderDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> GetOrderAsync(int orderId)
    {
        try
        {
            var order = await _mediator.Send(new GetOrderRequest { OrderId = orderId });

            if (order == null)
            {
                return NotFound();
            }
            
            return Ok(order);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersAsync()
    {
        var orderSummary = await _mediator.Send(new GetOrdersRequest());

        return Ok(orderSummary);
    }

    [Route("draft")]
    [HttpPost]
    public async Task<ActionResult<OrderDraftModel>> CreateOrderDraftFromBasketDataAsync([FromBody] CreateOrderDraftRequest request)
    {
        var result = await _mediator.Send(request);

        return result;
    }

}