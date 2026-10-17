using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;

        public DashboardController(
            ICustomerRepository customerRepository,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository)
        {
            _customerRepository = customerRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
        }

        // GET: api/Dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var totalCustomers = await _customerRepository.CountAsync();
            var totalBookings = await _bookingRepository.CountAsync();
            var totalRevenue = await _paymentRepository.GetTotalRevenueAsync();

            return Ok(new
            {
                totalCustomers,
                totalBookings,
                totalRevenue
            });
        }
    }
}