using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITourRepository _tourRepository;
        private readonly IUserRepository _userRepository;

        public DashboardController(
            ICustomerRepository customerRepository,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            ITourRepository tourRepository,
            IUserRepository userRepository)
        {
            _customerRepository = customerRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _tourRepository = tourRepository;
            _userRepository = userRepository;
        }

        // GET: api/Dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var totalCustomers = await _customerRepository.CountAsync();
            var totalBookings = await _bookingRepository.CountAsync();
            var totalRevenue = await _paymentRepository.GetTotalRevenueAsync();

            var tours = await _tourRepository.GetAllAsync();
            var totalTours = tours.Count();

            var users = await _userRepository.GetAllAsync();
            var totalUsers = users.Count();

            return Ok(new
            {
                totalCustomers,
                totalBookings,
                totalRevenue,
                totalTours,
                totalUsers
            });
        }
    }
}