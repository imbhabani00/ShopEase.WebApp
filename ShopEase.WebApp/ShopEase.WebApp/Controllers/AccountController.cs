using Ecommerce.Application.Services;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Helpers;
using ShopEase.WebApp.Models.Auth;
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
        #endregion

        #region Constructor
        public AccountController(
            IAccountService accountService,
            ILogger<AccountController> logger,
            IUserService userService,
            IEmailService emailService,
            IOtpRepository otpRepository)
        {
            _accountService = accountService;
            _logger = logger;
            _userService = userService;
            _emailService = emailService;
            _otpRepository = otpRepository;
        }
        #endregion

        #region Login GET
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(AccessToken))
                return Redirect(RouteConstants.Dashboard);

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        #endregion

        #region Login POST (Step 1: Email + Password → Generate OTP)
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
                // Step 1: Validate email and password
                var result = await _accountService.LoginAsync(model);

                if (result == null || !result.Status || result.Response == null)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = result?.Message ?? "Login failed"
                    });
                }

                // Step 2: Deserialize token response
                var token = JsonConvert.DeserializeObject<TokenResponseModel>(result.Response.ToString()!);

                if (token == null)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Invalid response from server."
                    });
                }

                // Step 3: Generate 6-digit OTP
                var otp = new Random().Next(100000, 999999).ToString();

                // Step 4: Save OTP to database (10 min expiry)
                var otpSaved = await _otpRepository.SaveOtpAsync(token.UserId, otp, 10);

                if (!otpSaved)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Failed to generate OTP. Please try again."
                    });
                }

                // Step 5: Send OTP via email
                await _emailService.SendOtpAsync(model.Email, model.Email.Split('@')[0], otp);

                // Step 6: Store pending user data in session (temporary)
                SessionHelper.SetPendingUserId(HttpContext.Session, token.UserId);
                SessionHelper.SetPendingEmail(HttpContext.Session, model.Email);
                SessionHelper.SetPendingTenantId(HttpContext.Session, token.TenantId);

                _logger.LogInformation("Login: OTP generated for user {UserId}", token.UserId);

                return StatusCode(200, new
                {
                    status = true,
                    message = "OTP sent to your email.",
                    returnUrl = "/Account/VerifyOtp",
                    statusCode = 200
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login: Error occurred");
                return StatusCode(200, new
                {
                    status = false,
                    message = "An error occurred. Please try again."
                });
            }
        }
        #endregion

        #region VerifyOtp GET (Show OTP input form)
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var pendingUserId = SessionHelper.GetPendingUserId(HttpContext.Session);

            if (pendingUserId == 0)
                return Redirect(RouteConstants.Login);

            return View(new OtpViewModel());
        }
        #endregion

        #region VerifyOtp POST (Step 2: Verify OTP → Complete Login)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpViewModel model)
        {
            if (!ModelState.IsValid)
                return StatusCode(200, new
                {
                    status = false,
                    message = "Invalid OTP format"
                });

            try
            {
                // Get pending user from session
                var pendingUserId = SessionHelper.GetPendingUserId(HttpContext.Session);
                var pendingEmail = SessionHelper.GetPendingEmail(HttpContext.Session);
                var pendingTenantId = SessionHelper.GetPendingTenantId(HttpContext.Session);

                if (pendingUserId == 0)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Session expired. Please login again."
                    });
                }

                // Verify OTP
                var isOtpValid = await _otpRepository.VerifyOtpAsync(pendingUserId, model.Otp);

                if (!isOtpValid)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Invalid or expired OTP. Please try again."
                    });
                }

                // OTP verified - now get full token from API using pending email
                var loginModel = new LoginViewModel { Email = pendingEmail, Password = "" };
                var tokenResult = await _accountService.LoginAsync(loginModel);

                if (tokenResult == null || !tokenResult.Status || tokenResult.Response == null)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Failed to complete login. Please try again."
                    });
                }

                var token = JsonConvert.DeserializeObject<TokenResponseModel>(tokenResult.Response.ToString()!);

                if (token == null)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = "Invalid response from server."
                    });
                }

                // Step 7: Store tokens in session
                SessionHelper.SetAccessToken(HttpContext.Session, token.AccessToken);
                SessionHelper.SetRefreshToken(HttpContext.Session, token.RefreshToken);
                SessionHelper.SetUserId(HttpContext.Session, token.UserId);
                SessionHelper.SetRoleCode(HttpContext.Session, token.RoleCode);
                SessionHelper.SetRoleName(HttpContext.Session, token.RoleName);
                SessionHelper.SetTenantId(HttpContext.Session, token.TenantId);

                // Store refresh token in HTTP-only cookie
                CookieHelper.SetRefreshTokenCookie(Response, token.RefreshToken, 7);

                // Clear pending user data
                SessionHelper.ClearPendingUser(HttpContext.Session);

                // Invalidate OTP
                await _otpRepository.InvalidateOtpAsync(pendingUserId);

                _logger.LogInformation("Login: User {UserId} OTP verified and logged in successfully", token.UserId);

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
                _logger.LogError(ex, "VerifyOtp: Error occurred");
                return StatusCode(200, new
                {
                    status = false,
                    message = "An error occurred. Please try again."
                });
            }
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
            if (!string.IsNullOrEmpty(AccessToken))
                return Redirect(RouteConstants.Dashboard);

            return View(new RegisterViewModel());
        }
        #endregion

        #region Register POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
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
                registerViewModel.PasswordHash = registerViewModel.Password;
                registerViewModel.RoleId = 3; // Default: Buyer role
                registerViewModel.IsActive = true;

                var result = await _userService.UserSaveAsync(registerViewModel);

                if (result == null || !result.Status)
                {
                    return StatusCode(200, new
                    {
                        status = false,
                        message = result?.Message ?? "Registration failed"
                    });
                }

                _logger.LogInformation("Register: New user registered successfully");

                return StatusCode(200, new
                {
                    status = true,
                    message = "Registration successful. Please login.",
                    returnUrl = RouteConstants.Login
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register: Error occurred");
                return StatusCode(200, new
                {
                    status = false,
                    message = "An error occurred. Please try again."
                });
            }
        }
        #endregion
    }
}