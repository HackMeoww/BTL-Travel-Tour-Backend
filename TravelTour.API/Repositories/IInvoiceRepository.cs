using TravelTour.API.DTOs;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetByBookingIdAsync(int bookingId);
        Task<InvoiceDetailDto?> GetDetailByIdAsync(int id);
        Task<InvoiceDetailDto?> GetDetailByBookingIdAsync(int bookingId);
        Task<int> CreateAsync(Invoice invoice);
        Task<int> UpdateAsync(Invoice invoice);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
