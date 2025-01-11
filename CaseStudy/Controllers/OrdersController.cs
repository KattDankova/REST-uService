using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaseStudy.Models;

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
        public async Task<ActionResult<Order>> PostOrder(OrderInput input)
        {
            if (input.Items.Count == 0)
            {
                return BadRequest("Order is empty!");
            }

            foreach (var item in input.Items)
            {
                if(GetExistingItem(item.IDItem) == null)
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

            var items = _context.Items.Where(item => input.Items.Select(dbItem => dbItem.IDItem).Contains(item.IDItem));

            Order order = new Order
            {
                CustomerName = input.CustomerName,
                Items = [.. items],
                OrderNumber = highestOrderNumberInDB != 0 ? highestOrderNumberInDB + 1 : predefinedOrderNumber + 1
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in input.Items)
            {
                var orderItem = _context.OrderItems.FirstOrDefault(x => x.IDItem == item.IDItem && x.IDOrder == order.IDOrder);
                orderItem.Quantity = item.Quantity;
                _context.Entry(orderItem).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { orderNumber = order.OrderNumber }, order);
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return Ok(await _context.Orders.ToListAsync());
        }

        // GET: api/Orders/5
        [HttpGet("{orderNumber}")]
        public async Task<ActionResult<Order>> GetOrder(int orderNumber)
        {
            var order = _context.Orders.SingleOrDefault(on => on.OrderNumber == orderNumber);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderPaymentAcceptance(int id)
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

        private Order GetExistingOrder(int id)
        {
            return _context.Orders.SingleOrDefault(x => x.IDOrder == id);
        }

        private Item GetExistingItem(int id)
        {
            return _context.Items.SingleOrDefault(x => x.IDItem == id);
        }

    }
}
