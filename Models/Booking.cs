using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Models
{
    /**
     * Booking model class
     */
    public class Booking
    {
        private int bookingId;
        private int userId;
        private int tourId;
        private DateTime? bookingDate;
        private string status; // PENDING, CONFIRMED, CANCELLED, COMPLETED
        private int numParticipants;
        private decimal totalPrice;
        private string notes;
        
        // Additional fields for display
        private string userName;
        private string userEmail;
        private string tourName;
        private string tourDestination;
        private string tourImage;
        private string tourDeparture;
        private int tourDuration;
        private decimal tourPrice;
        
        public Booking() {}
        
        public Booking(int bookingId, int userId, int tourId, DateTime? bookingDate,
                       string status, int numParticipants, decimal totalPrice, string notes)
        {
            this.bookingId = bookingId;
            this.userId = userId;
            this.tourId = tourId;
            this.bookingDate = bookingDate;
            this.status = status;
            this.numParticipants = numParticipants;
            this.totalPrice = totalPrice;
            this.notes = notes;
        }
        
        // Getters and Setters
        public int getBookingId() { return bookingId; }
        public void setBookingId(int bookingId) { this.bookingId = bookingId; }
        
        public int getUserId() { return userId; }
        public void setUserId(int userId) { this.userId = userId; }
        
        public int getTourId() { return tourId; }
        public void setTourId(int tourId) { this.tourId = tourId; }
        
        public DateTime? getBookingDate() { return bookingDate; }
        public void setBookingDate(DateTime? bookingDate) { this.bookingDate = bookingDate; }
        
        public string getStatus() { return status; }
        public void setStatus(string status) 
        { 
            this.status = (status != null) ? status.Trim().ToUpper() : "PENDING"; 
        }
        
        public int getNumParticipants() { return numParticipants; }
        public void setNumParticipants(int numParticipants) { this.numParticipants = numParticipants; }
        
        public decimal getTotalPrice() { return totalPrice; }
        public void setTotalPrice(decimal totalPrice) { this.totalPrice = totalPrice; }
        
        public string getNotes() { return notes; }
        public void setNotes(string notes) 
        { 
            this.notes = (notes != null) ? notes.Trim() : null; 
        }
        
        // Display fields
        public string getUserName() { return userName; }
        public void setUserName(string userName) { this.userName = userName; }
        
        public string getUserEmail() { return userEmail; }
        public void setUserEmail(string userEmail) { this.userEmail = userEmail; }
        
        public string getTourName() { return tourName; }
        public void setTourName(string tourName) { this.tourName = tourName; }
        
        public string getTourDestination() { return tourDestination; }
        public void setTourDestination(string tourDestination) { this.tourDestination = tourDestination; }
        
        public string getTourImage() { return tourImage; }
        public void setTourImage(string tourImage) { this.tourImage = tourImage; }
        
        public string getTourDeparture() { return tourDeparture; }
        public void setTourDeparture(string tourDeparture) { this.tourDeparture = tourDeparture; }
        
        public int getTourDuration() { return tourDuration; }
        public void setTourDuration(int tourDuration) { this.tourDuration = tourDuration; }
        
        public decimal getTourPrice() { return tourPrice; }
        public void setTourPrice(decimal tourPrice) { this.tourPrice = tourPrice; }
        
        // Helper methods
        public bool isPending()
        {
            return "PENDING".Equals(status, StringComparison.OrdinalIgnoreCase);
        }
        
        public bool isConfirmed()
        {
            return "CONFIRMED".Equals(status, StringComparison.OrdinalIgnoreCase);
        }
        
        public bool isCancelled()
        {
            return "CANCELLED".Equals(status, StringComparison.OrdinalIgnoreCase);
        }
        
        public bool isCompleted()
        {
            return "COMPLETED".Equals(status, StringComparison.OrdinalIgnoreCase);
        }
        
        public string getFormattedPrice()
        {
            if (totalPrice == 0) return "0đ";
            return String.Format("{0:N0}đ", totalPrice);
        }
        
        public string getFormattedDate()
        {
            if (bookingDate == null) return "";
            return bookingDate.Value.ToString("dd/MM/yyyy HH:mm");
        }
        
        public string getFormattedStatus()
        {
            switch (status)
            {
                case "PENDING": return "Chờ xác nhận";
                case "CONFIRMED": return "Đã xác nhận";
                case "CANCELLED": return "Đã hủy";
                case "COMPLETED": return "Hoàn thành";
                default: return status;
            }
        }
        
        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            Booking booking = (Booking)o;
            return bookingId == booking.bookingId;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(bookingId);
        }
    }
}
