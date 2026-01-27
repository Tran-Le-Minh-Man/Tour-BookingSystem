using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Models
{
    /**
     * BookingDetail model class
     */
    [Table("booking_details")]
    public class BookingDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("booking_id")]
        public int BookingId { get; set; }

        [Column("participant_name")]
        public string? ParticipantName { get; set; }

        [Column("participant_phone")]
        public string? ParticipantPhone { get; set; }

        [Column("participant_email")]
        public string? ParticipantEmail { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        public BookingDetail() { }

        public BookingDetail(int id, int bookingId, string participantName,
                            string participantPhone, string participantEmail, string note)
        {
            this.Id = id;
            this.BookingId = bookingId;
            this.ParticipantName = participantName;
            this.ParticipantPhone = participantPhone;
            this.ParticipantEmail = participantEmail;
            this.Note = note;
        }

        // Legacy Getters and Setters
        public int getId() { return Id; }
        public void setId(int id) { this.Id = id; }

        public int getBookingId() { return BookingId; }
        public void setBookingId(int bookingId) { this.BookingId = bookingId; }

        public string? getParticipantName() { return ParticipantName; }
        public void setParticipantName(string? participantName)
        {
            this.ParticipantName = (participantName != null) ? participantName.Trim() : null;
        }

        public string? getParticipantPhone() { return ParticipantPhone; }
        public void setParticipantPhone(string? participantPhone)
        {
            this.ParticipantPhone = (participantPhone != null) ? participantPhone.Trim() : null;
        }

        public string? getParticipantEmail() { return ParticipantEmail; }
        public void setParticipantEmail(string? participantEmail)
        {
            this.ParticipantEmail = (participantEmail != null) ? participantEmail.Trim() : null;
        }

        public string? getNote() { return Note; }
        public void setNote(string? note)
        {
            this.Note = (note != null) ? note.Trim() : null;
        }
    }
}
