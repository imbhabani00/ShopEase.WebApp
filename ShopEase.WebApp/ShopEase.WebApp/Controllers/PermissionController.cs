using Ecommerce.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.Role;
using ShopEase.WebApp.Services;

namespace Ecommerce.Web.Controllers
{
    public class PermissionController : BaseController
    {
        #region Properties
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;
        private readonly ILogger<PermissionController> _logger;
        #endregion

        #region Constructor
        public PermissionController(
            IPermissionService permissionService,
            IRoleService roleService,
            ILogger<PermissionController> logger)
        {
            _permissionService = permissionService;
            _roleService = roleService;
            _logger = logger;
        }
        #endregion

        #region Index
        [HttpGet]
        [PermissionFilter(PermissionConstants.Modules.Roles, PermissionConstants.CanView)]
        public IActionResult Index()
        {
            ViewData["ActiveMenu"] = "Role";
            ViewData["PageTitle"] = "Role Permissions";
            ViewData["PageSubTitle"] = "Assign module permissions per role";
            ViewData["BreadcrumbParent"] = "Admin";
            ViewData["BreadcrumbCurrent"] = "Permissions";
            return View();
        }
        #endregion

        #region LoadAssignGrid
        [HttpGet]
        public async Task<IActionResult> LoadAssignGrid(int roleId)
        {
            var model = new PermissionAssignViewModel { RoleId = roleId};
            try
            {
                var role = await _roleService.GetByIdAsync(roleId);
                if (role != null) model.RoleName = role.RoleName;

                var permissionResult = await _permissionService.GetByRoleIdAsync(roleId);
                model.Permissions = permissionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionController.LoadAssignGrid error - RoleId: {RoleId}", roleId);
            }
            return PartialView("_AssignGrid", model);
        }
        #endregion

        #region SavePermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(PermissionConstants.Modules.Roles, PermissionConstants.CanEdit)]
        public async Task<IActionResult> SavePermissions([FromBody] SavePermissionsRequest request)
        {
            var apiResponse = new ApiResponse();
            try
            {
                if (request.RoleId <= 0)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Invalid role.";
                    return new ObjectResult(apiResponse);
                }

                apiResponse = await _permissionService.SavePermissionsAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionController.SavePermissions error");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }
            return new ObjectResult(apiResponse);
        }
        #endregion
    }
}