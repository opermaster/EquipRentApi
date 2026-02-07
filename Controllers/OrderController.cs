using EquipRentApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipRentApi.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public OrderController(DatabaseContext context) {
            _context = context;
        }
        [HttpPost]
        public ActionResult CreateOrder(OrderDto dto) {
            using var transaction = _context.Database.BeginTransaction();

            try {
                var client = _context.Client.FirstOrDefault(c => c.Email == dto.Client.Email);

                if (client == null) {
                    client = new Client {
                        FirstName = dto.Client.FirstName,
                        LastName = dto.Client.LastName,
                        Email = dto.Client.Email
                    };

                    _context.Client.Add(client);
                    _context.SaveChanges();
                }

                var order = dto.ToOrder();

                order.ClientId = client.Id;
                order.CreatedAt = DateTime.UtcNow;
                order.Status = OrderStatus.Confirmed;

                _context.Orders.Add(order);
                _context.SaveChanges();

                transaction.Commit();

                return Created(nameof(CreateOrder),new { id = order.Id, status = order.Status.ToString() });
            }
            catch {
                transaction.Rollback();
                throw;
            }
        }

    }
}
