using EquipRentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Pipelines;
using System.Security.Claims;

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

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public ActionResult<IEnumerable<OrderResponseDto>> GetOrders() {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = _context.Users
                .AsNoTracking()
                .First(u => u.Id == userId);

            var query = _context.Orders.AsQueryable();

            if (user.Role == UserRole.Manager && user.PickUpPointId.HasValue) {
                query = query.Where(o =>
                    o.PickUpPointEquipment.PickUpPointId == user.PickUpPointId.Value);
            }

            var orders = query
                .Select(o => new OrderResponseDto {
                    Id = o.Id,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    Status = o.Status,

                    Client = new ClientDto {
                        FirstName = o.Client.FirstName,
                        LastName = o.Client.LastName,
                        Email = o.Client.Email
                    },

                    Equipment = new EquipmentDto {
                        Id = o.PickUpPointEquipment.Equipment.Id,
                        Name = o.PickUpPointEquipment.Equipment.Name,
                        Price = o.PickUpPointEquipment.Equipment.Price,
                        Img = o.PickUpPointEquipment.Equipment.Img
                    }
                });

            return Ok(orders);
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut]
        public ActionResult UpdateOrder(OrderUpdateDto dto) {
            Order? order = _context.Orders.FirstOrDefault(o => o.Id == dto.Id);
            if(order is null) {
                return BadRequest("Order with this id does-not exist!");
            }
            order.Status = dto.Status;
            _context.SaveChanges();
            return NoContent();
        }
    }
}
