using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);
        Task<int> CreateAsync(Booking booking);
        Task<int> UpdateAsync(Booking booking);
        Task<int> UpdateStatusAsync(int id, string status);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
    }
}
