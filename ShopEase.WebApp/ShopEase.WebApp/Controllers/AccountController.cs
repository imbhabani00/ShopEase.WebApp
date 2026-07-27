using Ecommerce.Application.Services;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Helpers;
using ShopEase.WebApp.Models.Auth;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Repositories;
using ShopEase.WebApp.Services;

namespace Ecommerce.Web.Controllers
{
    public class AccountController : BaseController
    {
        #region Properties
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private readonly IRoleService _roleService;
        #endregion

        #region Constructor
        public AccountController(
            IAccountService accountService,
            ILogger<AccountController> logger,
            IUserService userService,
            IEmailService emailService,
            IOtpRepository otpRepository,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            AppSettings appSettings,
            IRoleService roleService)
        {
            _accountService = accountService;
            _logger = logger;
            _userService = userService;
            _emailService = emailService;
            _otpRepository = otpRepository;
            _httpContextAccessor = httpContextAccessor;
            _appSettings = appSettings;
            _httpClientFactory = httpClientFactory;
            _roleService = roleService;
        }
        #endregion

        #region Login GET
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(AccessToken))
            {
                var forcePasswordChange = SessionHelper.GetForcePasswordChange(HttpContext.Session);
                return Redirect(forcePasswordChange ? RouteConstants.ChangePassword : RouteConstants.Dashboard);
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        #endregion

        #region Login POST (Step 1: Email + Password → Generate OTP)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var apiResponse = new ApiResponse();

            if (!ModelState.IsValid)
            {
                apiResponse.Status = false;
                apiResponse.Message = "Invalid input";
                return new ObjectResult(apiResponse);
            }

            try
            {
                var result = await _accountService.LoginAsync(model);

                if (result == null || !result.Status || result.Response == null)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = result?.Message ?? "Login failed";
                    return new ObjectResult(apiResponse);
                }

                var token = JsonConvert.DeserializeObject<TokenResponseModel>(result.Response.ToString()!);

                if (token == null)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Invalid response from server.";
                    return new ObjectResult(apiResponse);
                }

                _logger.LogWarning("Login: token.ForcePasswordChange = {Value}", token.ForcePasswordChange);   // 👈 add this

                if (token.ForcePasswordChange)
                {
                    SessionHelper.SetAccessToken(HttpContext.Session, token.AccessToken);
                    SessionHelper.SetRefreshToken(HttpContext.Session, token.RefreshToken);
                    SessionHelper.SetUserId(HttpContext.Session, token.UserId);
                    SessionHelper.SetRoleCode(HttpContext.Session, token.RoleCode);
                    SessionHelper.SetRoleName(HttpContext.Session, token.RoleName);
                    SessionHelper.SetTenantId(HttpContext.Session, token.TenantId);
                    SessionHelper.SetRoleId(HttpContext.Session, token.RoleId);
                    SessionHelper.SetForcePasswordChange(HttpContext.Session, true);

                    var userDetails = await _userService.GetByIdAsync(token.UserId);
                    if (userDetails != null)
                    {
                        SessionHelper.SetUserFullName(HttpContext.Session, userDetails.FullName);
                        SessionHelper.SetUserEmail(HttpContext.Session, userDetails.Email ?? "");
                        SessionHelper.SetUserPhone(HttpContext.Session, userDetails.PhoneNumber ?? "");
                        SessionHelper.SetInitials(HttpContext.Session, userDetails.Initials ?? "");
                        SessionHelper.SetBackgroundColorCode(HttpContext.Session, userDetails.BackgroundColorCode ?? "#1F3358");
                        SessionHelper.SetColorCode(HttpContext.Session, userDetails.ColorCode ?? "#FFFFFF");
                        SessionHelper.SetProfilePicturePath(HttpContext.Session, userDetails.ProfilePicturePath);
                    }

                    _logger.LogWarning("Login: session forcePasswordChange SET to true");
                    CookieHelper.SetRefreshTokenCookie(Response, token.RefreshToken, 7);

                    _logger.LogInformation("Login: User {UserId} requires forced password change, OTP skipped", token.UserId);

                    apiResponse.Status = true;
                    apiResponse.Message = "Please set a new password to continue.";
                    apiResponse.ReturnUrl = RouteConstants.ChangePassword;
                    return new ObjectResult(apiResponse);
                }

