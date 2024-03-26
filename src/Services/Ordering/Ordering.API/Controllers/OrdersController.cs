namespace Microsoft.eShopOnContainers.Services.Ordering.API.Controllers;

using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Extensions;
using Application.Commands;
using Application.Queries;
using Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Services;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<OrdersController> _logger;
    private readonly OrderingContext _orderingContext;

    public OrdersController(IIdentityService identityService,
        ILogger<OrdersController> logger, OrderingContext orderingContext)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        var address = new Address(message.Street, message.City, message.State, message.Country, message.ZipCode);
        var order = new Domain.AggregatesModel.OrderAggregate.Order(userId, userName, address, message.CardTypeId, message.CardNumber, message.CardSecurityNumber, message.CardHolderName, message.CardExpiration);

        foreach (var item in message.OrderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        _orderingContext.Orders.Add(order);

        var result = await _orderingContext.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest();
        }

        // Create or update the buyer details
        var cardTypeId = message.CardTypeId != 0 ? message.CardTypeId : 1;
        var buyer = await _orderingContext.Buyers.FindAsync(userId);
        bool buyerOriginallyExisted = buyer != null;

        if (!buyerOriginallyExisted)
        {
            buyer = new Buyer(userId, userName);
        }

        var paymentMethod = buyer.VerifyOrAddPaymentMethod(cardTypeId,
            $"Payment Method on {DateTime.UtcNow}",
            message.CardNumber,
            message.CardSecurityNumber,
            message.CardHolderName,
            message.CardExpiration,
            order.Id);

        if (buyerOriginallyExisted)
        {
            _orderingContext.Buyers.Update(buyer);
        }
        else
        {
            _orderingContext.Buyers.Add(buyer);
        }
        
        // Update order details
        order.SetPaymentId(paymentMethod.Id);
        order.SetBuyerId(buyer.Id);
        _orderingContext.Orders.Update(order);

        result = await _orderingContext.SaveChangesAsync() > 0;
        
        if (!result)
        {
            return BadRequest();
        }
        
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

            orderToUpdate.SetCancelledStatus();

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

            orderToUpdate.SetShippedStatus();
            
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
                total = order.GetTotal(),
                orderitems = order.OrderItems.Select(oi => new Orderitem
                {
                    productname = oi.GetOrderItemProductName(),
                    pictureurl = oi.GetPictureUri(),
                    unitprice = (double)oi.GetUnitPrice(),
                    units = oi.GetUnits()
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
                total = (double)o.GetTotal()
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
        _logger.LogInformation(
            "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            createOrderDraftCommand.GetGenericTypeName(),
            nameof(createOrderDraftCommand.BuyerId),
            createOrderDraftCommand.BuyerId,
            createOrderDraftCommand);
        
        var order = Ordering.Domain.AggregatesModel.OrderAggregate.Order.NewDraft();
        var orderItems = createOrderDraftCommand.Items.Select(i => i.ToOrderItemDTO());
        foreach (var item in orderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        var result = OrderDraftDTO.FromOrder(order);

        return result;
    }

}
