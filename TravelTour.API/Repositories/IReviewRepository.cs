using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<IEnumerable<ReviewDetailDto>> GetAllDetailsAsync();
        Task<Review?> GetByIdAsync(int id);
        Task<IEnumerable<Review>> GetByTourIdAsync(int tourId);
        Task<int> CreateAsync(Review review);
        Task<int> UpdateAsync(Review review);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
