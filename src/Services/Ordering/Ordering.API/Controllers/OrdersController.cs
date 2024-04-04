using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.ShipOrder;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrders;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Controllers;

using ApiDto;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly OrderingContext _orderingContext;
    private readonly IMediator _mediator;

    public OrdersController(OrderingContext orderingContext, IMediator mediator)
    {
        _orderingContext = orderingContext;
        _mediator = mediator;
    }

    [Route("new")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> NewOrderAsync([FromBody] NewOrderModel model)
    {
        // Get the user info
        var userId = HttpContext.User.FindFirst("sub").Value;
        var userName = HttpContext.User.FindFirst("name").Value;
        
        // Create the order
        var address = new Address
        {
            Street = model.Street,
            City = model.City,
            State = model.State,
            Country = model.Country,
            ZipCode = model.ZipCode
        };
        var order = new Order
        {
            OrderStatusId = OrderStatus.Submitted.Id,
            OrderDate = DateTime.UtcNow,
            Address = address,
        };

        foreach (var item in model.OrderItems)
        {
            OrderManager.AddOrderItem(order, item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        _orderingContext.Orders.Add(order);
        
        await _orderingContext.SaveChangesAsync();
        
        // Create or update the buyer details
        var cardTypeId = model.CardTypeId != 0 ? model.CardTypeId : 1;
        var buyer = await _orderingContext.Buyers
            .Where(b => b.IdentityGuid == userId)
            .Include(b => b.PaymentMethods)
            .SingleOrDefaultAsync();
        
        bool buyerOriginallyExisted = buyer != null;

        if (!buyerOriginallyExisted)
        {
            buyer = new Buyer
            {
                IdentityGuid = userId,
                Name = userName
            };
        }

        string alias = $"Payment Method on {DateTime.UtcNow}";
        PaymentMethod paymentMethod;
        var existingPayment = buyer.PaymentMethods
            .SingleOrDefault(p => p.CardTypeId == cardTypeId
                                  && p.CardNumber == model.CardNumber
                                  && p.Expiration == model.CardExpiration);

        if (existingPayment != null)
        {
            paymentMethod = existingPayment;
        }
        else
        {
            var payment = new PaymentMethod
            {
                CardNumber = model.CardNumber,
                SecurityNumber = model.CardSecurityNumber,
                CardHolderName = model.CardHolderName,
                Alias = alias,
                Expiration = model.CardExpiration,
                CardTypeId = cardTypeId
            };

            buyer.PaymentMethods.Add(payment);

            paymentMethod = payment;
        }

        if (buyerOriginallyExisted)
        {
            _orderingContext.Buyers.Update(buyer);
        }
        else
        {
            _orderingContext.Buyers.Add(buyer);
        }
        
        await _orderingContext.SaveChangesAsync();
        
        // Update order details with buyer information
        order.Buyer = buyer;
        order.PaymentMethodId = paymentMethod.Id;
        
        _orderingContext.Orders.Update(order);
        
        await _orderingContext.SaveChangesAsync();

        return CreatedAtAction("GetOrder", new { orderId = order.Id }, null);
    }
    [Route("cancel")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CancelOrderAsync([FromBody] CancelOrderModel model)
    {
        var orderToUpdate = await _orderingContext.Orders.FindAsync(model.OrderNumber);
        if (orderToUpdate == null)
        {
            return BadRequest();
        }

        orderToUpdate.OrderStatusId = OrderStatus.Cancelled.Id;
        orderToUpdate.Description = $"The order was cancelled.";

        await _orderingContext.SaveChangesAsync();

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
            var order = await _orderingContext.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                .Where(o => o.Id == orderId)
                .SingleOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }
            
            var queryOrder = new OrderDto
            {
                OrderNumber = order.Id,
                Description = order.Description,
                Street = order.Address.Street,
                City = order.Address.City,
                ZipCode = order.Address.ZipCode,
                Country = order.Address.Country,
                Date = order.OrderDate,
                Status = order.OrderStatus.ToString(),
                Total = OrderManager.GetTotal(order),
                OrderItems = order.OrderItems.Select(orderItem => new OrderItemDto
                {
                    ProductName = orderItem.ProductName,
                    PictureUrl = orderItem.PictureUrl,
                    UnitPrice = orderItem.UnitPrice,
                    Units = orderItem.Units
                }).ToList()
            };
            
            return Ok(queryOrder);
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

    [Route("cardtypes")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CardTypeDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<CardTypeDto>>> GetCardTypesAsync()
    {
        var cardTypes = await _orderingContext.CardTypes.ToListAsync();

        var result = cardTypes
            .Select(c => new CardTypeDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();

        return Ok(result);
    }

    [Route("draft")]
    [HttpPost]
    public ActionResult<OrderDraftModel> CreateOrderDraftFromBasketDataAsync([FromBody] CreateOrderDraftModel createOrderDraftModel)
    {
        var order = new Order();
        var orderItems = createOrderDraftModel.Items.Select(i => i.ToOrderItemDTO());
        foreach (var item in orderItems)
        {
            OrderManager.AddOrderItem(order, item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        var result = OrderDraftModel.FromOrder(order);

        return result;
    }

}
