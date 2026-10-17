using System.Data;
using Microsoft.Data.SqlClient;

namespace TravelTour.API.Data
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not found in configuration.");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
