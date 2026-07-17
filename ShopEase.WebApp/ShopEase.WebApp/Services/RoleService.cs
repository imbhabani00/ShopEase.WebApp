using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using Serilog;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.Role;
using System.Text;

namespace ShopEase.WebApp.Services
{
    #region Interface
    public interface IRoleService
    {
        Task<RoleViewModelList> GetListAsync(SortWithPageParameter sortParams);
        Task<RoleViewModel?> GetByIdAsync(int roleId);
        Task<ApiResponse> SaveAsync(RoleViewModel model);
        Task<ApiResponse> DeleteAsync(int roleId);
    }
    #endregion

    public class RoleService : BaseService, IRoleService
    {
        #region Properties
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RoleService> _logger;
        #endregion

        #region Constructor
        public RoleService(
                IHttpClientFactory httpClientFactory,
                AppSettings appSettings,
                ILogger<RoleService> logger,
                IHttpContextAccessor httpContextAccessor)
                : base(httpClientFactory, appSettings, httpContextAccessor)
        {
            _appSettings = appSettings;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        #endregion

        #region GetListAsync
        public async Task<RoleViewModelList> GetListAsync(SortWithPageParameter sortParams)
        {
            var viewModel = new RoleViewModelList();
            try
            {
                if (sortParams.PageNumber == null || sortParams.PageNumber < 1)
                    sortParams.PageNumber = 1;
                if (sortParams.PageSize == null || sortParams.PageSize < 1)
                    sortParams.PageSize = 10;

                var query = new StringBuilder();
                query.Append($"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/role/list?");
                query.Append($"PageNumber={sortParams.PageNumber}&");
                query.Append($"PageSize={sortParams.PageSize}&");
                query.Append($"SearchString={sortParams.SearchString}&");
                query.Append($"SortParameter={sortParams.SortParameter}&");
                query.Append($"SortDirection={sortParams.SortDirection}&");
                query.Append($"StatusId={sortParams.StatusId}");

                var apiResponse = await DoHttpGet<ApiResponse>(query.ToString());

                if (apiResponse?.Status == true && apiResponse.Response != null)
                    viewModel = JsonConvert.DeserializeObject<RoleViewModelList>(
                        apiResponse.Response.ToString()!) ?? viewModel;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("RoleService.GetListAsync error: {Message}", ex.Message);
            }
            return viewModel;
        }
        #endregion

        #region GetByIdAsync
        public async Task<RoleViewModel?> GetByIdAsync(int roleId)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/role/role-by-id/{roleId}";

                var apiResponse = await DoHttpGet<ApiResponse>(endpoint);

                if (apiResponse?.Status == true && apiResponse.Response != null)
                    return JsonConvert.DeserializeObject<RoleViewModel>(
                        apiResponse.Response.ToString()!);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("RoleService.GetByIdAsync error: {Message}", ex.Message);
            }
            return null;
        }
        #endregion

        #region SaveAsync
        public async Task<ApiResponse> SaveAsync(RoleViewModel model)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/role/save";

                var result = await DoHttpPost<ApiResponse>(endpoint, model, useAuth: true);
                return result ?? new ApiResponse { Status = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                Log.Logger.Error("RoleService.SaveAsync error: {Message}", ex.Message);
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion

        #region DeleteAsync
        public async Task<ApiResponse> DeleteAsync(int roleId)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/role/{roleId}";

                var result = await DoHttpDelete<ApiResponse>(endpoint);
                return result ?? new ApiResponse { Status = false, Message = "No response" };
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