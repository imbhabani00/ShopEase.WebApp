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
        Task<List<PermissionModel>> GetByRoleIdAsync(int roleId);
        Task<ApiResponse> SavePermissionsAsync(List<SavePermissionsRequest> requests);
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

                var response = await DoHttpGet(query.ToString());
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
                if (apiResponse?.Status == true && apiResponse.Response != null)
                {
                    viewModel = JsonConvert.DeserializeObject<RoleViewModelList>(apiResponse.Response.ToString()!) ?? viewModel;
                    viewModel.Pager = new Pager
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
        public async Task<RoleViewModel?> GetByIdAsync(int roleId)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/role/role-by-id/{roleId}";
                var response = await DoHttpGet(endpoint, useAuth: true);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.Status == true && apiResponse.Response != null)
                    return JsonConvert.DeserializeObject<RoleViewModel>(apiResponse.Response.ToString()!);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("GetByIdAsync error: {Message}", ex.Message);
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
                var response = await DoHttpPost(endpoint, model, useAuth: true);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ApiResponse>(content)
                    ?? new ApiResponse { Status = false };
            }
            catch (Exception ex)
            {
                Log.Logger.Error("SaveAsync error: {Message}", ex.Message);
                return new ApiResponse { Status = false, Message = "Error occurred" };
            }
        }
        #endregion

        #region DeleteAsync
        public async Task<ApiResponse> DeleteAsync(int roleId)
        {
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/role/delete/{roleId}";
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

        #region GetByRoleIdAsync
        public async Task<List<PermissionModel>> GetByRoleIdAsync(int roleId)
        {
            var list = new List<PermissionModel>();

            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/role/by-role/{roleId}";
                var response = await DoHttpGet(endpoint);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.Status == true && apiResponse.Response != null)
                {
                    var permissionList = JsonConvert.DeserializeObject<PermissionAssignViewModel>(
                        apiResponse.Response.ToString()!);

                    if (permissionList != null)
                    {
                        list = permissionList.Permissions ?? new List<PermissionModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "GetByRoleIdAsync error");
            }

            return list;
        
        }
        #endregion

        #region SavePermissionsAsync
        public async Task<ApiResponse> SavePermissionsAsync(List<SavePermissionsRequest> requests)
        {
            var apiResponse = new ApiResponse();
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/role/permission-save";
                var response = await DoHttpPost(endpoint, requests);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content) ?? apiResponse;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("SavePermissionsAsync error: {Message}", ex.Message);
                apiResponse.Status = false;
                apiResponse.Message = "Error occurred";
            }
            return apiResponse;
        }
        #endregion
    }
}