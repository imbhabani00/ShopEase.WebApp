using Ecommerce.Web.Controllers;
using Ecommerce.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.Role;
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
        [PermissionFilter(PermissionConstants.Modules.Roles, PermissionConstants.CanView)]
        public IActionResult Index()
        {
            ViewData["ActiveMenu"] = "Role";
            ViewData["PageTitle"] = "Roles";
            ViewData["PageSubTitle"] = "Manage system roles";
            ViewData["BreadcrumbParent"] = "Admin";
            ViewData["BreadcrumbCurrent"] = "Roles";
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
        [PermissionFilter(PermissionConstants.Modules.Roles, PermissionConstants.CanAdd)]
        public IActionResult RoleAdd()
        {
            return PartialView("_Add", new RoleViewModel());
        }
        #endregion

        #region RoleEdit
        [HttpGet]
        [PermissionFilter(PermissionConstants.Modules.Roles, PermissionConstants.CanEdit)]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(RoleViewModel model)
        {
            var apiResponse = new ApiResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Validation failed.";
                    apiResponse.Response = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return new ObjectResult(apiResponse);
                }

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionFilter(PermissionConstants.Modules.Roles, PermissionConstants.CanDelete)]
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
    }
}