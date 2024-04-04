namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.CardTypes;

[Route("api/v1/Orders")]
[Authorize]
[ApiController]
public class CardTypesController : ControllerBase
{
    private readonly OrderingContext _orderingContext;

    public CardTypesController(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
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
}