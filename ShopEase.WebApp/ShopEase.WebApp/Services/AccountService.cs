using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using Serilog;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Models.Auth;
using ShopEase.WebApp.Models.Common;

namespace Ecommerce.Web.Services
{
    public interface IAccountService
    {
        Task<ApiResponse> LoginAsync(LoginViewModel model);
        Task<ApiResponse> RefreshTokenAsync(string accessToken, string refreshToken);
        Task LogoutAsync();
        void StoreTokenInSession(TokenResponseModel token, IHttpContextAccessor httpContextAccessor);
        TokenResponseModel? GetTokenFromSession(IHttpContextAccessor httpContextAccessor);
        void ClearTempToken(IHttpContextAccessor httpContextAccessor);
        Task<ApiResponse> ChangePasswordAsync(int userId, string newPassword);
        Task<ApiResponse> LoginWithGoogleAsync(string email, string? name);
    }
    public class AccountService : BaseService, IAccountService
    {
        #region Properties
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Constructor
        public AccountService(
            IHttpClientFactory httpClientFactory,
            AppSettings appSettings,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, appSettings, httpContextAccessor)
        {
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region ClearTempToken
        public void ClearTempToken(IHttpContextAccessor httpContextAccessor)
        {
            httpContextAccessor.HttpContext?.Session.Remove("TempToken");
        }
        #endregion

        #region LoginAsync
        public async Task<ApiResponse> LoginAsync(LoginViewModel model)
        {
            var endpoint = ApiSettings.AccessTokenEndpoint(
                _appSettings.EcommerceApi.BaseUrl,
                _appSettings.EcommerceApi.Version);

            var payload = new
            {
                Email = model.Email,
                PasswordHash = model.Password
            };

            var response = await DoHttpPost(endpoint, payload, useAuth: false);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse>(content)
                ?? new ApiResponse { Status = false, Message = "No response from server" };
        }
        #endregion

        #region RefreshTokenAsync
        public async Task<ApiResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var endpoint = ApiSettings.RefreshTokenEndpoint(
                _appSettings.EcommerceApi.BaseUrl,
                _appSettings.EcommerceApi.Version);

            var payload = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            var response = await DoHttpPost(endpoint, payload, useAuth: false);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse>(content)
                ?? new ApiResponse { Status = false, Message = "No response from server" };
        }
        #endregion

        #region LogoutAsync
        public Task LogoutAsync()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
            return Task.CompletedTask;
        }
        #endregion

        #region StoreTokenInSession
        public void StoreTokenInSession(TokenResponseModel token, IHttpContextAccessor httpContextAccessor)
        {
            var json = JsonConvert.SerializeObject(token);
            httpContextAccessor.HttpContext.Session.SetString("TempToken", json);
        }
        #endregion

        #region GetTokenFromSession
        public TokenResponseModel? GetTokenFromSession(IHttpContextAccessor httpContextAccessor)
        {
            var json = httpContextAccessor.HttpContext.Session.GetString("TempToken");
            if (string.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject<TokenResponseModel>(json);
        }
        #endregion

        #region ChangePasswordAsync
        public async Task<ApiResponse> ChangePasswordAsync(int userId, string newPassword)
        {
            try
            {
                var payload = new { UserId = userId, PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword) };
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/change-password";
                var response = await DoHttpPost(endpoint, payload, useAuth: true);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Logger.Error("ChangePasswordAsync failed: {StatusCode} - {Content}", response.StatusCode, content);
                    return new ApiResponse { Status = false, Message = "Failed to change password." };
                }

                return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse { Status = false };
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "ChangePasswordAsync error");
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion

        #region LoginWithGoogleAsync
        public async Task<ApiResponse> LoginWithGoogleAsync(string email, string? name)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/login-google";

                var payload = new
                {
                    Email = email,
                    FullName = name
                };

                var response = await DoHttpPost(endpoint, payload, useAuth: false);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Logger.Error("LoginWithGoogleAsync failed: {StatusCode} - {Content}", response.StatusCode, content);
                    return new ApiResponse { Status = false, Message = "No account found for this Google email." };
                }

                return JsonConvert.DeserializeObject<ApiResponse>(content)
                    ?? new ApiResponse { Status = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "LoginWithGoogleAsync error");
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion
    }
}