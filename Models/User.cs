using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Models
{
    /**
     * User model class with security enhancements
     */
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int UserId { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("password_hash")]
        public string? PasswordHash { get; set; } // Renamed from password for clarity

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("role")]
        public string Role { get; set; } = "USER";

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("remember_token")]
        public string? RememberToken { get; set; }

        [Column("token_expiry")]
        public DateTime? TokenExpiry { get; set; }

        public User() { }

        public User(int userId, string fullName, string email, string password,
                    string phone, string role, DateTime? createdAt)
        {
            this.UserId = userId;
            this.FullName = fullName;
            this.Email = email;
            this.PasswordHash = password;
            this.Phone = phone;
            this.Role = role;
            this.CreatedAt = createdAt;
        }

        // Legacy Getters and Setters
        public int getUserId() { return UserId; }
        public void setUserId(int userId)
        {
            this.UserId = userId;
        }

        public string? getFullName() { return FullName; }
        public void setFullName(string? fullName)
        {
            this.FullName = (fullName != null) ? fullName.Trim() : null;
        }

        public string? getEmail() { return Email; }
        public void setEmail(string? email)
        {
            this.Email = (email != null) ? email.Trim().ToLower() : null;
        }

        // Password getter returns null for security (except for internal use)
        public string? getPassword()
        {
            return null; // Never return actual password in normal usage
        }

        // Overloaded method to get password for DAO operations
        public string? getHashedPassword()
        {
            return PasswordHash;
        }

        public void setPassword(string? password)
        {
            this.PasswordHash = (password != null) ? password : null;
        }

        public string? getPhone() { return Phone; }
        public void setPhone(string? phone)
        {
            this.Phone = (phone != null) ? phone.Trim() : null;
        }

        public string getRole() { return Role; }
        public void setRole(string? role)
        {
            this.Role = (role != null) ? role.Trim() : "USER";
        }

        public string? getCreatedAt() { return CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss"); }
        public void setCreatedAt(string? createdAt)
        {
            if (DateTime.TryParse(createdAt, out DateTime dt))
                this.CreatedAt = dt;
            else
                this.CreatedAt = null;
        }

        public void setCreatedAt(DateTime? createdAt)
        {
            this.CreatedAt = createdAt;
        }

        public string? getRememberToken() { return RememberToken; }
        public void setRememberToken(string? rememberToken)
        {
            this.RememberToken = rememberToken;
        }

        public string? getTokenExpiry() { return TokenExpiry?.ToString("yyyy-MM-dd HH:mm:ss"); }
        public void setTokenExpiry(string? tokenExpiry)
        {
            if (DateTime.TryParse(tokenExpiry, out DateTime dt))
                this.TokenExpiry = dt;
            else
                this.TokenExpiry = null;
        }

        public void setTokenExpiry(DateTime? tokenExpiry)
        {
            this.TokenExpiry = tokenExpiry;
        }

        public override bool Equals(object? o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            User user = (User)o;
            return UserId == user.UserId &&
                   Email == user.Email;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, Email);
        }

        public override string ToString()
        {
            return "User{" +
                    "userId=" + UserId +
                    ", fullName='" + FullName + '\'' +
                    ", email='" + Email + '\'' +
                    ", phone='" + Phone + '\'' +
                    ", role='" + Role + '\'' +
                    ", createdAt='" + CreatedAt + '\'' +
                    '}';
        }
    }
}
