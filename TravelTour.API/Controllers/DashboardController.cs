using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelTour.API.Data;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var totalCustomers = await _context.Customers.CountAsync();

            var totalBookings = await _context.Bookings.CountAsync();

            var totalRevenue = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            return Ok(new
            {
                totalCustomers,
                totalBookings,
                totalRevenue
            });
        }
    }
}