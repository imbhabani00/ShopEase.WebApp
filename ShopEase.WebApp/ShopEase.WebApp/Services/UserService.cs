using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using Serilog;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Models.Auth;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.Role;
using ShopEase.WebApp.Models.User;
using System.Text;

namespace ShopEase.WebApp.Services
{
    #region Interface
    public interface IUserService
    {
        Task<ApiResponse> UserSaveAsync(RegisterViewModel registerViewModel);
        Task<UserViewModelList> GetListAsync(SortWithPageParameter sortParams);
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

        #region GetListAsync
        public async Task<UserViewModelList> GetListAsync(SortWithPageParameter sortParams)
        {
            var viewModel = new UserViewModelList();
            try
            {
                if (sortParams.PageNumber == null || sortParams.PageNumber < 1)
                    sortParams.PageNumber = 1;
                if (sortParams.PageSize == null || sortParams.PageSize < 1)
                    sortParams.PageSize = 10;

                var query = new StringBuilder();
                query.Append($"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/list?");
                query.Append($"PageNumber={sortParams.PageNumber}&");
                query.Append($"PageSize={sortParams.PageSize}&");
                query.Append($"SearchString={sortParams.SearchString}&");
                query.Append($"SortParameter={sortParams.SortParameter}&");
                query.Append($"SortDirection={sortParams.SortDirection}&");
                query.Append($"StatusId={sortParams.StatusId}");

                var response = await DoHttpGet(query.ToString());
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
                if (apiResponse?.Status == true && apiResponse.Response != null)
                {
                    viewModel = JsonConvert.DeserializeObject<UserViewModelList>(apiResponse.Response.ToString()!) ?? viewModel;
                    viewModel.pager = new Pager
                    {
                        CurrentPage = sortParams.PageNumber.Value,
                        PageSize = sortParams.PageSize.Value,
                        TotalItems = viewModel.TotalCount
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("GetListAsync error: {Message}", ex.Message);
            }

            return viewModel;
        }
        #endregion
    }
}