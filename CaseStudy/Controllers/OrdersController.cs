using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaseStudy.Models;
using System.ComponentModel;
using System.Reflection;
using CaseStudy.Services;
using NuGet.Protocol;

namespace CaseStudy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDBContext _context;
        private readonly KafkaProducer _kafkaproducerService;
        private readonly IConfiguration _configuration;

        public OrdersController(OrdersDBContext context, KafkaProducer kafkaproducerService, IConfiguration configuration)
        {
            _context = context;
            _kafkaproducerService = kafkaproducerService;
            _configuration = configuration;
        }

        //Vytvoření objednávky
        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderOutput>> PostOrder([FromBody] OrderInput input)
        {
            //Objednávka musí obsahovat zboží
            if (input.Items.Count == 0)
            {
                return BadRequest("Order is empty!");
            }

            //Zboží musí existovat
            var existingItems = _context.Items.Where(item => input.Items.Select(i => i.IDItem).ToList().Contains(item.IDItem)).ToList();

            foreach (var item in input.Items)
            {
                if (existingItems.SingleOrDefault(i => i.IDItem == item.IDItem) != null)
                {
                    //Nelze koupit 0 položek
                    if (item.Quantity <= 0)
                    {
                        return BadRequest($"Quantity for #{item.IDItem} is lower than 1!");
                    }
                }
                else
                {
                    return BadRequest($"Item #{item.IDItem} does not exist!");
                }
            }

            //Návaznost číslování objednávek
            var highestOrderNumberInDB = _context.Orders.OrderByDescending(on => on.OrderNumber).Select(on => on.OrderNumber).FirstOrDefault();

            Order order = new()
            {
                IDOrder = new Guid(),
                CustomerName = input.CustomerName,
            };

            if (highestOrderNumberInDB > 0)
            {
                order.OrderNumber = highestOrderNumberInDB + 1;
            }

            _context.Orders.Add(order);
            //Získání ID nově vytvořené objednávky
            await _context.SaveChangesAsync();

            //Vytvoření záznamů ve spojovací tabulce mezi Objednávkou a Předmětem
            var items = input.Items
                .GroupBy(i => i.IDItem).Select(ni => new ItemInput
                {
                    IDItem = ni.Key,
                    Quantity = ni.Sum(i => i.Quantity)
                });

            foreach (var item in items)
            {
                _context.OrderItems.AddRange(
                    new OrderItems
                    {
                        IDItem = item.IDItem,
                        IDOrder = order.IDOrder,
                        Quantity = item.Quantity
                    });
            }

            await _context.SaveChangesAsync();

            //"Zkrášlení" výstupu detailu objednávky
            var response = MapOrderToOrderOutput(order);

            return CreatedAtAction("GetOrder", new { orderNumber = order.OrderNumber }, response);
        }

        //Výpis všech objednávek
        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderOutput>>> GetOrders()
        {
            var orders = _context.Orders.Include(i => i.Items)
                .ThenInclude(i => i.Item)
                .Select(order => MapOrderToOrderOutput(order))
                .ToList();

            return Ok(orders);
        }

        //Výpis konkrétní objednávky pro uživatele přes číslo objednávky
        // GET: api/Orders/5
        [HttpGet("{orderNumber}")]
        public async Task<ActionResult<OrderOutput>> GetOrder([FromRoute] int orderNumber)
        {
            var dbOrder = _context.Orders.Include(i => i.Items)
                .ThenInclude(i => i.Item)
                .SingleOrDefault(on => on.OrderNumber == orderNumber);

            //Objednávka musí existovat
            if (dbOrder == null)
            {
                return NotFound();
            }

            var response = MapOrderToOrderOutput(dbOrder);

            return Ok(response);
        }

        //Přijetí informace o zaplacení objednávky
        // POST: api/Orders/Payment
        [HttpPost("Payment")]
        public async Task<IActionResult> PostPaymentOrder([FromBody] MessageInput paymentInfo)
        {
            var order = GetExistingOrder(paymentInfo.IDOrder);
            if (order == null)
            {
                return BadRequest("Order does not exist.");
            }

            //Uložení platby do Kafka topicu
            var topic = _configuration["Kafka:PaymentTopic"];
            var message = new MessageInput 
            {
                IDOrder = order.IDOrder.ToString(),
                Paid = paymentInfo.Paid
            };

            _kafkaproducerService.ProduceAsync(topic, message.ToJson());
            return Ok($"Payment for order #{order.OrderNumber} has been processed");
        }

        //Obnovení objednávky do stavu Nová pro Zrušené objednávky
        // POST: api/Orders/Restore
        [HttpPost("Restore")]
        public async Task<IActionResult> PutOrderStatus([FromBody] IDOfOrder order)
        {
            var existingOrder = GetExistingOrder(order.IDOrder);
            if (existingOrder == null)
            {
                return BadRequest("Order does not exist.");
            }

            //Pouze zrušené objednávky
            if (existingOrder.Status != OrderStatus.Canceled)
            {
                return BadRequest("Order status cannot be changed");
            }

            existingOrder.Status = OrderStatus.New;
            _context.Entry(existingOrder).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok($"Status for order #{existingOrder.OrderNumber} is {existingOrder.Status}");
        }

        private Order? GetExistingOrder(string orderId)
        {
            return _context.Orders.SingleOrDefault(x => x.IDOrder.ToString() == orderId);
        }

        //Metoda pro získání uživatelsky přívětivějšího názvu pro enum stavu objednávky
        private string GetEnumName<T>(T enumValue) where T : Enum
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? enumValue.ToString();
        }

        //Metoda pro mapování objednávky do "úhlednějšího" objektu
        private OrderOutput MapOrderToOrderOutput(Order order)
        {
            var items = order.Items.Select(item => new ItemOutput
            {
                IDItem = item.IDItem,
                Name = item.Item.Name,
                Price = item.Item.Price,
                Quantity = item.Quantity,
                CalculatedPrice = item.Quantity * item.Item.Price
            }).ToList();

            return new OrderOutput
            {
                IDOrder = order.IDOrder.ToString(),
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                Status = GetEnumName(order.Status),
                Items = items,
                SumPrice = items.Sum(x => x.CalculatedPrice)
            };
        }
    }
}
