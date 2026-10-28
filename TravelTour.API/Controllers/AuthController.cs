using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TravelTour.API.DTOs;
using TravelTour.API.Models;
using TravelTour.API.Repositories;
using TravelTour.API.Services;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _configuration = configuration;
            _emailService = emailService;
        }

      

        [HttpPost("register-send-otp")]
        public async Task<IActionResult> RegisterSendOtp([FromBody] RegisterSendOtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest(new { message = "Vui lòng nhập Tên đăng nhập!" });
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Vui lòng nhập địa chỉ Email (Gmail)!" });
            }

            // Kiểm tra username đã tồn tại chưa
            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username.Trim());
            if (existingUser != null)
            {
                return BadRequest(new { message = "Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên đăng nhập khác!" });
            }

            // Kiểm tra email đã được đăng ký chưa
            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email.Trim());
            if (existingEmail != null)
            {
                return BadRequest(new { message = "Email này đã được liên kết với một tài khoản khác trong hệ thống!" });
            }

            // Sinh mã OTP 6 số ngẫu nhiên
            var otpCode = Random.Shared.Next(100000, 999999).ToString();

            // Gửi email xác nhận đăng ký qua Gmail SMTP
            var emailSent = await _emailService.SendRegistrationOtpEmailAsync(dto.Email.Trim(), dto.FullName?.Trim() ?? "", otpCode);

            return Ok(new
            {
                success = true,
                message = emailSent
                    ? $"Mã xác thực OTP đã được gửi thành công về địa chỉ Gmail: {dto.Email}"
                    : $"Đã tạo mã OTP xác nhận đăng ký cho {dto.Username}!",
                email = dto.Email.Trim(),
                username = dto.Username.Trim(),
                emailSent = emailSent,
                otpCode = otpCode
            });
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

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Vui lòng nhập địa chỉ Email của bạn!" });
            }

            var email = dto.Email.Trim();
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản nào được đăng ký với email này trong hệ thống!" });
            }

            // Sinh mã OTP 6 số ngẫu nhiên
            var otpCode = Random.Shared.Next(100000, 999999).ToString();

            // Thử gửi email thực tế qua Gmail SMTP
            var emailSent = await _emailService.SendOtpEmailAsync(email, user.FullName, otpCode);

            return Ok(new
            {
                success = true,
                message = emailSent
                    ? $"Mã xác thực OTP đã được gửi thành công về hộp thư Gmail: {email}"
                    : $"Xác nhận tài khoản thành công! Mã OTP khôi phục của bạn là: {otpCode}",
                email = user.Email,
                userName = user.UserName,
                fullName = user.FullName,
                emailSent = emailSent,
                otpCode = otpCode
            });
        }

        [HttpPost("reset-password-by-email")]
        public async Task<IActionResult> ResetPasswordByEmail([FromBody] ResetPasswordByEmailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Vui lòng cung cấp email tài khoản!" });
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { message = "Vui lòng nhập mật khẩu mới!" });
            }

            if (dto.NewPassword.Length < 4)
            {
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 4 ký tự!" });
            }

            var user = await _userRepository.GetByEmailAsync(dto.Email.Trim());
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản với email đã cung cấp!" });
            }

            user.PassWord = dto.NewPassword.Trim();
            await _userRepository.UpdateAsync(user);

            return Ok(new
            {
                success = true,
                message = "Đặt lại mật khẩu thành công! Bạn có thể sử dụng mật khẩu mới để đăng nhập ngay bây giờ.",
                userName = user.UserName
            });
        }
    }
}
