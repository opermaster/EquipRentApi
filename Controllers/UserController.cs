using EquipRentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipRentApi.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public UserController(DatabaseContext context) {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("new_user")]
        public ActionResult RegisterUser(UserDto _user) {
            bool exist = _context.Users.Any(u => u.Login == _user.Login);
            if (exist) return Conflict("User with this lgoin already exists");

            if (_user.Role != UserRole.Admin && _user.PickUpPointId is null) {
                return BadRequest("No pickup poit was setted!");
            }
            User user = _user.ToUser();

            user.PickUpPointId = _user.PickUpPointId;
            _context.Users.Add(user);
            _context.SaveChanges();

            return Created(nameof(RegisterUser), new { id = user.Id, });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("/by-id{id}")]
        public ActionResult DeleteUser(int id) {
            User? _user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (_user is null) return Conflict("User with this id does not exists");

            _context.Users.Remove(_user);
            _context.SaveChanges();

            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/{newPointId:int}")]
        public ActionResult UpdateUser(int id,int newPointId) {
            User? _user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (_user is null) return Conflict("User with this id does not exists");

            _user.PickUpPointId = newPointId;

            _context.SaveChanges();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult GetUsers() {
            var users = _context.Users
                .Include(u => u.PickUpPoint)
                .Select(u => new UserDto {
                    Id = u.Id,
                    Login = u.Login,
                    Role = u.Role,
                    Address = u.PickUpPoint != null
                        ? u.PickUpPoint.Addres
                        : null
                }).OrderBy(u => u.Id);

            return Ok(users);
        }
    }
}
