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
        [HttpPost]
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
        [HttpGet]
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
                        Address = p.PickUpPoint.Addres,
                        Quantity = p.Quantity
                    }).ToList()
                })
                .ToList();

            return Ok(res);
        }

    }
}
