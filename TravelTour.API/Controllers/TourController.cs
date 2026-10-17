using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly ITourRepository _tourRepository;

        public TourController(ITourRepository tourRepository)
        {
            _tourRepository = tourRepository;
        }

        // GET: api/Tour
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetTours()
        {
            var tours = await _tourRepository.GetAllAsync();
            return Ok(tours);
        }

        // GET: api/Tour/search?destination=Quang Ninh&minPrice=1000000&maxPrice=5000000
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Tour>>> SearchTours(
            string? destination,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var tours = await _tourRepository.SearchAsync(destination, minPrice, maxPrice);
            return Ok(tours);
        }

        // GET: api/Tour/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Tour>> GetTour(int id)
        {
            var tour = await _tourRepository.GetByIdAsync(id);

            if (tour == null)
            {
                return NotFound(new { message = "Không tìm thấy tour" });
            }

            return Ok(tour);
        }

        // POST: api/Tour
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Tour>> CreateTour(Tour tour)
        {
            var tourId = await _tourRepository.CreateAsync(tour);
            tour.TourId = tourId;

            return CreatedAtAction(
                nameof(GetTour),
                new { id = tour.TourId },
                tour
            );
        }

        // PUT: api/Tour/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTour(int id, Tour tour)
        {
            if (id != tour.TourId)
            {
                return BadRequest(new { message = "TourId không khớp" });
            }

            if (!await _tourRepository.ExistsAsync(id))
            {
                return NotFound(new { message = "Không tìm thấy tour" });
            }

            await _tourRepository.UpdateAsync(tour);

            return NoContent();
        }

        // DELETE: api/Tour/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var tour = await _tourRepository.GetByIdAsync(id);

            if (tour == null)
            {
                return NotFound(new { message = "Không tìm thấy tour" });
            }

            await _tourRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}