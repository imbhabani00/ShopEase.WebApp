using Microsoft.AspNetCore.Mvc;
using ShopEase.WebApp.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace ShopEase.WebApp.Models.Role
{
    public class RoleViewModel : SortWithPageParameter
    {
        public int? RoleId { get; set; }
        [Required(ErrorMessage = "Role name is required")]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Role code is required")]
        [MaxLength(20)]
        public string RoleCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RoleViewModelList
    {
        public List<RoleViewModel> Roles { get; set; }
        public Pager Pager { get; set; }
        public int TotalCount { get; set; }
    }
}