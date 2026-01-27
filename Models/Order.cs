namespace TourBookingSystem.Models
{
    public class Order
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int booking_id { get; set; }
        public int tour_id { get; set; }
        public int quantity { get; set; }
        public decimal total_price { get; set; }
        public string note { get; set; }
        public string payment_method { get; set; }
        public string payment_provider { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }

    }
}
