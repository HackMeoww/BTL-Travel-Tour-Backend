using Dapper;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public class ItineraryRepository : IItineraryRepository
    {
        private readonly DapperContext _context;

        public ItineraryRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Itinerary>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Itineraries";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Itinerary>(sql);
        }

        public async Task<Itinerary?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Itineraries WHERE ItineraryId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Itinerary>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Itinerary>> GetByTourIdAsync(int tourId)
        {
            const string sql = "SELECT * FROM Itineraries WHERE TourId = @TourId ORDER BY DayNumber ASC";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Itinerary>(sql, new { TourId = tourId });
        }

        public async Task<int> CreateAsync(Itinerary itinerary)
        {
            const string sql = @"
                INSERT INTO Itineraries (TourId, DayNumber, Description)
                VALUES (@TourId, @DayNumber, @Description);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, itinerary);
        }

        public async Task<int> UpdateAsync(Itinerary itinerary)
        {
            const string sql = @"
                UPDATE Itineraries
                SET TourId = @TourId,
                    DayNumber = @DayNumber,
                    Description = @Description
                WHERE ItineraryId = @ItineraryId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, itinerary);
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Itineraries WHERE ItineraryId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Itineraries WHERE ItineraryId = @Id";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
