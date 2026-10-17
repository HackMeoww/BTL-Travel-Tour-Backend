using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByBookingIdAsync(int bookingId);
        Task<int> CreateAsync(Payment payment);
        Task<int> CreatePaymentTransactionAsync(Payment payment, Invoice invoice, int bookingId, string newBookingStatus);
        Task<int> UpdateAsync(Payment payment);
        Task<int> UpdateStatusAsync(int id, string status);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<decimal> GetTotalRevenueAsync();
    }
}
