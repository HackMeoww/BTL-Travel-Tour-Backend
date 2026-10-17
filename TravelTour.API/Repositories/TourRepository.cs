using System.Text;
using Dapper;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public class TourRepository : ITourRepository
    {
        private readonly DapperContext _context;

        public TourRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tour>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Tours";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Tour>(sql);
        }

        public async Task<Tour?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Tours WHERE TourId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Tour>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Tour>> SearchAsync(string? destination, decimal? minPrice, decimal? maxPrice)
        {
            var sb = new StringBuilder("SELECT * FROM Tours WHERE 1=1");
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(destination))
            {
                sb.Append(" AND Destination LIKE @Destination");
                parameters.Add("Destination", $"%{destination}%");
            }

            if (minPrice.HasValue)
            {
                sb.Append(" AND Price >= @MinPrice");
                parameters.Add("MinPrice", minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                sb.Append(" AND Price <= @MaxPrice");
                parameters.Add("MaxPrice", maxPrice.Value);
            }

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Tour>(sb.ToString(), parameters);
        }

        public async Task<int> CreateAsync(Tour tour)
        {
            const string sql = @"
                INSERT INTO Tours (TourName, Destination, Description, Price, Duration, Latitude, Longitude)
                VALUES (@TourName, @Destination, @Description, @Price, @Duration, @Latitude, @Longitude);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, tour);
        }

        public async Task<int> UpdateAsync(Tour tour)
        {
            const string sql = @"
                UPDATE Tours
                SET TourName = @TourName,
                    Destination = @Destination,
                    Description = @Description,
                    Price = @Price,
                    Duration = @Duration,
                    Latitude = @Latitude,
                    Longitude = @Longitude
                WHERE TourId = @TourId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, tour);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Tours WHERE TourId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Tours WHERE TourId = @Id";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
