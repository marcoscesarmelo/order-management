using Fix;
using OrderGenerator.Models;

namespace OrderGenerator.Services
{
    public class OrderService
    {
        private static List<Order> _orders = new List<Order>();

        public async Task AddOrder(Order order)
        {
            _orders.Add(order);
            new FixUtil().SendOrder(order);
        }

      }
}
