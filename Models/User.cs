using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Models
{
    /**
     * User model class with security enhancements
     */
    public class User
    {
        private int userId;
        private string fullName;
        private string email;
        private string password; // Marked as transient for security
        private string phone;
        private string role;
        private string createdAt;
        private string rememberToken;
        private string tokenExpiry;
        
        public User() {}
        
        public User(int userId, string fullName, string email, string password, 
                    string phone, string role, string createdAt)
        {
            this.userId = userId;
            this.fullName = fullName;
            this.email = email;
            this.password = password;
            this.phone = phone;
            this.role = role;
            this.createdAt = createdAt;
        }
        
        // Getters and Setters with validation
        public int getUserId() { return userId; }
        public void setUserId(int userId) 
        { 
            this.userId = userId; 
        }
        
        public string getFullName() { return fullName; }
        public void setFullName(string fullName) 
        { 
            this.fullName = (fullName != null) ? fullName.Trim() : null; 
        }
        
        public string getEmail() { return email; }
        public void setEmail(string email) 
        { 
            this.email = (email != null) ? email.Trim().ToLower() : null; 
        }
        
        // Password getter returns null for security (except for internal use)
        public string getPassword() 
        { 
            return null; // Never return actual password in normal usage
        }
        
        // Overloaded method to get password for DAO operations
        public string getHashedPassword() 
        { 
            return password;
        }
        
        public void setPassword(string password) 
        { 
            this.password = (password != null) ? password : null; 
        }
        
        public string getPhone() { return phone; }
        public void setPhone(string phone) 
        { 
            this.phone = (phone != null) ? phone.Trim() : null; 
        }
        
        public string getRole() { return role; }
        public void setRole(string role) 
        { 
            this.role = (role != null) ? role.Trim() : "USER"; 
        }
        
        public string getCreatedAt() { return createdAt; }
        public void setCreatedAt(string createdAt) 
        { 
            this.createdAt = createdAt; 
        }
        
        public string getRememberToken() { return rememberToken; }
        public void setRememberToken(string rememberToken) 
        { 
            this.rememberToken = rememberToken; 
        }
        
        public string getTokenExpiry() { return tokenExpiry; }
        public void setTokenExpiry(string tokenExpiry) 
        { 
            this.tokenExpiry = tokenExpiry; 
        }
        
        public override bool Equals(object? o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            User user = (User)o;
            return userId == user.userId && 
                   email == user.email;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(userId, email);
        }
        
        public override string ToString()
        {
            return "User{" +
                    "userId=" + userId +
                    ", fullName='" + fullName + '\'' +
                    ", email='" + email + '\'' +
                    ", phone='" + phone + '\'' +
                    ", role='" + role + '\'' +
                    ", createdAt='" + createdAt + '\'' +
                    '}';
        }
    }
}
