using Dapper;
using TravelTour.API.Data;
using TravelTour.API.Models;

namespace TravelTour.API.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DapperContext _context;

        public BookingRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Bookings";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Booking>(sql);
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Bookings WHERE BookingId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(Booking booking)
        {
            const string sql = @"
                INSERT INTO Bookings (TourId, CustomerId, BookingDate, DepartureDate, NumberOfPeople, TotalPrice, Status)
                VALUES (@TourId, @CustomerId, @BookingDate, @DepartureDate, @NumberOfPeople, @TotalPrice, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, booking);
        }

        public async Task<int> UpdateAsync(Booking booking)
        {
            const string sql = @"
                UPDATE Bookings
                SET TourId = @TourId,
                    CustomerId = @CustomerId,
                    BookingDate = @BookingDate,
                    DepartureDate = @DepartureDate,
                    NumberOfPeople = @NumberOfPeople,
                    TotalPrice = @TotalPrice,
                    Status = @Status
                WHERE BookingId = @BookingId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, booking);
        }

        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            const string sql = "UPDATE Bookings SET Status = @Status WHERE BookingId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id, Status = status });
        }

        public async Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Bookings WHERE BookingId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Bookings WHERE BookingId = @Id";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            const string sql = "SELECT COUNT(1) FROM Bookings";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql);
        }
    }
}
