using Ecommerce.Web.Controllers;
using Ecommerce.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Helpers;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.User;
using ShopEase.WebApp.Services;
using System.Reflection.Metadata.Ecma335;

namespace ShopEase.WebApp.Controllers
{
    public class UserController : BaseController
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
        [PermissionFilter(PermissionConstants.User_View_List)]
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
        [PermissionFilter(PermissionConstants.User_Add)]
        public IActionResult UserAdd()
        {
            var userAdd = new UserViewModel();
            return PartialView("_Add", userAdd);
        }
        #endregion

        #region UserEdit
        [HttpGet]
        [PermissionFilter(PermissionConstants.User_Edit)]
        public async Task<IActionResult> UserEdit(int userId)
        {
            var model = new UserDetails();
            try
            {
                model = await _userService.GetByIdAsync(userId) ?? new UserDetails();
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
        [PermissionFilter(PermissionConstants.User_Delete)]
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

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = SessionHelper.GetUserId(HttpContext.Session) ?? 0;
            var model = await _userService.GetByIdAsync(userId) ?? new UserDetails();
            return PartialView("_Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var apiResponse = new ApiResponse();
            try
            {
                var userId = SessionHelper.GetUserId(HttpContext.Session) ?? 0;
                apiResponse = await _userService.UploadProfilePictureAsync(userId, file);

                if (apiResponse.Status)
                {
                    var newPath = apiResponse.Response?.ToString();
                    SessionHelper.SetProfilePicturePath(HttpContext.Session, newPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccountController.UploadProfilePicture error");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }
            return new ObjectResult(apiResponse);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            var apiResponse = new ApiResponse();
            try
            {
                var userId = SessionHelper.GetUserId(HttpContext.Session) ?? 0;
                apiResponse = await _userService.RemoveProfilePictureAsync(userId);

                if (apiResponse.Status)
                {
                    SessionHelper.ClearProfilePicturePath(HttpContext.Session);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccountController.RemoveProfilePicture error");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred.";
            }
            return new ObjectResult(apiResponse);
        }
    }
}
