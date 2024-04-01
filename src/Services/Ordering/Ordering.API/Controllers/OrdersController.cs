namespace Microsoft.eShopOnContainers.Services.Ordering.API.Controllers;

using Application.Commands;
using Application.Queries;
using Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Services;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly OrderingContext _orderingContext;

    public OrdersController(IIdentityService identityService, OrderingContext orderingContext)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _orderingContext = orderingContext;
    }

    [Route("new")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> NewOrderAsync([FromBody] NewOrder message, [FromHeader(Name = "x-requestid")] string requestId)
    {
        // Get the user info
        var userId = HttpContext.User.FindFirst("sub").Value;
        var userName = HttpContext.User.FindFirst("name").Value;
        
        // Create the order
        var address = new Address
        {
            Street = message.Street,
            City = message.City,
            State = message.State,
            Country = message.Country,
            ZipCode = message.ZipCode
        };
        var order = new Domain.AggregatesModel.OrderAggregate.Order
        {
            OrderStatusId = OrderStatus.Submitted.Id,
            IsDraft = true,
            OrderDate = DateTime.UtcNow,
            Address = address,
        };

        foreach (var item in message.OrderItems)
        {
            OrderManager.AddOrderItem(order, item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        _orderingContext.Orders.Add(order);
        
        await _orderingContext.SaveChangesAsync();
        
        // Create or update the buyer details
        var cardTypeId = message.CardTypeId != 0 ? message.CardTypeId : 1;
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
                                  && p.CardNumber == message.CardNumber
                                  && p.Expiration == message.CardExpiration);

        if (existingPayment != null)
        {
            paymentMethod = existingPayment;
        }
        else
        {
            var payment = new PaymentMethod
            {
                CardNumber = message.CardNumber,
                SecurityNumber = message.CardSecurityNumber,
                CardHolderName = message.CardHolderName,
                Alias = alias,
                Expiration = message.CardExpiration,
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
        
        return Ok();
    }
    [Route("cancel")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CancelOrderAsync([FromBody] CancelOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
    {
        if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
        {
            var orderToUpdate = await _orderingContext.Orders.FindAsync(command.OrderNumber);
            if (orderToUpdate == null)
            {
                return BadRequest();
            }

            orderToUpdate.OrderStatusId = OrderStatus.Cancelled.Id;
            orderToUpdate.Description = $"The order was cancelled.";

            await _orderingContext.SaveChangesAsync();
        }
        else
        {
            return BadRequest();
        }

        return Ok();
    }

    [Route("ship")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ShipOrderAsync([FromBody] ShipOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
    {
        if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
        {
            var orderToUpdate = await _orderingContext.Orders.FindAsync(command.OrderNumber);
            if (orderToUpdate == null)
            {
                return BadRequest();
            }

            orderToUpdate.OrderStatusId = OrderStatus.Shipped.Id;
            orderToUpdate.Description = "The order was shipped.";

            await _orderingContext.SaveChangesAsync();
        }
        else 
        {
            return BadRequest();
        }

        return Ok();
    }

    [Route("{orderId:int}")]
    [HttpGet]
    [ProducesResponseType(typeof(Order), (int)HttpStatusCode.OK)]
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
            
            var queryOrder = new Order
            {
                ordernumber = order.Id,
                description = order.Description,
                street = order.Address.Street,
                city = order.Address.City,
                zipcode = order.Address.ZipCode,
                country = order.Address.Country,
                date = order.OrderDate,
                status = order.OrderStatus.ToString(),
                total = Domain.AggregatesModel.OrderAggregate.OrderManager.GetTotal(order),
                orderitems = order.OrderItems.Select(oi => new Orderitem
                {
                    productname = oi.ProductName,
                    pictureurl = oi.PictureUrl,
                    unitprice = (double)oi.UnitPrice,
                    units = oi.Units
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
    [ProducesResponseType(typeof(IEnumerable<OrderSummary>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<OrderSummary>>> GetOrdersAsync()
    {
        var userid = _identityService.GetUserIdentity();
        var orders = await _orderingContext.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .Where(o => o.Buyer.IdentityGuid == userid)
            .ToListAsync();

        var orderSummary = orders
            .Select(o => new OrderSummary
            {
                ordernumber = o.Id,
                date = o.OrderDate,
                status = o.OrderStatus.ToString(),
                total = (double)Domain.AggregatesModel.OrderAggregate.OrderManager.GetTotal(o)
            });

        return Ok(orderSummary);
    }

    [Route("cardtypes")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CardType>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<CardType>>> GetCardTypesAsync()
    {
        var cardTypes = await _orderingContext.CardTypes.ToListAsync();

        var result = cardTypes
            .Select(c => new CardType
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();

        return Ok(result);
    }

    [Route("draft")]
    [HttpPost]
    public ActionResult<OrderDraftDTO> CreateOrderDraftFromBasketDataAsync([FromBody] CreateOrderDraftCommand createOrderDraftCommand)
    {
        var order = Ordering.Domain.AggregatesModel.OrderAggregate.OrderManager.NewDraft();
        var orderItems = createOrderDraftCommand.Items.Select(i => i.ToOrderItemDTO());
        foreach (var item in orderItems)
        {
            OrderManager.AddOrderItem(order, item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        var result = OrderDraftDTO.FromOrder(order);

        return result;
    }

}
