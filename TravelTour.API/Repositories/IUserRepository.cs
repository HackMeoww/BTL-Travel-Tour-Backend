using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByUsernameAndPasswordAsync(string username, string password);
        Task<int> CreateAsync(User user);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
