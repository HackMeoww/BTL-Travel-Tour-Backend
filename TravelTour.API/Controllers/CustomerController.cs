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

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
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
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            var customerId = await _customerRepository.CreateAsync(customer);
            customer.CustomerId = customerId;

            return CreatedAtAction(
                nameof(GetCustomer),
                new { id = customer.CustomerId },
                customer
            );
        }

        // PUT: api/Customer/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest(new { message = "CustomerId không khớp" });
            }

            if (!await _customerRepository.ExistsAsync(id))
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            await _customerRepository.UpdateAsync(customer);

            return NoContent();
        }

        // DELETE: api/Customer/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            await _customerRepository.DeleteAsync(id);

            return NoContent();
        }

        // GET: api/Customer/count
        [HttpGet("count")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetCustomerCount()
        {
            var totalCustomers = await _customerRepository.CountAsync();

            return Ok(new
            {
                message = "Thống kê số lượng khách hàng thành công",
                totalCustomers = totalCustomers
            });
        }
    }
}