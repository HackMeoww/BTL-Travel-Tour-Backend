using Dapper;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DapperContext _context;

        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Users";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<User>(sql);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Users WHERE UserId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = "SELECT * FROM Users WHERE UserName = @Username";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = "SELECT * FROM Users WHERE LOWER(Email) = LOWER(@Email)";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email.Trim() });
        }

        public async Task<User?> GetByUsernameAndPasswordAsync(string username, string password)
        {
            const string sql = "SELECT * FROM Users WHERE UserName = @Username AND PassWord = @Password";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username, Password = password });
        }

        public async Task<int> CreateAsync(User user)
        {
            const string sql = @"
                INSERT INTO Users (UserName, PassWord, FullName, Email, Phone, Role)
                VALUES (@UserName, @PassWord, @FullName, @Email, @Phone, @Role);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, user);
        }

        public async Task<int> UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE Users
                SET UserName = @UserName,
                    PassWord = @PassWord,
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    Role = @Role
                WHERE UserId = @UserId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, user);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Users WHERE UserId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE UserId = @Id";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
