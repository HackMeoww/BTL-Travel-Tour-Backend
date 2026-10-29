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
            const string sql = "SELECT * FROM Reviews ORDER BY CreatedAt DESC";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Review>(sql);
        }

        public async Task<IEnumerable<ReviewDetailDto>> GetAllDetailsAsync()
        {
            const string sql = @"
                SELECT 
                    r.ReviewId,
                    r.TourId,
                    ISNULL(t.TourName, N'Tour không xác định') AS TourName,
                    ISNULL(t.Destination, N'Điểm đến') AS Destination,
                    ISNULL(t.Price, 0) AS TourPrice,
                    r.CustomerId,
                    ISNULL(c.FullName, N'Khách vãng lai') AS CustomerName,
                    ISNULL(c.Phone, N'') AS CustomerPhone,
                    ISNULL(c.Email, N'') AS CustomerEmail,
                    r.Rating,
                    ISNULL(r.Comment, N'') AS Comment,
                    r.CreatedAt
                FROM Reviews r
                LEFT JOIN Tours t ON r.TourId = t.TourId
                LEFT JOIN Customers c ON r.CustomerId = c.CustomerId
                ORDER BY r.CreatedAt DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<ReviewDetailDto>(sql);
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Reviews WHERE ReviewId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Review>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Review>> GetByTourIdAsync(int tourId)
        {
            const string sql = "SELECT * FROM Reviews WHERE TourId = @TourId ORDER BY CreatedAt DESC";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Review>(sql, new { TourId = tourId });
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
