using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTour.API.Models;
using TravelTour.API.Repositories;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItineraryController : ControllerBase
    {
        private readonly IItineraryRepository _itineraryRepository;

        public ItineraryController(IItineraryRepository itineraryRepository)
        {
            _itineraryRepository = itineraryRepository;
        }

        // GET: api/Itinerary
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Itinerary>>> GetItineraries()
        {
            var itineraries = await _itineraryRepository.GetAllAsync();
            return Ok(itineraries);
        }

        // GET: api/Itinerary/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Itinerary>> GetItinerary(int id)
        {
            var itinerary = await _itineraryRepository.GetByIdAsync(id);

            if (itinerary == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch trình" });
            }

            return Ok(itinerary);
        }

        // GET: api/Itinerary/tour/1
        [HttpGet("tour/{tourId}")]
        public async Task<ActionResult<IEnumerable<Itinerary>>> GetItinerariesByTour(int tourId)
        {
            var itineraries = await _itineraryRepository.GetByTourIdAsync(tourId);
            return Ok(itineraries);
        }

        // POST: api/Itinerary
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Itinerary>> CreateItinerary(Itinerary itinerary)
        {
            var itineraryId = await _itineraryRepository.CreateAsync(itinerary);
            itinerary.ItineraryId = itineraryId;

            return CreatedAtAction(
                nameof(GetItinerary),
                new { id = itinerary.ItineraryId },
                itinerary
            );
        }

        // PUT: api/Itinerary/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItinerary(int id, Itinerary itinerary)
        {
            if (id != itinerary.ItineraryId)
            {
                return BadRequest(new { message = "ItineraryId không khớp" });
            }

            if (!await _itineraryRepository.ExistsAsync(id))
            {
                return NotFound(new { message = "Không tìm thấy lịch trình" });
            }

            await _itineraryRepository.UpdateAsync(itinerary);

            return NoContent();
        }

        // DELETE: api/Itinerary/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItinerary(int id)
        {
            var itinerary = await _itineraryRepository.GetByIdAsync(id);

            if (itinerary == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch trình" });
            }

            await _itineraryRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}