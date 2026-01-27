namespace TourBookingSystem.Models
{
    public class CheckoutViewModel
    {
        public int BookingId { get; set; }
        public int TourId { get; set; }

        public string TourName { get; set; }
        public string Destination { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }

        public int Quantity { get; set; }
        public decimal Total { get; set; }

        // User info
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string Note { get; set; }

        // Payment
        public string PaymentMethod { get; set; }
        public string PaymentProvider { get; set; }
    }
}
