using Ecommerce.Web.Controllers;
using Ecommerce.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.Role;
using ShopEase.WebApp.Models.User;
using ShopEase.WebApp.Services;

namespace ShopEase.WebApp.Controllers
{
    public class RoleController : BaseController
    {
        #region Properties
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;
        #endregion

        #region Constructor
        public RoleController(
            IRoleService roleService,
            ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }
        #endregion

        #region Index
        [HttpGet]
        [PermissionFilter(PermissionConstants.Role_View_List)]
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region GetList
        [HttpGet]
        public async Task<IActionResult> GetList(SortWithPageParameter sortParams)
        {
            var model = new RoleViewModelList();
            try
            {
                model = await _roleService.GetListAsync(sortParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleController.GetList error");
            }
            return PartialView("_List", model);
        }
        #endregion

        #region RoleAdd
        [HttpGet]
        [PermissionFilter(PermissionConstants.Role_Add)]
        public IActionResult RoleAdd()
        {
            var roleAdd = new RoleViewModel();
            return PartialView("_Add", roleAdd);
        }
        #endregion

        #region RoleEdit
        [HttpGet]
        [PermissionFilter(PermissionConstants.Role_Edit)]
        public async Task<IActionResult> RoleEdit(int roleId)
        {
            var model = new RoleViewModel();
            try
            {
                model = await _roleService.GetByIdAsync(roleId) ?? new RoleViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleController.LoadEdit error - RoleId: {RoleId}", roleId);
            }
            return PartialView("_Edit", model);
        }
        #endregion

        #region Save
        [HttpPost]
        public async Task<IActionResult> Save(RoleViewModel model)
        {
            var apiResponse = new ApiResponse();
            try
            {
                apiResponse = await _roleService.SaveAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleController.Save error");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }
            return new ObjectResult(apiResponse);
        }
        #endregion

        #region Delete
        public IActionResult DeleteConfirm(int roleId)
        {
            var model = new RoleViewModel()
            {
                RoleId = roleId
            };
            return PartialView("_Delete", model);
        }

        [HttpDelete]
        [PermissionFilter(PermissionConstants.Role_Delete)]
        public async Task<IActionResult> Delete(int roleId)
        {
            var apiResponse = new ApiResponse();
            try
            {
                apiResponse = await _roleService.DeleteAsync(roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleController.Delete error - RoleId: {RoleId}", roleId);
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }
            return new ObjectResult(apiResponse);
        }
        #endregion

        #region ActiveInactive

        public IActionResult ActiveInactive(int roleId, bool isActive)
        {
            var model = new RoleViewModel()
            {
                RoleId = roleId,
                IsActive = isActive
            };

            return PartialView("_ActiveInactive", model);
        }


        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int roleId, bool isActive)
        {
            var apiResponse = new ApiResponse();

            try
            {
                apiResponse = await _roleService.ActiveInactiveAsync(roleId, isActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleController.ActiveInactive error - UserId: {RoleId}", roleId);

                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }

            return new ObjectResult(apiResponse);
        }

        #endregion

        #region LoadAssignGrid
        [HttpGet]
        [PermissionFilter(PermissionConstants.Permissions_Edit)]
        public async Task<IActionResult> LoadAssignGrid(int roleId)
        {
            var model = new PermissionAssignViewModel { RoleId = roleId };
            try
            {
                var role = await _roleService.GetByIdAsync(roleId);
                if (role != null) model.RoleName = role.RoleName;

                var permissionResult = await _roleService.GetByRoleIdAsync(roleId);
                model.Permissions = permissionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleController.LoadAssignGrid error - RoleId: {RoleId}", roleId);
            }
            return PartialView("_AssignGrid", model);
        }
        #endregion

        #region SavePermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermissions([FromBody] List<SavePermissionsRequest> requests)
        {
            var apiResponse = new ApiResponse();
            try
            {
                if (requests == null || requests.Count == 0)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Invalid permissions.";
                    return new ObjectResult(apiResponse);
                }

                apiResponse = await _roleService.SavePermissionsAsync(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleController.SavePermissions error");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }
            return new ObjectResult(apiResponse);
        }
        #endregion
    }
}