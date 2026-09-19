using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItineraryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItineraryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Itinerary
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Itinerary>>> GetItineraries()
        {
            return await _context.Itineraries.ToListAsync();
        }

        // GET: api/Itinerary/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Itinerary>> GetItinerary(int id)
        {
            var itinerary = await _context.Itineraries.FindAsync(id);

            if (itinerary == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch trình" });
            }

            return itinerary;
        }

        // GET: api/Itinerary/tour/1
        [HttpGet("tour/{tourId}")]
        public async Task<ActionResult<IEnumerable<Itinerary>>> GetItinerariesByTour(int tourId)
        {
            return await _context.Itineraries
                .Where(i => i.TourId == tourId)
                .OrderBy(i => i.DayNumber)
                .ToListAsync();
        }

        // POST: api/Itinerary
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Itinerary>> CreateItinerary(Itinerary itinerary)
        {
            _context.Itineraries.Add(itinerary);
            await _context.SaveChangesAsync();

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

            _context.Entry(itinerary).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItineraryExists(id))
                {
                    return NotFound(new { message = "Không tìm thấy lịch trình" });
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/Itinerary/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItinerary(int id)
        {
            var itinerary = await _context.Itineraries.FindAsync(id);

            if (itinerary == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch trình" });
            }

            _context.Itineraries.Remove(itinerary);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItineraryExists(int id)
        {
            return _context.Itineraries.Any(e => e.ItineraryId == id);
        }
    }
}