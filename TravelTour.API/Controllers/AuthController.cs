using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TravelTour.API.DTOs;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại!!" });
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

            var userId = await _userRepository.CreateAsync(user);
            user.UserId = userId;

            // Tự động tạo hồ sơ khách hàng tương ứng cho tài khoản Customer mới
            var customer = new Customer
            {
                UserId = userId,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address ?? string.Empty
            };
            var customerId = await _customerRepository.CreateAsync(customer);

            return Ok(new
            {
                message = "Đăng ký thành công!!",
                userId = user.UserId,
                username = user.UserName,
                role = user.Role,
                customerId = customerId
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByUsernameAndPasswordAsync(dto.UserName, dto.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            // Đảm bảo tài khoản Customer luôn có hồ sơ trong bảng Customers
            var customer = await _customerRepository.GetByUserIdAsync(user.UserId);
            if (customer == null && user.Role == "Customer")
            {
                customer = new Customer
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = string.Empty
                };
                var customerId = await _customerRepository.CreateAsync(customer);
                customer.CustomerId = customerId;
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
                role = user.Role,
                customerId = customer?.CustomerId
            });
        }
    }
}
