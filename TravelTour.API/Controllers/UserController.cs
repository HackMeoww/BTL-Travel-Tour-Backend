using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.DTOs;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;

        public UserController(
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IBookingRepository bookingRepository)
        {
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _bookingRepository = bookingRepository;
        }

        // GET: api/User
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var customers = await _customerRepository.GetAllAsync();
            var customerMap = customers.Where(c => c.UserId > 0)
                                       .GroupBy(c => c.UserId)
                                       .ToDictionary(g => g.Key, g => g.First());

            var result = users.Select(u => new
            {
                u.UserId,
                u.UserName,
                u.FullName,
                u.Email,
                u.Phone,
                u.Role,
                CustomerId = customerMap.TryGetValue(u.UserId, out var c) ? (int?)c.CustomerId : null,
                Address = customerMap.TryGetValue(u.UserId, out var cust) ? cust.Address : string.Empty
            });

            return Ok(result);
        }

        // GET: api/User/1
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản"
                });
            }

            var customer = await _customerRepository.GetByUserIdAsync(id);

            return Ok(new
            {
                user.UserId,
                user.UserName,
                user.FullName,
                user.Email,
                user.Phone,
                user.Role,
                Address = customer?.Address ?? string.Empty,
                CustomerId = customer?.CustomerId
            });
        }

        // POST: api/User
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.PassWord))
            {
                return BadRequest(new { message = "Tên đăng nhập và mật khẩu không được để trống." });
            }

            var existingUser = await _userRepository.GetByUsernameAsync(dto.UserName.Trim());
            if (existingUser != null)
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại trong hệ thống." });
            }

            var role = dto.Role?.Trim();
            if (role != "Admin" && role != "Staff" && role != "Customer")
            {
                role = "Customer";
            }

            var user = new User
            {
                UserName = dto.UserName.Trim(),
                PassWord = dto.PassWord.Trim(),
                FullName = dto.FullName?.Trim() ?? string.Empty,
                Email = dto.Email?.Trim() ?? string.Empty,
                Phone = dto.Phone?.Trim() ?? string.Empty,
                Role = role
            };

            var userId = await _userRepository.CreateAsync(user);
            user.UserId = userId;

            // Nếu tạo tài khoản là Customer -> tự động tạo hồ sơ khách hàng
            int? customerId = null;
            if (role == "Customer")
            {
                var customer = new Customer
                {
                    UserId = userId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = dto.Address ?? string.Empty
                };
                customerId = await _customerRepository.CreateAsync(customer);
            }

            return Ok(new
            {
                message = "Tạo tài khoản thành công.",
                userId = user.UserId,
                userName = user.UserName,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone,
                role = user.Role,
                customerId = customerId
            });
        }

        // PUT: api/User/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            // Đổi tên đăng nhập nếu có truyền vào
            if (!string.IsNullOrWhiteSpace(dto.UserName) && !string.Equals(dto.UserName.Trim(), user.UserName, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userRepository.GetByUsernameAsync(dto.UserName.Trim());
                if (existingUser != null && existingUser.UserId != id)
                {
                    return BadRequest(new { message = "Tên đăng nhập mới đã tồn tại." });
                }
                user.UserName = dto.UserName.Trim();
            }

            // Cập nhật mật khẩu nếu có
            if (!string.IsNullOrWhiteSpace(dto.PassWord))
            {
                user.PassWord = dto.PassWord.Trim();
            }

            // Cập nhật thông tin liên hệ
            if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName.Trim();
            if (dto.Email != null) user.Email = dto.Email.Trim();
            if (dto.Phone != null) user.Phone = dto.Phone.Trim();

            // Phân quyền vai trò (Role): Admin, Staff, Customer
            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                var targetRole = dto.Role.Trim();
                if (targetRole == "Admin" || targetRole == "Staff" || targetRole == "Customer")
                {
                    // Ngăn chặn hạ quyền của tài khoản admin gốc
                    if (user.UserName.ToLower() == "admin" && targetRole != "Admin")
                    {
                        return BadRequest(new { message = "Không thể hạ quyền tài khoản Quản trị viên gốc (admin)." });
                    }
                    user.Role = targetRole;
                }
            }

            await _userRepository.UpdateAsync(user);

            // Đồng bộ hồ sơ Customer tương ứng
            var customer = await _customerRepository.GetByUserIdAsync(id);
            if (customer != null)
            {
                customer.FullName = user.FullName;
                customer.Email = user.Email;
                customer.Phone = user.Phone;
                if (!string.IsNullOrEmpty(dto.Address)) customer.Address = dto.Address;
                await _customerRepository.UpdateAsync(customer);
            }
            else if (user.Role == "Customer")
            {
                var newCustomer = new Customer
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = dto.Address ?? string.Empty
                };
                await _customerRepository.CreateAsync(newCustomer);
            }

            return Ok(new
            {
                message = "Cập nhật tài khoản thành công.",
                userId = user.UserId,
                userName = user.UserName,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone,
                role = user.Role
            });
        }

        // PUT: api/User/1/role (Phân quyền nhanh vai trò)
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            var targetRole = dto.Role?.Trim();
            if (targetRole != "Admin" && targetRole != "Staff" && targetRole != "Customer")
            {
                return BadRequest(new { message = "Vai trò không hợp lệ. Chỉ chấp nhận Admin, Staff hoặc Customer." });
            }

            if (user.UserName.ToLower() == "admin" && targetRole != "Admin")
            {
                return BadRequest(new { message = "Không thể thay đổi quyền tài khoản Quản trị viên gốc (admin)." });
            }

            user.Role = targetRole;
            await _userRepository.UpdateAsync(user);

            // Đảm bảo có hồ sơ khách hàng nếu chuyển sang Customer
            if (targetRole == "Customer")
            {
                var customer = await _customerRepository.GetByUserIdAsync(id);
                if (customer == null)
                {
                    await _customerRepository.CreateAsync(new Customer
                    {
                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        Address = string.Empty
                    });
                }
            }

            return Ok(new
            {
                message = $"Đã phân quyền thành công cho tài khoản {user.UserName} sang vai trò {targetRole}.",
                userId = user.UserId,
                userName = user.UserName,
                role = user.Role
            });
        }

        // DELETE: api/User/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            if (user.UserName.ToLower() == "admin")
            {
                return BadRequest(new { message = "Không được xóa tài khoản Quản trị viên gốc (admin)." });
            }

            // Kiểm tra ràng buộc đơn booking nếu có hồ sơ Customer
            var customer = await _customerRepository.GetByUserIdAsync(id);
            if (customer != null)
            {
                var bookings = await _bookingRepository.GetAllAsync();
                var hasBookings = bookings.Any(b => b.CustomerId == customer.CustomerId);
                if (hasBookings)
                {
                    return BadRequest(new
                    {
                        message = "Không thể xóa tài khoản này vì đã có lịch sử đặt tour trong hệ thống. Bạn có thể đổi vai trò thay vì xóa."
                    });
                }

                // Xóa hồ sơ Customer trước để tránh lỗi khóa ngoại
                await _customerRepository.DeleteAsync(customer.CustomerId);
            }

            try
            {
                await _userRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Không thể xóa tài khoản do có dữ liệu liên kết khác trong hệ thống." });
            }
        }
    }
}