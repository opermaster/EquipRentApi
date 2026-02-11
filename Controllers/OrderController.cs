using EquipRentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
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
        [HttpPost("create")]
        public ActionResult CreateOrder(OrderDto dto) {
            using var transaction = _context.Database.BeginTransaction();
            dto.Client.Email = dto.Client.Email.Trim();
            try {

                var client = _context.Client.FirstOrDefault(c => c.Email == dto.Client.Email);

                if (client is null) {
                    Console.WriteLine("CLIENT DOES NOT EXIST!!!!!    --------------------------");
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
                return BadRequest();
                throw;
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("inner")]
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
                    },
                    
                });

            return Ok(orders);
        }

        [HttpPut("personal")]
        public ActionResult<IEnumerable<OrderResponseDto>> GetOrdersPersonal(ClientDto _dto) {
            Client? client = _context.Client.FirstOrDefault(c => c.Email == _dto.Email);
            if (client is null) return BadRequest("Client with this email does`not exists!");
            var orders = _context.Orders.Where(o => o.ClientId == client.Id)
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
                    },
                    Address = o.PickUpPointEquipment.PickUpPoint.Addres
                });

            return Ok(orders);
        }
        [HttpPut("cancel")]
        public ActionResult CandelOrder(OrderCancelDto _dto) {
            Client? client = _context.Client.FirstOrDefault(c => c.Email == _dto.Client.Email);
            if (client is null) return BadRequest("Client with this email does`not exists!");
            Order? order = _context.Orders.FirstOrDefault(o => o.Id== _dto.Id);
            if (order is null) return BadRequest("Order with this Id does`not exists!");

            order.Status = OrderStatus.Rejected;
            _context.SaveChanges();
            return NoContent();
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
