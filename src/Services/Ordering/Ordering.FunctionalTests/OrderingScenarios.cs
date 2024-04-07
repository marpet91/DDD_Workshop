using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.API.DTOs;
using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.NewOrder;
using Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Behaviors;
using WebMVC.Services.ModelDTOs;
using Xunit;
using Xunit.Abstractions;

namespace Ordering.FunctionalTests
{
    public class OrderingScenarios
        : OrderingScenarioBase
    {
        private readonly ITestOutputHelper _output;

        public OrderingScenarios(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public async Task Get_get_all_stored_orders_and_response_ok_status_code()
        {
            using var server = CreateServer();
            var response = await server.CreateClient()
                .GetAsync(Get.Orders);

            await response.EnsureSuccessResponseCodeAsync(_output);
        }
        
        [Fact]
        public async Task Put_creates_order_and_buyer()
        {
            using var server = CreateServer();
            var newOrder = BuildNewOrder();
            var content = JsonContent.Create(newOrder);
            var response = await server.CreateClient()
                .PutAsync(Put.NewOrder, content);

            await response.EnsureSuccessResponseCodeAsync(_output);

            var orderLocation = response.Headers.Location;

            Assert.NotNull(orderLocation);
            
            Assert.True(int.TryParse(orderLocation.Segments.Last(), out var orderId));

            response = await server.CreateClient()
                .GetAsync(Get.OrderBy(orderId));

            await response.EnsureSuccessResponseCodeAsync(_output);

            var order = await response.Content.ReadFromJsonAsync<OrderDto>();
            
            Assert.Equal(newOrder.Street, order.Street);
            Assert.Equal(newOrder.OrderItems.Count, order.OrderItems.Count);
        }
        
        [Fact]
        public async Task Put_validates_request()
        {
            using var server = CreateServer();
            
            var newOrder = new NewOrderRequest();
            var content = JsonContent.Create(newOrder);
            var response = await server.CreateClient()
                .PutAsync(Put.NewOrder, content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cancel_order_no_order_created_bad_request_response()
        {
            using var server = CreateServer();
            var content = new StringContent(BuildOrder(), UTF8Encoding.UTF8, "application/json");
            var response = await server.CreateIdempotentClient()
                .PutAsync(Put.CancelOrder, content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Ship_order_no_order_created_bad_request_response()
        {
            using var server = CreateServer();
            var content = new StringContent(BuildOrder(), UTF8Encoding.UTF8, "application/json");
            var response = await server.CreateIdempotentClient()
                .PutAsync(Put.ShipOrder, content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        string BuildOrder()
        {
            var order = new OrderDTO()
            {
                OrderNumber = "-1"
            };
            return JsonSerializer.Serialize(order);
        }

        NewOrderRequest BuildNewOrder()
        {
            var newOrder = new NewOrderRequest
            {
                Street = "123 Cherry Park Ln",
                City = "Redmond",
                State = "WA",
                ZipCode = "98008",
                Country = "USA",
                OrderItems =
                [
                    new()
                    {
                        ProductId = 1,
                        ProductName = ".NET Bot Black Hoodie",
                        UnitPrice = 19.5m,
                        Units = 4,
                        PictureUrl = "/1.png"
                    },
                    new()
                    {
                        ProductId = 8,
                        ProductName = "Kudu Purple Hoodie",
                        UnitPrice = 8.5m,
                        Units = 2,
                        PictureUrl = "/8.png"
                    }
                ],
                CardHolderName = "Satya Nadella",
                CardTypeId = 1,
                CardNumber = "4111111111111111",
                CardExpiration = DateTime.UtcNow.AddYears(1),
                CardSecurityNumber = "123"
            };
            
            return newOrder;
        }
    }
}
