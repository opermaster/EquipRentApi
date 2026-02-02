using EquipRentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EquipRentApi.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public AuthController(DatabaseContext context) {
            _context = context;
        }
        
        [HttpPost("login")]
        public ActionResult Login(UserDto _login) {
            {
                //UserDto _u = new UserDto();
                //_u.Login = "admin";
                //_u.Password = "apass";
                //_u.Role = UserRole.Admin;
                //
                //User u = _u.ToUser();
                //
                //_context.Users.Add(u);
                //_context.SaveChanges();
            }
            User? _user = _context.Users.FirstOrDefault(u => u.Login == _login.Login);
            if (_user is null) return Unauthorized("Ivalid login");

            if (!UserDto.VerifyPassword(_login.Password, _user.PasswordHash)) {
                return Unauthorized("Ivalid password");
            }
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()),
                new Claim(ClaimTypes.Name, _user.Login),
                new Claim(ClaimTypes.Role, _user.Role.ToString()),
            };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(new { Token = token, Role = _user.Role.ToString() });

        }
    }
}
