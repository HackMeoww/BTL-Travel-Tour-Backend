using Dapper;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly DapperContext _context;

        public PaymentRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Payments";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Payment>(sql);
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Payments WHERE PaymentId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { Id = id });
        }

        public async Task<Payment?> GetByBookingIdAsync(int bookingId)
        {
            const string sql = "SELECT * FROM Payments WHERE BookingId = @BookingId";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { BookingId = bookingId });
        }

        public async Task<int> CreateAsync(Payment payment)
        {
            const string sql = @"
                INSERT INTO Payments (BookingId, PaymentDate, Amount, PaymentMethod, Status)
                VALUES (@BookingId, @PaymentDate, @Amount, @PaymentMethod, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, payment);
        }

        public async Task<int> CreatePaymentTransactionAsync(Payment payment, Invoice invoice, int bookingId, string newBookingStatus)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateBookingSql = "UPDATE Bookings SET Status = @Status WHERE BookingId = @BookingId";
                await connection.ExecuteAsync(updateBookingSql, new { Status = newBookingStatus, BookingId = bookingId }, transaction);

                const string insertPaymentSql = @"
                    INSERT INTO Payments (BookingId, PaymentDate, Amount, PaymentMethod, Status)
                    VALUES (@BookingId, @PaymentDate, @Amount, @PaymentMethod, @Status);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                var paymentId = await connection.QuerySingleAsync<int>(insertPaymentSql, payment, transaction);
                payment.PaymentId = paymentId;

                const string insertInvoiceSql = @"
                    INSERT INTO Invoices (BookingId, InvoiceDate, Amount, PaymentStatus)
                    VALUES (@BookingId, @InvoiceDate, @Amount, @PaymentStatus);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                var invoiceId = await connection.QuerySingleAsync<int>(insertInvoiceSql, invoice, transaction);
                invoice.InvoiceId = invoiceId;

                transaction.Commit();
                return paymentId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateAsync(Payment payment)
        {
            const string sql = @"
                UPDATE Payments
                SET BookingId = @BookingId,
                    PaymentDate = @PaymentDate,
                    Amount = @Amount,
                    PaymentMethod = @PaymentMethod,
                    Status = @Status
                WHERE PaymentId = @PaymentId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, payment);
        }

        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            const string sql = "UPDATE Payments SET Status = @Status WHERE PaymentId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id, Status = status });
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Payments WHERE PaymentId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Payments WHERE PaymentId = @Id";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            const string sql = "SELECT ISNULL(SUM(Amount), 0) FROM Payments WHERE Status = 'Paid'";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<decimal>(sql);
        }
    }
}
