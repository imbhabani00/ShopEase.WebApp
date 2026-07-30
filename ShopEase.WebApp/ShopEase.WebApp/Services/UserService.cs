using Ecommerce.Application.Services;
using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using Serilog;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.User;
using System.Text;

namespace ShopEase.WebApp.Services
{
    #region Interface
    public interface IUserService
    {
        Task<UserViewModelList> GetListAsync(SortWithPageParameter sortParams);
        Task<UserDetails> GetByIdAsync(int userId);
        Task<ApiResponse> SaveAsync(UserViewModel model);
        Task<ApiResponse> DeleteAsync(int userId);
        Task<ApiResponse> UploadProfilePictureAsync(IFormFile file);
        Task<ApiResponse> RemoveProfilePictureAsync();
        Task<ApiResponse> ActiveInactiveAsync(int userId, bool isActive);
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
        public async Task<UserDetails> GetByIdAsync(int userId)
        {
            var data = new UserDetails();
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/id/{userId}";
                var response = await DoHttpGet(endpoint, useAuth: true);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.Status == true && apiResponse.Response != null)
                    return JsonConvert.DeserializeObject<UserDetails>(apiResponse.Response.ToString()!);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("GetByIdAsync error: {Message}", ex.Message);
            }

            return data;
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
                    model.ForcePasswordChange = true;
                    model.IsActive = true;
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

        #region ActiveInactiveAsync

        public async Task<ApiResponse> ActiveInactiveAsync(int userId, bool isActive)
        {
            try
            {
                var query = new StringBuilder();

                query.Append($"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/active-inactive?");
                query.Append($"userId={userId}&isActive={isActive}");

                var apiResponse = await DoHttpPut<ApiResponse>(
                    query.ToString(),
                    null,
                    useAuth: true
                );

                return apiResponse ?? new ApiResponse
                {
                    Status = false,
                    Message = "Invalid response"
                };
            }
            catch (Exception ex)
            {
                Log.Logger.Error("UserService.ActiveInactiveAsync error: {Message}", ex.Message);

                return new ApiResponse
                {
                    Status = false,
                    Message = "Error occurred"
                };
            }
        }

        #endregion+

        #region UploadProfilePictureAsync
        public async Task<ApiResponse> UploadProfilePictureAsync(IFormFile file)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/upload-profile-picture";
                var response = await DoHttpPostFile(endpoint, file, "file", useAuth: true);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    var errorContent = response != null ? await response.Content.ReadAsStringAsync() : "no response";
                    Log.Logger.Error("UploadProfilePictureAsync failed: {Content}", errorContent);
                    return new ApiResponse { Status = false, Message = "Error occurred while uploading picture." };
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content)
                    ?? new ApiResponse { Status = false };

                return apiResponse;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "UploadProfilePictureAsync error");
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion

        #region RemoveProfilePictureAsync
        public async Task<ApiResponse> RemoveProfilePictureAsync()
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/user/remove-profile-picture";
                var response = await DoHttpPostNoBody(endpoint, useAuth: true);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    var errorContent = response != null ? await response.Content.ReadAsStringAsync() : "no response";
                    Log.Logger.Error("RemoveProfilePictureAsync failed: {Content}", errorContent);
                    return new ApiResponse { Status = false, Message = "Error occurred while removing picture." };
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content)
                    ?? new ApiResponse { Status = false };

                return apiResponse;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "RemoveProfilePictureAsync error");
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion
    }
}