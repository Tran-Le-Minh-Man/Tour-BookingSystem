using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Models
{
    /**
     * BookingDetail model class
     */
    public class BookingDetail
    {
        private int id;
        private int bookingId;
        private string participantName;
        private string participantPhone;
        private string participantEmail;
        private string note;
        
        public BookingDetail() {}
        
        public BookingDetail(int id, int bookingId, string participantName, 
                            string participantPhone, string participantEmail, string note)
        {
            this.id = id;
            this.bookingId = bookingId;
            this.participantName = participantName;
            this.participantPhone = participantPhone;
            this.participantEmail = participantEmail;
            this.note = note;
        }
        
        // Getters and Setters
        public int getId() { return id; }
        public void setId(int id) { this.id = id; }
        
        public int getBookingId() { return bookingId; }
        public void setBookingId(int bookingId) { this.bookingId = bookingId; }
        
        public string getParticipantName() { return participantName; }
        public void setParticipantName(string participantName) 
        { 
            this.participantName = (participantName != null) ? participantName.Trim() : null; 
        }
        
        public string getParticipantPhone() { return participantPhone; }
        public void setParticipantPhone(string participantPhone) 
        { 
            this.participantPhone = (participantPhone != null) ? participantPhone.Trim() : null; 
        }
        
        public string getParticipantEmail() { return participantEmail; }
        public void setParticipantEmail(string participantEmail) 
        { 
            this.participantEmail = (participantEmail != null) ? participantEmail.Trim() : null; 
        }
        
        public string getNote() { return note; }
        public void setNote(string note) 
        { 
            this.note = (note != null) ? note.Trim() : null; 
        }
    }
}
