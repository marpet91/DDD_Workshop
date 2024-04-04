using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.DTOs;

public class GetCardTypesHandler : IRequestHandler<GetCardTypesRequest, IEnumerable<CardTypeDto>>
{
    private readonly OrderingContext _orderingContext;

    public GetCardTypesHandler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }
    
    public async Task<IEnumerable<CardTypeDto>> Handle(GetCardTypesRequest request, CancellationToken cancellationToken)
    {
        var cardTypes = await _orderingContext.CardTypes.ToListAsync();

        var result = cardTypes
            .Select(c => new CardTypeDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();
        
        return result;
    }
}