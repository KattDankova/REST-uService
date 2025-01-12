using CaseStudy.Models;

namespace CaseStudy.Services
{
    public interface IOrderService
    {
        Task MarkOrderAsPaid(MessageInput message);
    }

    public class OrderService : IOrderService
    {
        private readonly OrdersDBContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(OrdersDBContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task MarkOrderAsPaid(MessageInput message)
        {
            var order = _context.Orders.SingleOrDefault(x => x.IDOrder.ToString() == message.IDOrder);

            if (order == null)
            {
                _logger.LogWarning($"Order with ID {message.IDOrder} not found.");
                return;
            }

            if (order.Status != OrderStatus.New)
            {
                _logger.LogWarning($"Order with ID {order.IDOrder} cannot be marked as {(message.Paid ? "Paid" : "Canceled")} because it is in {order.Status} status.");
                return;
            }
            order.Status = message.Paid ? OrderStatus.Paid : OrderStatus.Canceled;

            await _context.SaveChangesAsync();
        }
    }
}
