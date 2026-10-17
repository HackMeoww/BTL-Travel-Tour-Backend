using Dapper;
using TravelTour.API.DTOs;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly DapperContext _context;

        public InvoiceRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Invoices";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Invoice>(sql);
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Invoices WHERE InvoiceId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Invoice>(sql, new { Id = id });
        }

        public async Task<InvoiceDetailDto?> GetDetailByIdAsync(int id)
        {
            const string sql = @"
                SELECT 
                    i.InvoiceId,
                    i.InvoiceDate,
                    i.Amount,
                    i.PaymentStatus,
                    b.BookingId,
                    b.BookingDate,
                    b.DepartureDate,
                    b.NumberOfPeople,
                    b.TotalPrice,
                    b.Status AS BookingStatus,
                    ISNULL(t.TourId, 0) AS TourId,
                    ISNULL(t.TourName, N'Tour Du Lịch') AS TourName,
                    ISNULL(t.Destination, N'') AS Destination,
                    ISNULL(t.Duration, 1) AS Duration,
                    ISNULL(t.Price, 0) AS TourPrice,
                    ISNULL(c.CustomerId, 0) AS CustomerId,
                    ISNULL(c.FullName, N'Khách hàng') AS CustomerName,
                    ISNULL(c.Phone, N'') AS CustomerPhone,
                    ISNULL(c.Email, N'') AS CustomerEmail,
                    ISNULL(c.Address, N'') AS CustomerAddress,
                    p.PaymentMethod,
                    p.PaymentDate
                FROM Invoices i
                INNER JOIN Bookings b ON i.BookingId = b.BookingId
                LEFT JOIN Tours t ON b.TourId = t.TourId
                LEFT JOIN Customers c ON b.CustomerId = c.CustomerId
                LEFT JOIN Payments p ON b.BookingId = p.BookingId
                WHERE i.InvoiceId = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<InvoiceDetailDto>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(Invoice invoice)
        {
            const string sql = @"
                INSERT INTO Invoices (BookingId, InvoiceDate, Amount, PaymentStatus)
                VALUES (@BookingId, @InvoiceDate, @Amount, @PaymentStatus);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, invoice);
        }

        public async Task<int> UpdateAsync(Invoice invoice)
        {
            const string sql = @"
                UPDATE Invoices
                SET BookingId = @BookingId,
                    InvoiceDate = @InvoiceDate,
                    Amount = @Amount,
                    PaymentStatus = @PaymentStatus
                WHERE InvoiceId = @InvoiceId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, invoice);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Invoices WHERE InvoiceId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Invoices WHERE InvoiceId = @Id";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
