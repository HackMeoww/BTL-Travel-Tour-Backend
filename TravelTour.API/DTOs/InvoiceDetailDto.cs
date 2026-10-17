namespace TravelTour.API.DTOs
{
    public class InvoiceDetailDto
    {
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;

        // Booking Info
        public int BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal TotalPrice { get; set; }
        public string BookingStatus { get; set; } = string.Empty;

        // Tour Info
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int Duration { get; set; }
        public decimal TourPrice { get; set; }

        // Customer Info
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;

        // Payment Info
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
