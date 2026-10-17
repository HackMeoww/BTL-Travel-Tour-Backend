using Dapper;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DapperContext _context;

        public CustomerRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Customers";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Customer>(sql);
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Customers WHERE CustomerId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
        }

        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            const string sql = "SELECT * FROM Customers WHERE UserId = @UserId";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { UserId = userId });
        }

        public async Task<int> CreateAsync(Customer customer)
        {
            const string sql = @"
                INSERT INTO Customers (UserId, FullName, Phone, Email, Address)
                VALUES (@UserId, @FullName, @Phone, @Email, @Address);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, customer);
        }

        public async Task<int> UpdateAsync(Customer customer)
        {
            const string sql = @"
                UPDATE Customers
                SET UserId = @UserId,
                    FullName = @FullName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address
                WHERE CustomerId = @CustomerId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, customer);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Customers WHERE CustomerId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Customers WHERE CustomerId = @Id";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            const string sql = "SELECT COUNT(1) FROM Customers";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql);
        }
    }
}
