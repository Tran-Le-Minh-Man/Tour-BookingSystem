using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Models
{
    /**
     * Booking model class
     */
    [Table("bookings")]
    public class Booking
    {
        [Key]
        [Column("id")]
        public int BookingId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("tour_id")]
        public int TourId { get; set; }

        [Column("booking_date")]
        public DateTime? BookingDate { get; set; }

        [Column("status")]
        public string Status { get; set; } = "PENDING"; // PENDING, CONFIRMED, CANCELLED, COMPLETED

        [Column("num_participants")]
        public int NumParticipants { get; set; }

        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        // Additional fields for display (marked as NotMapped for EF Core)
        [NotMapped]
        public string? UserName { get; set; }
        [NotMapped]
        public string? UserEmail { get; set; }
        [NotMapped]
        public string? TourName { get; set; }
        [NotMapped]
        public string? TourDestination { get; set; }
        [NotMapped]
        public string? TourImage { get; set; }
        [NotMapped]
        public string? TourDeparture { get; set; }
        [NotMapped]
        public int TourDuration { get; set; }
        [NotMapped]
        public decimal TourPrice { get; set; }
        [NotMapped]
        public bool isPaid { get; set; }


        public Booking() { }

        public Booking(int bookingId, int userId, int tourId, DateTime? bookingDate,
                       string status, int numParticipants, decimal totalPrice, string notes)
        {
            this.BookingId = bookingId;
            this.UserId = userId;
            this.TourId = tourId;
            this.BookingDate = bookingDate;
            this.Status = status;
            this.NumParticipants = numParticipants;
            this.TotalPrice = totalPrice;
            this.Notes = notes;
        }

        // Legacy Getters and Setters
        public int getBookingId() { return BookingId; }
        public void setBookingId(int bookingId) { this.BookingId = bookingId; }

        public int getUserId() { return UserId; }
        public void setUserId(int userId) { this.UserId = userId; }

        public int getTourId() { return TourId; }
        public void setTourId(int tourId) { this.TourId = tourId; }

        public DateTime? getBookingDate() { return BookingDate; }
        public void setBookingDate(DateTime? bookingDate) { this.BookingDate = bookingDate; }

        public string getStatus() { return Status; }
        public void setStatus(string? status)
        {
            this.Status = (status != null) ? status.Trim().ToUpper() : "PENDING";
        }

        public int getNumParticipants() { return NumParticipants; }
        public void setNumParticipants(int numParticipants) { this.NumParticipants = numParticipants; }

        public decimal getTotalPrice() { return TotalPrice; }
        public void setTotalPrice(decimal totalPrice) { this.TotalPrice = totalPrice; }

        public string? getNotes() { return Notes; }
        public void setNotes(string? notes)
        {
            this.Notes = (notes != null) ? notes.Trim() : null;
        }

        // Display fields
        public string? getUserName() { return UserName; }
        public void setUserName(string? userName) { this.UserName = userName; }

        public string? getUserEmail() { return UserEmail; }
        public void setUserEmail(string? userEmail) { this.UserEmail = userEmail; }

        public string? getTourName() { return TourName; }
        public void setTourName(string? tourName) { this.TourName = tourName; }

        public string? getTourDestination() { return TourDestination; }
        public void setTourDestination(string? tourDestination) { this.TourDestination = tourDestination; }

        public string? getTourImage() { return TourImage; }
        public void setTourImage(string? tourImage) { this.TourImage = tourImage; }

        public string? getTourDeparture() { return TourDeparture; }
        public void setTourDeparture(string? tourDeparture) { this.TourDeparture = tourDeparture; }

        public int getTourDuration() { return TourDuration; }
        public void setTourDuration(int tourDuration) { this.TourDuration = tourDuration; }

        public decimal getTourPrice() { return TourPrice; }
        public void setTourPrice(decimal tourPrice) { this.TourPrice = tourPrice; }
        public bool getIsPaid() { return isPaid; }
        public void setIsPaid(bool value)
        {
            isPaid = value;
        }

        // Helper methods
        public bool isPending()
        {
            return "PENDING".Equals(Status, StringComparison.OrdinalIgnoreCase);
        }

        public bool isConfirmed()
        {
            return "CONFIRMED".Equals(Status, StringComparison.OrdinalIgnoreCase);
        }

        public bool isCancelled()
        {
            return "CANCELLED".Equals(Status, StringComparison.OrdinalIgnoreCase);
        }

        public bool isCompleted()
        {
            return "COMPLETED".Equals(Status, StringComparison.OrdinalIgnoreCase);
        }

        public string getFormattedPrice()
        {
            if (TotalPrice == 0) return "0đ";
            return String.Format("{0:N0}đ", TotalPrice);
        }

        public string getFormattedDate()
        {
            if (BookingDate == null) return "";
            return BookingDate.Value.ToString("dd/MM/yyyy HH:mm");
        }

        public string getFormattedStatus()
        {
            switch (Status)
            {
                case "PENDING": return "Chờ xác nhận";
                case "CONFIRMED": return "Đã xác nhận";
                case "CANCELLED": return "Đã hủy";
                case "COMPLETED": return "Hoàn thành";
                default: return Status;
            }
        }

        public override bool Equals(object? o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            Booking booking = (Booking)o;
            return BookingId == booking.BookingId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BookingId);
        }
    }
}
