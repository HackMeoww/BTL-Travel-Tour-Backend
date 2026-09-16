using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.PassWord == dto.Password);
            if (user == null)
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng!!");
            }
            return Ok(new
            {
                message = "Đăng nhập thành công!!",
                userId = user.UserId,
                username = user.UserName,
                fullname = user.FullName,
                email = user.Email,
                phone = user.Phone,
                role = user.Role
            });
        }
    }
}
