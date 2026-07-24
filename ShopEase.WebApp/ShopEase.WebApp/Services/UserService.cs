using Ecommerce.Application.Services;
using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using Serilog;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Models.Auth;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.User;
using System.Text;

namespace ShopEase.WebApp.Services
{
    #region Interface
    public interface IUserService
    {
        Task<UserViewModelList> GetListAsync(SortWithPageParameter sortParams);
        Task<UserViewModel?> GetByIdAsync(int userId);
        Task<ApiResponse> SaveAsync(UserViewModel model);
        Task<ApiResponse> DeleteAsync(int userId);

    }
    #endregion
    public class UserService : BaseService, IUserService
    {
        #region Properties
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;
        #endregion

        #region Constructor
        public UserService(IHttpClientFactory httpClientFactory,
        AppSettings appSettings,
        IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger, IEmailService emailService) : base(httpClientFactory, appSettings, httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _emailService = emailService;
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

        #region GetByIdAsync
        public async Task<UserViewModel?> GetByIdAsync(int userId)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/id/{userId}";
                var response = await DoHttpGet(endpoint, useAuth: true);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.Status == true && apiResponse.Response != null)
                    return JsonConvert.DeserializeObject<UserViewModel>(apiResponse.Response.ToString()!);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("GetByIdAsync error: {Message}", ex.Message);
            }

            return null;
        }
        #endregion

        #region SaveAsync
        private string GenerateTemporaryPassword()
        {
            return Guid.NewGuid()
                .ToString()
                .Substring(0, 8)
                + "@1";
        }
        public async Task<ApiResponse> SaveAsync(UserViewModel model)
        {
            try
            {
                bool isNewUser = !model.UserId.HasValue;
                string temporaryPassword = null;

                if (isNewUser)
                {
                    temporaryPassword = GenerateTemporaryPassword();
                    model.Password = temporaryPassword;
                    model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);
                }

                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/save";
                var response = await DoHttpPost(endpoint, model, useAuth: true);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Logger.Error("SaveAsync failed: {StatusCode} - {Content}", response.StatusCode, content);
                    return new ApiResponse { Status = false, Message = "Error occurred" };
                }

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content)
                    ?? new ApiResponse { Status = false };

                if (apiResponse.Status && isNewUser)
                {
                    await _emailService.SendWelcomeAsync(model.Email, $"{model.FirstName} {model.LastName}", model.Email, temporaryPassword);
                }

                return apiResponse;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "SaveAsync error");
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion

        #region DeleteAsync
        public async Task<ApiResponse> DeleteAsync(int userId)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/delete/{userId}";
                var response = await DoHttpDelete(endpoint);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                return apiResponse ?? new ApiResponse { Status = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                Log.Logger.Error("RoleService.DeleteAsync error: {Message}", ex.Message);
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion
    }
}