using System.ComponentModel.DataAnnotations;

namespace ShopEase.WebApp.Models.Auth
{
    public class OtpViewModel
    {
        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only digits")]
        public string Otp { get; set; } = string.Empty;
    }
}