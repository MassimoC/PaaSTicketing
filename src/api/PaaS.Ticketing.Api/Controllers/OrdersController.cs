using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PaaS.Ticketing.Events;
using CloudNative.CloudEvents;
using System.Net.Mime;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

using PaaS.Ticketing.ApiLib.DTOs;
using PaaS.Ticketing.ApiLib.Entities;
using PaaS.Ticketing.ApiLib.Repositories;
using PaaS.Ticketing.ApiLib.Extensions;
using PaaS.Ticketing.ApiLib.Factories;
using PaaS.Ticketing.Events.Data;
using LoggingContext = PaaS.Ticketing.Events.Logging.Constants;
using PaaS.Ticketing.Security;




namespace PaaS.Ticketing.Api.Controllers
{
    [Route("core/v1/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ITelemetryClientFactory _telemetryClientFactory;
        private readonly IVaultService _vaultService;

        public OrdersController(IOrdersRepository ordersRepository, 
            ILogger<OrdersController> logger, 
            IConfiguration configuration, 
            ITelemetryClientFactory telemetryClientFactory,
            IVaultService vaultService)
        {
            _ordersRepository = ordersRepository;
            _logger = logger;
            _configuration = configuration;
            _telemetryClientFactory = telemetryClientFactory;
            _vaultService = vaultService;
        }

        /// <summary>
        /// Get single order (id)
        /// </summary>
        /// <param name="id">Order identifier</param>
        /// <remarks>Search order by Id</remarks>
        /// <returns>Return an order</returns>
        [HttpGet("{id}", Name = "Orders_GetOrderById")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Order object", typeof(OrderDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Order  not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            _logger.LogInformation("API - Order controller - GetOrderById");

            var orderDb = await _ordersRepository.GetOrderByIdAsync(id);
            var result = Mapper.Map<OrderDto>(orderDb);
            return Ok(result);
        }

        /// <summary>
        /// Get single order
        /// </summary>
        /// <param name="token">Ticket identifier</param>
        /// <remarks>Get information of a single order</remarks>
        /// <returns>Return an order</returns>
        [HttpGet("links/{token}", Name = "Orders_GetOrderDetails")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Order object", typeof(OrderDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Order  not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/json", "application/problem+json")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetails(String token)
        {
            _logger.LogInformation("API - Order controller - GetOrderDetails");

            var claimsIdentity = User.Identity as ClaimsIdentity;

            var order = await _ordersRepository.GetOrderAsync(token);
            var result = Mapper.Map<OrderDto>(order);
            return Ok(result);
        }

        /// <summary>
        /// Get orders
        /// </summary>
        /// <param name="status">Indicates whether to return a specific status only</param>
        /// <remarks>Get list of orders</remarks>
        /// <returns>Return list of orders</returns>
        [HttpGet(Name = "Orders_GetOrders")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Orders list", typeof(IEnumerable<OrderDto>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Orders  not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> GetOrders([FromQuery(Name = "status")]string status = "")
        {
            _logger.LogInformation("API - Order controller - GetOrders");
            if (status.ToUpper() == "ERRORS" || status.ToUpper() == "ERROR") throw new ArgumentException("This should be a 400, 500 just for debug purposes.");
            var orders = await _ordersRepository.GetOrdersAsync(status);
            var results = Mapper.Map<IEnumerable<OrderDto>>(orders);
            _logger.LogInformation($"Returning values");
            return Ok(results);
        }

        /// <summary>
        /// Place a new order
        /// </summary>
        /// <param name="orderCreate">Order json representation</param>
        /// <remarks>Place new order</remarks>
        /// <returns>Return the order details</returns>
        [HttpPost(Name = "Orders_PlaceOrder")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Order created", typeof(OrderDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "User or Concert not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Consumes("application/json")]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderCreateDto orderCreate)
        {
            // dependency tracking
            var telemetryClient = _telemetryClientFactory.Create();

            _logger.LogInformation("API - Order controller - PlaceOrders");
            ConcertUser entityConcertUser;
            try
            {
                entityConcertUser = Mapper.Map<ConcertUser>(orderCreate);
                _ordersRepository.PlaceOrderAsync(entityConcertUser);
                await _ordersRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex.InnerException.Message.Contains("PK_"))
                {
                    return BadRequest(new ProblemDetailsError(StatusCodes.Status400BadRequest, "Duplicate order."));
                }
                throw;
            }

            _logger.LogInformation($"Retrieving new order");
            var orderDb = await _ordersRepository.GetOrderAsync(entityConcertUser.Token);
            var orderDto = Mapper.Map<OrderDto>(orderDb);

            // dependency tracking
            var current = Activity.Current;
            var requestActivity = new Activity("command://order.pay");
            var requestOperation = telemetryClient.StartOperation<RequestTelemetry>(requestActivity);
            try
            {
                // drop message in the queue
                _logger.LogInformation($"Drop message in the queue");

                //TODO KeyVault : local MSI and dicker
                var sbConnectionString = String.Empty;
                try
                {
                    sbConnectionString = _vaultService.GetSecret("cn-servicebus").Result;
                }
                catch (Exception ex)
                {
                    // TODO local debug with docker 
                    // MSI + docker not working in debug mode?
                    _logger.LogError(ex.Message);
                }
                if (String.IsNullOrEmpty(sbConnectionString)) sbConnectionString = _configuration.GetConnectionString(name: "ServiceBus");

                var pub = new Publisher("q-payment-in", sbConnectionString);
                var cloudEvent = new CloudEvent("command://order.pay", new Uri("app://ticketing.api"))
                {
                    Id = Guid.NewGuid().ToString(),
                    ContentType = new ContentType(MediaTypeNames.Application.Json),
                    Data = JsonConvert.SerializeObject(new PaymentContext()
                    {
                        Attendee = orderDto.Attendee,
                        OrderId = orderDto.OrderId.ToString(),
                        Token = orderDto.Token,
                    })
                };

                _logger.LogInformation(new EventId((int)LoggingContext.EventId.Processing),
                                      LoggingContext.Template,
                                      "cloud event publishing [command://order.pay]",
                                      LoggingContext.EntityType.Order.ToString(),
                                      LoggingContext.EventId.Processing.ToString(),
                                      LoggingContext.Status.Pending.ToString(),
                                      "correlationId",
                                      LoggingContext.CheckPoint.Publisher.ToString(),
                                      "long description");

                _logger.LogInformation("COMMAND - Sending message to the bus.");
                var jsonFormatter = new JsonEventFormatter();
                var messageBody = jsonFormatter.EncodeStructuredEvent(cloudEvent, out var contentType);
                await pub.SendMessagesAsync(Encoding.UTF8.GetString(messageBody));
            }
            catch (Exception ex)
            {
                // dependency tracking
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                // dependency tracking
                telemetryClient.StopOperation(requestOperation);
            }

            _logger.LogInformation($"Returning order token '{entityConcertUser.Token}'");
            return CreatedAtRoute("Orders_GetOrderDetails",
                new { token = entityConcertUser.Token },
                orderDto);
        }

        /// <summary>
        /// Update Order
        /// </summary>
        /// <param name="id">Order identifier</param>
        /// <param name="order">Operation to be performed on the Player in json-patch+json format</param>
        /// <remarks>Update the order (incremental update with Json Patch)</remarks>
        /// <returns>Acknowledge the object has been updated</returns>
        [HttpPatch("{id}", Name = "Orders_UpdateIncrementalJsonPatch")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "The data has been updated", typeof(JsonPatchDocument))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Order not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Consumes("application/json-patch+json")]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> UpdateIncrementalJsonPatch(Guid id, [FromBody]JsonPatchDocument<OrderDto> order)
        {
            _logger.LogInformation("API - Order controller - UpdateIncrementalJsonPatch");

            var concertuserPatch = Mapper.Map<JsonPatchDocument<ConcertUser>>(order);
            var orderDb = await _ordersRepository.GetOrderByIdAsync(id);
            if (orderDb == null)
            {
                _logger.LogInformation("Order not found.");
                return NotFound(new ProblemDetailsError(StatusCodes.Status404NotFound));
            }
            //Apply the patch to the DB. 
            _logger.LogInformation("Patching object.");
            concertuserPatch.ApplyTo(orderDb);
            await _ordersRepository.UpdateOrderAsync(orderDb);

            _logger.LogInformation("Return order");
            return Ok(orderDb);
        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="id">Order identifier</param>
        /// <param name="orderStatus">Status object</param>
        /// <remarks>Update the order status</remarks>
        /// <returns>Accepted</returns>
        [HttpPost("{id}/state", Name = "Orders_UpdateOrderStatus")]
        [SwaggerResponse((int)HttpStatusCode.Accepted, "Status change has been accepted. No response body", typeof(OrderStatusDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Order not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Consumes("application/json")]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> ChangeOrderStatus(Guid id, [FromBody]OrderStatusDto orderStatus)
        {
            _logger.LogInformation("API - Order controller - ChangeOrderStatus");
            var orderDb = await _ordersRepository.GetOrderByIdAsync(id);
            if (orderDb == null)
            {
                return NotFound(new ProblemDetailsError(StatusCodes.Status404NotFound));
            }
            _logger.LogInformation("Update order.");
            await _ordersRepository.UpdateOrderStatusAsync(id, orderStatus.Status);

            _logger.LogInformation("Return accepted");
            return Accepted();
        }
    }
}