﻿using Order = Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate.Order;

namespace GrpcOrdering;

public class OrderingService : OrderingGrpc.OrderingGrpcBase
{
    private readonly ILogger<OrderingService> _logger;

    public OrderingService(ILogger<OrderingService> logger)
    {
        _logger = logger;
    }

    public override async Task<OrderDraftDTO> CreateOrderDraftFromBasketData(CreateOrderDraftCommand createOrderDraftCommand, ServerCallContext context)
    {
        _logger.LogInformation("Begin grpc call from method {Method} for ordering get order draft {CreateOrderDraftCommand}", context.Method, createOrderDraftCommand);

        var command = new AppCommand.CreateOrderDraftCommand(
                        createOrderDraftCommand.BuyerId,
                        this.MapBasketItems(createOrderDraftCommand.Items));


        var order = Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate.OrderManager.NewDraft();
        var orderItems = command.Items.Select(i => i.ToOrderItemDTO());
        foreach (var item in orderItems)
        {
            OrderManager.AddOrderItem(order, item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        var data = AppCommand.OrderDraftDTO.FromOrder(order);

        if (data != null)
        {
            context.Status = new Status(StatusCode.OK, $" ordering get order draft {createOrderDraftCommand} do exist");

            return this.MapResponse(data);
        }
        else
        {
            context.Status = new Status(StatusCode.NotFound, $" ordering get order draft {createOrderDraftCommand} do not exist");
        }

        return new OrderDraftDTO();
    }

    public OrderDraftDTO MapResponse(AppCommand.OrderDraftDTO order)
    {
        var result = new OrderDraftDTO()
        {
            Total = (double)order.Total,
        };

        order.OrderItems.ToList().ForEach(i => result.OrderItems.Add(new OrderItemDTO()
        {
            Discount = (double)i.Discount,
            PictureUrl = i.PictureUrl,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = (double)i.UnitPrice,
            Units = i.Units,
        }));

        return result;
    }

    public IEnumerable<ApiModels.BasketItem> MapBasketItems(RepeatedField<BasketItem> items)
    {
        return items.Select(x => new ApiModels.BasketItem()
        {
            Id = x.Id,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            UnitPrice = (decimal)x.UnitPrice,
            OldUnitPrice = (decimal)x.OldUnitPrice,
            Quantity = x.Quantity,
            PictureUrl = x.PictureUrl,
        });
    }
}
