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
    }
}