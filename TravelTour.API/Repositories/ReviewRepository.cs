using Dapper;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DapperContext _context;

        public ReviewRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Reviews";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Review>(sql);
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Reviews WHERE ReviewId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Review>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(Review review)
        {
            const string sql = @"
                INSERT INTO Reviews (TourId, CustomerId, Rating, Comment, CreatedAt)
                VALUES (@TourId, @CustomerId, @Rating, @Comment, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, review);
        }

        public async Task<int> UpdateAsync(Review review)
        {
            const string sql = @"
                UPDATE Reviews
                SET TourId = @TourId,
                    CustomerId = @CustomerId,
                    Rating = @Rating,
                    Comment = @Comment
                WHERE ReviewId = @ReviewId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, review);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Reviews WHERE ReviewId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Reviews WHERE ReviewId = @Id";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
