using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaseStudy.Models;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace CaseStudy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDBContext _context;

        public OrdersController(OrdersDBContext context)
        {
            _context = context;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderOutput>> PostOrder(OrderInput input)
        {
            if (input.Items.Count == 0)
            {
                return BadRequest("Order is empty!");
            }

            foreach (var item in input.Items)
            {
                if (GetExistingItem(item.IDItem) == null)
                {
                    return BadRequest($"Item #{item.IDItem} does not exist!");
                }
                if (item.Quantity <= 0)
                {
                    return BadRequest($"Quantity for #{item.IDItem} is lower than 1!");
                }
            }

            var highestOrderNumberInDB = _context.Orders.OrderByDescending(on => on.OrderNumber).Select(on => on.OrderNumber).FirstOrDefault();
            var predefinedOrderNumber = 100000000;

            Order order = new Order
            {
                IDOrder = new Guid(),
                CustomerName = input.CustomerName,
                OrderNumber = highestOrderNumberInDB != 0 ? highestOrderNumberInDB + 1 : predefinedOrderNumber + 1
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

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

            var response = new OrderOutput
            {
                IDOrder = order.IDOrder.ToString(),
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                Status = GetEnumName(order.Status),
                Items = order.Items
                .Select(item => new ItemOutput
                {
                    IDItem = item.IDItem,
                    Name = item.Item.Name,
                    Price = item.Item.Price,
                    Quantity = item.Quantity,
                    CalculatedPrice = item.Quantity * item.Item.Price
                }).ToList(),
                SumPrice = order.Items.Sum(x => x.Item.Price * x.Quantity)
            };

            return CreatedAtAction("GetOrder", new { orderNumber = order.OrderNumber }, response);
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderOutput>>> GetOrders()
        {
            List<OrderOutput> orders = _context.Orders.Include(i => i.Items)
                .Select(order => new OrderOutput
                {
                    IDOrder = order.IDOrder.ToString(),
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.CustomerName,
                    OrderDate = order.OrderDate,
                    Status = GetEnumName(order.Status),
                    Items = order.Items
                        .Select(item => new ItemOutput
                        {
                            IDItem = item.IDItem,
                            Name = item.Item.Name,
                            Price = item.Item.Price,
                            Quantity = item.Quantity,
                            CalculatedPrice = item.Quantity * item.Item.Price
                        }).ToList(),
                    SumPrice = order.Items.Sum(x => x.Item.Price * x.Quantity)
                }).ToList();

            return Ok(orders);
        }

        // GET: api/Orders/5
        [HttpGet("{orderNumber}")]
        public async Task<ActionResult<OrderOutput>> GetOrder(int orderNumber)
        {
            var dbOrder = _context.Orders.Include(i => i.Items).ThenInclude(i => i.Item).SingleOrDefault(on => on.OrderNumber == orderNumber);

            if (dbOrder == null)
            {
                return NotFound();
            }

            List<ItemOutput> items = dbOrder.Items
            .Select(item => new ItemOutput
            {
                IDItem = item.IDItem,
                Name = item.Item.Name,
                Price = item.Item.Price,
                Quantity = item.Quantity,
                CalculatedPrice = item.Quantity * item.Item.Price
            })
            .ToList();

            OrderOutput order = new OrderOutput
            {
                IDOrder = dbOrder.IDOrder.ToString(),
                OrderNumber = dbOrder.OrderNumber,
                CustomerName = dbOrder.CustomerName,
                OrderDate = dbOrder.OrderDate,
                Status = GetEnumName(dbOrder.Status),
                Items = items,
                SumPrice = items.Sum(x => x.CalculatedPrice)
            };

            return Ok(order);
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderPaymentAcceptance(string id)
        {
            var order = GetExistingOrder(id);
            if (order == null)
            {
                return BadRequest("Order does not exist.");
            }

            order.Status = OrderStatus.Paid;

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();


            return Ok($"Order {order.OrderNumber} has been paid!");
        }

        private Order GetExistingOrder(string id)
        {
            return _context.Orders.SingleOrDefault(x => x.IDOrder.ToString() == id);
        }

        private Item GetExistingItem(int id)
        {
            return _context.Items.SingleOrDefault(x => x.IDItem == id);
        }

        public string GetEnumName<T>(T enumValue) where T : Enum
        {
            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
            DescriptionAttribute attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? enumValue.ToString();
        }

    }
}
