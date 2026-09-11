namespace TravelTour.API.Models
{
    public class Itinerary
    {
        public int ItineraryId { get; set; }
        public int TourId { get; set; }
        public int DayNumber { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
