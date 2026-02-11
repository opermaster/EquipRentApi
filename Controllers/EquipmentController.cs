using EquipRentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipRentApi.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public EquipmentController(DatabaseContext context) {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("new")]
        public async Task<IActionResult> CreateEquipment([FromForm] EquipmentCreateDto dto) {
            if (dto.Image == null || dto.Image.Length == 0)
                return BadRequest("Image required");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(dto.Image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid image format");

            if (dto.Image.Length > 5 * 1024 * 1024)
                return BadRequest("File too large");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create)) {
                await dto.Image.CopyToAsync(stream);
            }

            var equipment = new Equipment {
                Name = dto.Name,
                Price = dto.Price,
                Img = fileName
            };

            _context.Equipments.Add(equipment);
            await _context.SaveChangesAsync();

            return Ok(equipment);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("address/{id}")]
        public ActionResult DeletePointToEquipment(int id) {
            
            PickUpPointEquipment? pupe = _context.PickUpPointEquipments.FirstOrDefault(p=>p.Id == id);

            if (pupe is null) return BadRequest("PickUpPointEquipment with this id does not exists!");

            _context.PickUpPointEquipments.Remove(pupe);
            _context.SaveChanges();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("address")]
        public ActionResult AddPointToEquipment(PickUpPointEquipmentDto _dto) {
            PickUpPointEquipment pupe = _dto.ToPickUpPointEquipment();
            _context.PickUpPointEquipments.Add(pupe);
            _context.SaveChanges();
            return Created(nameof(AddPointToEquipment), new { id = pupe.Id, });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_point_equipments")]
        public ActionResult UpdatePointEquipment(PickUpPointEquipmentRequestDto dto) {
            var existing = _context.PickUpPointEquipments
                .Where(x => x.EquipmentId == dto.Id)
                .ToList();

            foreach (var pointDto in dto.Points) {
                var entity = existing.FirstOrDefault(x => x.Id == pointDto.Id);

                if (entity != null) {
                    entity.PickUpPointId = pointDto.PickUpPointId;
                    entity.Quantity = pointDto.Quantity;
                }
                else {
                    var newEntity = new PickUpPointEquipment {
                        EquipmentId = dto.Id,
                        PickUpPointId = pointDto.PickUpPointId,
                        Quantity = pointDto.Quantity
                    };

                    _context.PickUpPointEquipments.Add(newEntity);
                }
            }

            var dtoIds = dto.Points.Select(p => p.Id).ToList();

            var toDelete = existing
                .Where(x => !dtoIds.Contains(x.Id))
                .ToList();

            _context.PickUpPointEquipments.RemoveRange(toDelete);

            _context.SaveChanges();

            return Ok();
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_price/{id}/{price}")]
        public ActionResult UpdateEquipmentPrice(int id,decimal price) {
            Equipment? eq = _context.Equipments.FirstOrDefault(e => e.Id == id);
            if (eq is not null) {
                eq.Price = price;
            }
            else return BadRequest("Equipment with this id does not exists!");
            _context.SaveChanges();

            return Ok();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public ActionResult GetEquipments() {
            var res = _context.Equipments
                .Include(e => e.PickUpPoints)
                    .ThenInclude(p => p.PickUpPoint)
                .Select(e => new EquipmentDto {
                    Id = e.Id,
                    Name = e.Name,
                    Price = e.Price,
                    Img = e.Img,

                    PickUpPoints = e.PickUpPoints.Select(p => new EquipmentAvailabilityDto {
                        Id = p.Id,                         
                        PickUpPointId = p.PickUpPointId,   
                        Address = p.PickUpPoint.Addres,
                        Quantity = p.Quantity
                    }).ToList()
                })
                .ToList();
            return Ok(res);
        }
        [HttpPost("avaliable")]
        public ActionResult GetAvaliableEquipments(AvaliableEquipmentsRequestDto _dto) {
            var res = _context.Equipments
                .Include(e => e.PickUpPoints)
                    .ThenInclude(p => p.PickUpPoint)
                .Select(e => new EquipmentDto {
                    Id = e.Id,
                    Name = e.Name,
                    Price = e.Price,
                    Img = e.Img,

                    PickUpPoints = e.PickUpPoints.Select(p => new EquipmentAvailabilityDto {
                        Id = p.Id,
                        PickUpPointId = p.PickUpPointId,
                        Address = p.PickUpPoint.Addres,
                        Quantity = p.Quantity - _context.Orders
                                .Where(o =>
                                    o.StartDate < _dto.EndDate &&
                                    o.EndDate > _dto.StartDate
                                )
                                .Count(),
                    }).ToList()
                })
                .ToList();
            return Ok(res);
        }
    }
}
