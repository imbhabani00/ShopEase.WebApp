using Ecommerce.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.User;
using ShopEase.WebApp.Services;

namespace ShopEase.WebApp.Controllers
{
    public class UserController : Controller
    {
        #region Properties
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        #endregion

        #region Constructor
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }
        #endregion 

        #region Index
        [HttpGet]
        [PermissionFilter(PermissionConstants.Modules.Roles, PermissionConstants.CanView)]
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region GetList
        [HttpGet]
        public async Task<IActionResult> GetList(SortWithPageParameter sortParams)
        {
            var model = new UserViewModelList();
            try
            {
                model = await _userService.GetListAsync(sortParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleController.GetList error");
            }
            return PartialView("_List", model);
        }
        #endregion

        #region UserAdd
        [HttpGet]
        [PermissionFilter(PermissionConstants.Modules.Users, PermissionConstants.CanAdd)]
        public IActionResult UserAdd()
        {
            var userAdd = new UserViewModel();
            return PartialView("_Add", userAdd);
        }
        #endregion

        #region UserEdit
        [HttpGet]
        [PermissionFilter(PermissionConstants.Modules.Users, PermissionConstants.CanEdit)]
        public async Task<IActionResult> UserEdit(int userId)
        {
            var model = new UserViewModel();
            try
            {
                model = await _userService.GetByIdAsync(userId) ?? new UserViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserController.UserEdit error - userId: {userId}", userId);
            }
            return PartialView("_Edit", model);
        }
        #endregion

        #region Save
        [HttpPost]
        public async Task<IActionResult> Save(UserViewModel model)
        {
            var apiResponse = new ApiResponse();
            try
            {
                apiResponse = await _userService.SaveAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserController.Save error");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }
            return new ObjectResult(apiResponse);
        }
        #endregion

        #region Delete
        public IActionResult DeleteConfirm(int userId)
        {
            var model = new UserViewModel()
            {
                UserId = userId
            };
            return PartialView("_Delete", model);
        }

        [HttpDelete]
        [PermissionFilter(PermissionConstants.Modules.Users, PermissionConstants.CanDelete)]
        public async Task<IActionResult> Delete(int userId)
        {
            var apiResponse = new ApiResponse();
            try
            {
                apiResponse = await _userService.DeleteAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserController.Delete error - UserId: {UserId}", userId);
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }
            return new ObjectResult(apiResponse);
        }
        #endregion
    }
}
