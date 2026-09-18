using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using TravelTour.API.Data;
using TravelTour.API.DTOs;
using TravelTour.API.Models;
namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.Username);
            if (existingUser != null)
            {
                return BadRequest("Tên đăng nhập đã tồn tại!!");
            }
            var user = new User
            {
                UserName = dto.Username,
                PassWord = dto.Password, 
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = "Customer"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Đăng ký thành công!!",
                userId = user.UserId,
                username = user.UserName,
                role = user.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserName == dto.UserName &&
                    u.PassWord == dto.Password);

            if (user == null)
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ExpireMinutes"]!)),
                signingCredentials: credentials);

            return Ok(new
            {
                message = "Đăng nhập thành công.",
                token = new JwtSecurityTokenHandler().WriteToken(token),
                userId = user.UserId,
                userName = user.UserName,
                fullName = user.FullName,
                role = user.Role
            });
        }
    }
}
