using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByUserIdAsync(int userId);
        Task<int> CreateAsync(Customer customer);
        Task<int> UpdateAsync(Customer customer);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
    }
}
