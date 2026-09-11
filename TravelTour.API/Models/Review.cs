namespace TravelTour.API.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int TourId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
