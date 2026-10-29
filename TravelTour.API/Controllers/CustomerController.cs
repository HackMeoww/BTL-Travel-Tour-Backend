using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;

        public CustomerController(
            ICustomerRepository customerRepository,
            IUserRepository userRepository)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
        }

        // GET: api/Customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _customerRepository.GetAllAsync();
            return Ok(customers);
        }

        // GET: api/Customer/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            return Ok(customer);
        }

        // GET: api/Customer/user/1
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Customer>> GetCustomerByUserId(int userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ khách hàng cho tài khoản này" });
            }

            return Ok(customer);
        }

        // POST: api/Customer
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
        {
            if (customer.UserId <= 0)
            {
                return BadRequest(new
                {
                    message = "UserId không hợp lệ! Hồ sơ khách hàng bắt buộc phải liên kết với một tài khoản UserId đã tồn tại trong hệ thống."
                });
            }

            var userExists = await _userRepository.ExistsAsync(customer.UserId);
            if (!userExists)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy tài khoản người dùng với UserId = {customer.UserId}. Vui lòng tạo tài khoản User trước khi tạo hồ sơ khách hàng!"
                });
            }

            // Kiểm tra xem User này đã có hồ sơ Customer chưa để tránh trùng lặp
            var existingCustomer = await _customerRepository.GetByUserIdAsync(customer.UserId);
            if (existingCustomer != null)
            {
                return BadRequest(new
                {
                    message = $"Tài khoản UserId = {customer.UserId} này đã có hồ sơ khách hàng trong hệ thống (CustomerId = {existingCustomer.CustomerId}).",
                    customerId = existingCustomer.CustomerId
                });
            }

            if (string.IsNullOrWhiteSpace(customer.FullName))
            {
                var user = await _userRepository.GetByIdAsync(customer.UserId);
                if (user != null && !string.IsNullOrWhiteSpace(user.FullName))
                {
                    customer.FullName = user.FullName;
                }
            }

            var customerId = await _customerRepository.CreateAsync(customer);
            customer.CustomerId = customerId;

            return CreatedAtAction(
                nameof(GetCustomer),
                new { id = customer.CustomerId },
                customer
            );
        }
    }
}