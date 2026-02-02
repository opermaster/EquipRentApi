using EquipRentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipRentApi.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class PointController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public PointController(DatabaseContext context) {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("new_point")]
        public ActionResult RegisterPoint(PickUpPointDto _point) {
            bool exist = _context.PickUpPoints.Any(u => u.Addres == _point.Addres);
            if (exist) return Conflict("User with this lgoin already exists");

            PickUpPoint user = _point.ToPickUpPoint();
            _context.PickUpPoints.Add(user);
            _context.SaveChanges();

            return Created(nameof(RegisterPoint), new { id = user.Id, });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public ActionResult DeletePoint(int id) {
            PickUpPoint? _point = _context.PickUpPoints.FirstOrDefault(u => u.Id == id);
            if (_point is null) return Conflict("Point with this id does not exists");

            _context.PickUpPoints.Remove(_point);
            _context.SaveChanges();

            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public ActionResult UpdatePoint(int id, UpdatePointDto _upoint) {
            PickUpPoint? _point = _context.PickUpPoints.FirstOrDefault(u => u.Id == id);
            if (_point is null) return Conflict("Point with this id does not exists");

            _point.Addres = _upoint.Address;
            _point.PhoneNumber = _upoint.Phone;

            _context.SaveChanges();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult GetPoints() {
            var points = _context.PickUpPoints.Select(p => new PickUpPointDto {
                Id = p.Id,
                Addres = p.Addres,
                PhoneNumber = p.PhoneNumber,
            }).OrderBy(p=>p.Id);
            return Ok(points);
        }
    }
}
