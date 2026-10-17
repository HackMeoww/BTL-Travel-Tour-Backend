using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review?> GetByIdAsync(int id);
        Task<int> CreateAsync(Review review);
        Task<int> UpdateAsync(Review review);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
