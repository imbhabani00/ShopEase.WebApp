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

        #region 
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
    }
}
