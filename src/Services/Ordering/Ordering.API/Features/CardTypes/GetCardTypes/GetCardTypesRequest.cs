using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.DTOs;

public class GetCardTypesRequest : IRequest<IEnumerable<CardTypeDto>>
{
    
}