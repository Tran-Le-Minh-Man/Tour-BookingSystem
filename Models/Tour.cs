using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Models
{
    /**
     * Tour model class
     */
    public class Tour
    {
        private int tourId;
        private string name;
        private string description;
        private string destination;
        private DateTime? departureDate;
        private int duration; // Số ngày
        private decimal price;
        private int maxParticipants;
        private int currentParticipants;
        private string imageUrl;
        private string status; // ACTIVE, INACTIVE
        
        public Tour() {}
        
        public Tour(int tourId, string name, string description, string destination,
                    DateTime? departureDate, int duration, decimal price,
                    int maxParticipants, int currentParticipants, string imageUrl, string status)
        {
            this.tourId = tourId;
            this.name = name;
            this.description = description;
            this.destination = destination;
            this.departureDate = departureDate;
            this.duration = duration;
            this.price = price;
            this.maxParticipants = maxParticipants;
            this.currentParticipants = currentParticipants;
            this.imageUrl = imageUrl;
            this.status = status;
        }
        
        // Getters and Setters
        public int getTourId() { return tourId; }
        public void setTourId(int tourId) { this.tourId = tourId; }
        
        public string getName() { return name; }
        public void setName(string name) 
        { 
            this.name = (name != null) ? name.Trim() : null; 
        }
        
        public string getDescription() { return description; }
        public void setDescription(string description) 
        { 
            this.description = (description != null) ? description.Trim() : null; 
        }
        
        public string getDestination() { return destination; }
        public void setDestination(string destination) 
        { 
            this.destination = (destination != null) ? destination.Trim() : null; 
        }
        
        public DateTime? getDepartureDate() { return departureDate; }
        public void setDepartureDate(DateTime? departureDate) { this.departureDate = departureDate; }
        
        public int getDuration() { return duration; }
        public void setDuration(int duration) { this.duration = duration; }
        
        public decimal getPrice() { return price; }
        public void setPrice(decimal price) { this.price = price; }
        
        public int getMaxParticipants() { return maxParticipants; }
        public void setMaxParticipants(int maxParticipants) { this.maxParticipants = maxParticipants; }
        
        public int getCurrentParticipants() { return currentParticipants; }
        public void setCurrentParticipants(int currentParticipants) { this.currentParticipants = currentParticipants; }
        
        public string getImageUrl() { return imageUrl; }
        public void setImageUrl(string imageUrl) 
        { 
            this.imageUrl = (imageUrl != null) ? imageUrl.Trim() : null; 
        }
        
        public string getStatus() { return status; }
        public void setStatus(string status) 
        { 
            this.status = (status != null) ? status.Trim() : "ACTIVE"; 
        }
        
        // Helper methods
        public bool isActive()
        {
            return "ACTIVE".Equals(status, StringComparison.OrdinalIgnoreCase);
        }
        
        public int getAvailableSlots()
        {
            return Math.Max(0, maxParticipants - currentParticipants);
        }
        
        public string getFormattedPrice()
        {
            if (price == 0) return "0đ";
            return String.Format("{0:N0}đ", price);
        }
        
        public string getFormattedDepartureDate()
        {
            if (departureDate == null) return "";
            return departureDate.Value.ToString("dd/MM/yyyy");
        }
        
        public override bool Equals(object? o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            Tour tour = (Tour)o;
            return tourId == tour.tourId;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(tourId);
        }
        
        public override string ToString()
        {
            return "Tour{" +
                    "tourId=" + tourId +
                    ", name='" + name + '\'' +
                    ", destination='" + destination + '\'' +
                    ", price=" + price +
                    ", status='" + status + '\'' +
                    '}';
        }
    }
}
