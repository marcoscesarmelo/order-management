using Microsoft.AspNetCore.Mvc;
using OrderGenerator.Exceptions;
using OrderGenerator.Models;
using OrderGenerator.Services;

namespace OrderGenerator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                if (order == null)
                {
                    return BadRequest("Ordem inválida.");
                }
                _orderService.AddOrder(order);
                return Ok(new { message = "Ordem criada com sucesso!" });
            }
            catch (OrderAccumulatorException oaexc)
            {
                return BadRequest(new { message = oaexc.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

    }
}
