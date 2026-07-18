using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Models.Auth;
using ShopEase.WebApp.Models.Common;

namespace ShopEase.WebApp.Services
{
    #region Interface
    public interface IUserService
    {
        Task<ApiResponse> UserSaveAsync(RegisterViewModel registerViewModel);
    }
    #endregion
    public class UserService : BaseService, IUserService
    {
        #region Properties
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;
        #endregion

        #region Constructor
        public UserService(IHttpClientFactory httpClientFactory,
        AppSettings appSettings,
        IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger) : base(httpClientFactory, appSettings, httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        #endregion

        #region UserSaveAsync
        public async Task<ApiResponse> UserSaveAsync(RegisterViewModel registerViewModel)
        {
            try
            {
                registerViewModel.PasswordHash = registerViewModel.Password;
                if (registerViewModel.RoleId == 0)
                    registerViewModel.RoleId = RoleConstants.Customer;
                registerViewModel.IsActive = RoleConstants.IsActive;

                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/register";
                var response = await DoHttpPost(endpoint, registerViewModel, useAuth: false);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ApiResponse>(content)
                    ?? new ApiResponse { Status = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserSaveAsync: Registration error");
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion
    }
}