                var otp = new Random().Next(100000, 999999).ToString();
                _accountService.StoreTokenInSession(token, _httpContextAccessor);

                var otpSaved = await _otpRepository.SaveOtpAsync(model.Email, token.UserId, otp, 10);
                if (!otpSaved)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Failed to generate OTP. Please try again.";
                    return new ObjectResult(apiResponse);
                }

                await _emailService.SendOtpAsync(model.Email, model.Email.Split('@')[0], otp);

                SessionHelper.SetPendingUserId(HttpContext.Session, token.UserId);
                SessionHelper.SetPendingEmail(HttpContext.Session, model.Email);
                SessionHelper.SetPendingTenantId(HttpContext.Session, token.TenantId);

                _logger.LogInformation("Login: OTP generated for user {UserId}", token.UserId);

                apiResponse.Status = true;
                apiResponse.Message = "OTP sent to your email.";
                apiResponse.ReturnUrl = RouteConstants.VerifyOtp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login: Error occurred");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred. Please try again.";
            }

            return new ObjectResult(apiResponse);
        }
        #endregion

        #region VerifyOtp GET (Show OTP input form)
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var model = new OtpViewModel();
            var pendingUserId = SessionHelper.GetPendingUserId(HttpContext.Session);
            var pendingEmail = SessionHelper.GetPendingEmail(HttpContext.Session);
            if (pendingUserId == 0)
                return Redirect(RouteConstants.Login);
            ViewBag.Email = pendingEmail;
            return View(model);
        }
        #endregion

        #region VerifyOtp POST (Step 2: Verify OTP → Complete Login)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpViewModel model)
        {
            var apiResponse = new ApiResponse();

            if (!ModelState.IsValid)
            {
                apiResponse.Status = false;
                apiResponse.Message = "Invalid OTP format";
                return new ObjectResult(apiResponse);
            }

            try
            {
                var pendingUserId = SessionHelper.GetPendingUserId(HttpContext.Session);
                var pendingEmail = SessionHelper.GetPendingEmail(HttpContext.Session);

                if (pendingUserId == 0)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Session expired. Please login again.";
                    return new ObjectResult(apiResponse);
                }

                var isOtpValid = await _otpRepository.VerifyOtpAsync(pendingUserId, pendingEmail, model.Otp);
                if (!isOtpValid)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Invalid or expired OTP. Please try again.";
                    return new ObjectResult(apiResponse);
                }

                var token = _accountService.GetTokenFromSession(_httpContextAccessor);
                if (token == null)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Session expired. Please login again.";
                    return new ObjectResult(apiResponse);
                }

                SessionHelper.SetAccessToken(HttpContext.Session, token.AccessToken);
                SessionHelper.SetRefreshToken(HttpContext.Session, token.RefreshToken);
                SessionHelper.SetUserId(HttpContext.Session, token.UserId);
                SessionHelper.SetRoleCode(HttpContext.Session, token.RoleCode);
                SessionHelper.SetRoleName(HttpContext.Session, token.RoleName);
                SessionHelper.SetTenantId(HttpContext.Session, token.TenantId);
                SessionHelper.SetRoleId(HttpContext.Session, token.RoleId);
                SessionHelper.SetForcePasswordChange(HttpContext.Session, token.ForcePasswordChange);

                try
                    {
                    var permissions = await _roleService.GetByRoleIdAsync(token.RoleId);
                    if (permissions != null && permissions.Count > 0)
                    {
                        SessionHelper.SetPermissions(HttpContext.Session, permissions);
                        _logger.LogInformation("VerifyOtp: Permissions loaded for role {RoleId}", token.RoleId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "VerifyOtp: Failed to load permissions");
                }

                CookieHelper.SetRefreshTokenCookie(Response, token.RefreshToken, 7);

                SessionHelper.ClearPendingUser(HttpContext.Session);
                _accountService.ClearTempToken(_httpContextAccessor);
                await _otpRepository.InvalidateOtpAsync(pendingUserId);

                _logger.LogInformation("Login: User {UserId} OTP verified and logged in successfully", token.UserId);

                var forcePasswordChange = SessionHelper.GetForcePasswordChange(HttpContext.Session);

                apiResponse.Status = true;
                apiResponse.Message = "Login successful";
                apiResponse.ReturnUrl = forcePasswordChange ? RouteConstants.ChangePassword : RouteConstants.Dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyOtp: Error occurred");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred. Please try again.";
            }

            return new ObjectResult(apiResponse);
        }
        #endregion

        #region ResendOtp POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp()
        {
            try
            {
                var pendingUserId = SessionHelper.GetPendingUserId(HttpContext.Session);
                var pendingEmail = SessionHelper.GetPendingEmail(HttpContext.Session);

                if (pendingUserId == 0)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Session expired. Please login again."
                    });
                }

                // Generate new OTP
                var newOtp = new Random().Next(100000, 999999).ToString();

                // Resend OTP
                var otpResent = await _otpRepository.ResendOtpAsync(pendingUserId, newOtp, 10);

                if (!otpResent)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Failed to resend OTP. Please try again."
                    });
                }

                // Send via email
                await _emailService.SendOtpAsync(pendingEmail, "User", newOtp);

                _logger.LogInformation("ResendOtp: OTP resent for user {UserId}", pendingUserId);

                return StatusCode(200, new
                {
                    status = true,
                    message = "OTP resent to your email."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResendOtp: Error occurred");
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

        #region Register GET
        [HttpGet]
        public IActionResult Register()
        {
            var registerModel = new RegisterViewModel();
            if (!string.IsNullOrEmpty(AccessToken))
                return Redirect(RouteConstants.Dashboard);

            return View(registerModel);
        }
        #endregion

        //#region Register POST
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        //{
        //    if (!ModelState.IsValid)
        //        return StatusCode(200, new
        //        {
        //            status = false,
        //            message = "Invalid input",
        //            errors = ModelState
        //                .Where(x => x.Value.Errors.Count > 0)
        //                .Select(x => new
        //                {
        //                    propertyName = x.Key,
        //                    errorMessage = x.Value.Errors.First().ErrorMessage
        //                })
        //        });

        //    try
        //    {
        //        registerViewModel.PasswordHash = registerViewModel.Password;
        //        registerViewModel.RoleId = 3; // Default: Buyer role
        //        registerViewModel.IsActive = true;

        //        var result = await _userService.UserSaveAsync(registerViewModel);

        //        if (result == null || !result.Status)
        //        {
        //            return StatusCode(200, new
        //            {
        //                status = false,
        //                message = result?.Message ?? "Registration failed"
        //            });
        //        }

        //        _logger.LogInformation("Register: New user registered successfully");

        //        return StatusCode(200, new
        //        {
        //            status = true,
        //            message = "Registration successful. Please login.",
        //            returnUrl = RouteConstants.Login
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Register: Error occurred");
        //        return StatusCode(200, new
        //        {
        //            status = false,
        //            message = "An error occurred. Please try again."
        //        });
        //    }
        //}
        //#endregion

        #region Profile
        public async Task Profile()
        {
            var loginViewModel = new LoginViewModel();
        }
        #endregion

        #region ChangePassword GET
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            if (string.IsNullOrEmpty(AccessToken))
                return Redirect(RouteConstants.Login);
            ViewBag.IsForced = SessionHelper.GetForcePasswordChange(HttpContext.Session);
            return View("_ChangePassword", model);
        }
        #endregion

        #region ChangePassword POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var apiResponse = new ApiResponse();

            if (!ModelState.IsValid)
            {
                apiResponse.Status = false;
                apiResponse.Message = "Invalid input";
                return new ObjectResult(apiResponse);
            }

            try
            {
                var userId = SessionHelper.GetUserId(HttpContext.Session);
                if (userId == null || userId == 0)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Session expired. Please login again.";
                    return new ObjectResult(apiResponse);
                }

                var isForced = SessionHelper.GetForcePasswordChange(HttpContext.Session);
                var otpVerifiedForChange = SessionHelper.GetOtpVerifiedForPasswordChange(HttpContext.Session);

                if (!isForced && !otpVerifiedForChange)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "OTP verification required before changing password.";
                    return new ObjectResult(apiResponse);
                }

                var result = await _accountService.ChangePasswordAsync(userId.Value, model.NewPassword);

                if (result == null || !result.Status)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = result?.Message ?? "Failed to change password.";
                    return new ObjectResult(apiResponse);
                }

                SessionHelper.SetForcePasswordChange(HttpContext.Session, false);
                SessionHelper.ClearOtpVerifiedForPasswordChange(HttpContext.Session);
                SessionHelper.ClearSession(HttpContext.Session);
                CookieHelper.DeleteRefreshTokenCookie(Response);

                _logger.LogInformation("ChangePassword: User {UserId} changed password successfully", userId);

                apiResponse.Status = true;
                apiResponse.Message = "Password changed successfully. Please login again.";
                apiResponse.ReturnUrl = RouteConstants.Login;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePassword: Error occurred");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred. Please try again.";
            }

            return new ObjectResult(apiResponse);
        }
        #endregion

        #region RequestPasswordChangeOtp POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestPasswordChangeOtp()
        {
            var apiResponse = new ApiResponse();
            try
            {
                var userId = SessionHelper.GetUserId(HttpContext.Session);
                if (userId == null || userId == 0)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Session expired. Please login again.";
                    return new ObjectResult(apiResponse);
                }

                var email = SessionHelper.GetUserEmail(HttpContext.Session);

                var otp = new Random().Next(100000, 999999).ToString();
                var otpSaved = await _otpRepository.SaveOtpAsync(email, userId.Value, otp, 10);

                if (!otpSaved)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Failed to generate OTP. Please try again.";
                    return new ObjectResult(apiResponse);
                }

                await _emailService.SendOtpAsync(email, email.Split('@')[0], otp);

                apiResponse.Status = true;
                apiResponse.Message = "OTP sent to your email.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RequestPasswordChangeOtp: Error occurred");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred. Please try again.";
            }

            return new ObjectResult(apiResponse);
        }
        #endregion

        #region VerifyPasswordChangeOtp POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPasswordChangeOtp(OtpViewModel model)
        {
            var apiResponse = new ApiResponse();

            if (!ModelState.IsValid)
            {
                apiResponse.Status = false;
                apiResponse.Message = "Invalid OTP format";
                return new ObjectResult(apiResponse);
            }

            try
            {
                var userId = SessionHelper.GetUserId(HttpContext.Session);
                var email = SessionHelper.GetUserEmail(HttpContext.Session);

                if (userId == null || userId == 0)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Session expired. Please login again.";
                    return new ObjectResult(apiResponse);
                }

                var isOtpValid = await _otpRepository.VerifyOtpAsync(userId.Value, email, model.Otp);
                if (!isOtpValid)
                {
                    apiResponse.Status = false;
                    apiResponse.Message = "Invalid or expired OTP. Please try again.";
                    return new ObjectResult(apiResponse);
                }

                await _otpRepository.InvalidateOtpAsync(userId.Value);
                SessionHelper.SetOtpVerifiedForPasswordChange(HttpContext.Session, true);

                apiResponse.Status = true;
                apiResponse.Message = "OTP verified.";
                apiResponse.ReturnUrl = RouteConstants.ChangePassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyPasswordChangeOtp: Error occurred");
                apiResponse.Status = false;
                apiResponse.Message = "An error occurred. Please try again.";
            }

            return new ObjectResult(apiResponse);
        }
        #endregion
    }
}