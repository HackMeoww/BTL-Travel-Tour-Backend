namespace TravelTour.API.Models
{
    public class Tour
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
