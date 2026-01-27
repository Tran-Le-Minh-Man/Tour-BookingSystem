using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Models
{
    /**
     * Tour model class
     */
    [Table("tours")]
    public class Tour
    {
        [Key]
        [Column("id")]
        public int TourId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("destination")]
        public string? Destination { get; set; }

        [Column("departure_date")]
        public DateTime? DepartureDate { get; set; }

        [Column("duration")]
        public int Duration { get; set; } // Số ngày

        [Column("price")]
        public decimal Price { get; set; }

        [Column("max_participants")]
        public int MaxParticipants { get; set; }

        [Column("current_participants")]
        public int CurrentParticipants { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("status")]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, INACTIVE

        public Tour() { }

        public Tour(int tourId, string name, string description, string destination,
                    DateTime? departureDate, int duration, decimal price,
                    int maxParticipants, int currentParticipants, string imageUrl, string status)
        {
            this.TourId = tourId;
            this.Name = name;
            this.Description = description;
            this.Destination = destination;
            this.DepartureDate = departureDate;
            this.Duration = duration;
            this.Price = price;
            this.MaxParticipants = maxParticipants;
            this.CurrentParticipants = currentParticipants;
            this.ImageUrl = imageUrl;
            this.Status = status;
        }

        // Legacy Getters and Setters for compatibility
        public int getTourId() { return TourId; }
        public void setTourId(int tourId) { this.TourId = tourId; }

        public string? getName() { return Name; }
        public void setName(string? name)
        {
            this.Name = (name != null) ? name.Trim() : null;
        }

        public string? getDescription() { return Description; }
        public void setDescription(string? description)
        {
            this.Description = (description != null) ? description.Trim() : null;
        }

        public string? getDestination() { return Destination; }
        public void setDestination(string? destination)
        {
            this.Destination = (destination != null) ? destination.Trim() : null;
        }

        public DateTime? getDepartureDate() { return DepartureDate; }
        public void setDepartureDate(DateTime? departureDate) { this.DepartureDate = departureDate; }

        public int getDuration() { return Duration; }
        public void setDuration(int duration) { this.Duration = duration; }

        public decimal getPrice() { return Price; }
        public void setPrice(decimal price) { this.Price = price; }

        public int getMaxParticipants() { return MaxParticipants; }
        public void setMaxParticipants(int maxParticipants) { this.MaxParticipants = maxParticipants; }

        public int getCurrentParticipants() { return CurrentParticipants; }
        public void setCurrentParticipants(int currentParticipants) { this.CurrentParticipants = currentParticipants; }

        public string? getImageUrl() { return ImageUrl; }
        public void setImageUrl(string? imageUrl)
        {
            this.ImageUrl = (imageUrl != null) ? imageUrl.Trim() : null;
        }

        public string getStatus() { return Status; }
        public void setStatus(string? status)
        {
            this.Status = (status != null) ? status.Trim() : "ACTIVE";
        }

        // Helper methods
        public bool isActive()
        {
            return "ACTIVE".Equals(Status, StringComparison.OrdinalIgnoreCase);
        }

        public int getAvailableSlots()
        {
            return Math.Max(0, MaxParticipants - CurrentParticipants);
        }

        public string getFormattedPrice()
        {
            if (Price == 0) return "0đ";
            return String.Format("{0:N0}đ", Price);
        }

        public string getSafeImageUrl()
        {
            if (string.IsNullOrEmpty(ImageUrl))
            {
                return "https://images.unsplash.com/photo-1502602898657-3e917247a183?q=80&w=2070&auto=format&fit=crop";
            }

            // If it's a full URL or a relative path starting with / or ~/ or images/
            if (ImageUrl.StartsWith("http") || ImageUrl.StartsWith("/") || ImageUrl.StartsWith("~/") || ImageUrl.ToLower().Contains("images/"))
            {
                // If it's relative but doesn't start with /, prepend it (except if encoded/etc)
                if (!ImageUrl.StartsWith("http") && !ImageUrl.StartsWith("/") && !ImageUrl.StartsWith("~/"))
                {
                    return "/" + ImageUrl;
                }
                return ImageUrl;
            }

            return "https://images.unsplash.com/photo-1502602898657-3e917247a183?q=80&w=2070&auto=format&fit=crop";
        }

        public string getFormattedDepartureDate()
        {
            if (DepartureDate == null) return "";
            return DepartureDate.Value.ToString("dd/MM/yyyy");
        }

        public override bool Equals(object? o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            Tour tour = (Tour)o;
            return TourId == tour.TourId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TourId);
        }

        public override string ToString()
        {
            return "Tour{" +
                    "tourId=" + TourId +
                    ", name='" + Name + '\'' +
                    ", destination='" + Destination + '\'' +
                    ", price=" + Price +
                    ", status='" + Status + '\'' +
                    '}';
        }
    }
}
