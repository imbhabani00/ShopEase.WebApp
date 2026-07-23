using ShopEase.WebApp.Models.Common;

namespace ShopEase.WebApp.Models.User
{
    #region UserViewModel
    public class UserViewModel
    {
        public int? UserId { get; set; }
        public int? RoleId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleName { get; set; }
        public string RoleCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
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