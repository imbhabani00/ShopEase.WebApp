using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using Serilog;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.Role;

namespace ShopEase.WebApp.Services
{
    #region Interface
    public interface IPermissionService
    {
        Task<List<PermissionModel>> GetByRoleIdAsync(int roleId);
        Task<ApiResponse> SavePermissionsAsync(SavePermissionsRequest request);
    }
    #endregion

    public class PermissionService : BaseService, IPermissionService
    {
        #region Properties
        private readonly AppSettings _appSettings;
        #endregion

        #region Constructor
        public PermissionService(
            IHttpClientFactory httpClientFactory,
            AppSettings appSettings,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, appSettings, httpContextAccessor)
        {
            _appSettings = appSettings;
        }
        #endregion

        #region GetByRoleIdAsync
        public async Task<List<PermissionModel>> GetByRoleIdAsync(int roleId)
        {
            var list = new List<PermissionModel>();
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/permission/by-role/{roleId}";
                var response = await DoHttpGet(endpoint);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.Status == true && apiResponse.Response != null)
                    list = JsonConvert.DeserializeObject<List<PermissionModel>>(
                        apiResponse.Response.ToString()!) ?? list;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("GetByRoleIdAsync error: {Message}", ex.Message);
            }
            return list;
        }
        #endregion

        #region SavePermissionsAsync
        public async Task<ApiResponse> SavePermissionsAsync(SavePermissionsRequest request)
        {
            var apiResponse = new ApiResponse();
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/permission/save";
                var response = await DoHttpPost(endpoint, request);

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