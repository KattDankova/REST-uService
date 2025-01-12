using CaseStudy.Models;

namespace CaseStudy.Services
{
    public interface IOrderService
    {
        Task MarkOrderAsPaid(string orderId, bool paid);
    }

    public class OrderService : IOrderService
    {
        private readonly OrdersDBContext _context;

        public OrderService(OrdersDBContext context)
        {
            _context = context;
        }

        public async Task MarkOrderAsPaid(string orderId, bool paid)
        {
            var order = _context.Orders.SingleOrDefault(x => x.IDOrder.ToString() == orderId);

            if (order != null)
            {
                if (order.Status != OrderStatus.New)
                {
                    Console.WriteLine($"Order with ID {orderId} is canceled.");
                }
                else
                {
                    order.Status = paid ? OrderStatus.Paid : OrderStatus.Canceled;

                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                Console.WriteLine($"Order with ID {orderId} not found.");
            }
        }
    }
}
