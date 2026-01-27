using System.ComponentModel.DataAnnotations;

namespace TourBookingSystem.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; }


        // EMAIL (Java -> C# Regex)
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [RegularExpression(
            @"[a-zA-Z0-9_!#$%&’*+/=?`{|}~^.-]+@[a-zA-Z0-9.-]+$",
            ErrorMessage = "Email không đúng định dạng"
        )]
        public string Email { get; set; }


        // PHONE
        [RegularExpression(
            @"^(84|0[35789])[0-9]{8}$",
            ErrorMessage = "Số điện thoại không hợp lệ"
        )]
        public string? Phone { get; set; }


        // PASSWORD
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+=\-])\S{8,}$",
            ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường, số, ký tự đặc biệt và >=8 ký tự"
        )]
        public string Password { get; set; }


        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
