using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface ITourRepository
    {
        Task<IEnumerable<Tour>> GetAllAsync();
        Task<Tour?> GetByIdAsync(int id);
        Task<IEnumerable<Tour>> SearchAsync(string? destination, decimal? minPrice, decimal? maxPrice);
        Task<int> CreateAsync(Tour tour);
        Task<int> UpdateAsync(Tour tour);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
