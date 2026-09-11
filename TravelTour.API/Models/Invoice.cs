namespace TravelTour.API.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int BookingId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
    }
}
