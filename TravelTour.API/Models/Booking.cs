namespace TravelTour.API.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int TourId { get; set; }
        public int CustomerId { get; set; }
        public DateTime BookingDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
