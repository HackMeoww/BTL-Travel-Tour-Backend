using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface IItineraryRepository
    {
        Task<IEnumerable<Itinerary>> GetAllAsync();
        Task<Itinerary?> GetByIdAsync(int id);
        Task<IEnumerable<Itinerary>> GetByTourIdAsync(int tourId);
        Task<int> CreateAsync(Itinerary itinerary);
        Task<int> UpdateAsync(Itinerary itinerary);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
