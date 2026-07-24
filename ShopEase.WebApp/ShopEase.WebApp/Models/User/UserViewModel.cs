using ShopEase.WebApp.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace ShopEase.WebApp.Models.User
{
    #region UserViewModel
    public class UserViewModel
    {
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public int? RoleId { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [MaxLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string Password { get; set; }
        public bool IsNewUser { get; set; }
        public bool ForcePasswordChange { get; set; }
        public string? PasswordHash { get; set; }
    }
    #endregion

    #region UserViewModelList
    public class UserViewModelList
    {
        public List<UserViewModel> UsersData { get; set; }
        public Pager pager { get; set; }
        public int TotalCount { get; set; }
    }
    #endregion
}