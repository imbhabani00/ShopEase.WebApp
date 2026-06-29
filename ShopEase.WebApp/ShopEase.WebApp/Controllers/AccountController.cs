using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Helpers;
using ShopEase.WebApp.Models.Auth;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Services;

namespace Ecommerce.Web.Controllers
{
    public class AccountController : BaseController
    {
        #region Properties
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        #endregion

        #region Constructor
        public AccountController(
            IAccountService accountService,
            ILogger<AccountController> logger,
            IUserService userService)
        {
            _accountService = accountService;
            _logger = logger;
            _userService = userService;
        }
        #endregion

        #region Login GET
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(AccessToken))
                return Redirect(RouteConstants.Dashboard);
            return View("Login");
        }
        #endregion

        #region Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return StatusCode(200, new
                {
                    status = false,
                    message = "Invalid input",
                    errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new
                {
                    propertyName = x.Key,
                    errorMessage = x.Value.Errors.First().ErrorMessage
                })
                });

            try
            {
                var result = await _accountService.LoginAsync(model);

                if (result == null || !result.Status || result.Response == null)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = result?.Message ?? "Login failed"
                    });
                }

                var token = JsonConvert.DeserializeObject<TokenResponseModel>(result.Response.ToString()!);

                if (token == null)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Invalid response from server."
                    });
                }

                // Store in session
                SessionHelper.SetAccessToken(HttpContext.Session, token.AccessToken);
                SessionHelper.SetRefreshToken(HttpContext.Session, token.RefreshToken);
                SessionHelper.SetUserId(HttpContext.Session, token.UserId);
                SessionHelper.SetRoleCode(HttpContext.Session, token.RoleCode);
                SessionHelper.SetRoleName(HttpContext.Session, token.RoleName);

                // Store refresh token in HTTP-only cookie as well
                CookieHelper.SetRefreshTokenCookie(Response, token.RefreshToken, 7);

                _logger.LogInformation("Login: User {UserId} logged in successfully", token.UserId);

                return StatusCode(200, new
                {
                    status = true,
                    message = "Login successful",
                    returnUrl = RouteConstants.Dashboard,
                    statusCode = 200
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login: Error occurred");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return StatusCode(200, new
                {
                    status = false,
                    message = "An error occurred. Please try again."
                });
            }
        }
        #endregion

        #region Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            SessionHelper.ClearSession(HttpContext.Session);
            CookieHelper.DeleteRefreshTokenCookie(Response);
            _logger.LogInformation("Logout: User {UserId} logged out", CurrentUserId);
            return Redirect(RouteConstants.Login);
        }
        #endregion

        #region AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        #endregion

        #region Register -- GET
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        #endregion

        #region Register -- POST
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            var apiResponse = new ApiResponse();
            try
            {
                apiResponse = await _userService.UserSaveAsync(registerViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register: Error occurred");
            }
            return new ObjectResult(apiResponse);
        }
        #endregion 
    }
}