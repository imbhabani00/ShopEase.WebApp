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
                var apiResponse = await DoHttpGet<ApiResponse>(endpoint);
                if (apiResponse?.Status == true && apiResponse.Response != null)
                    list = JsonConvert.DeserializeObject<List<PermissionModel>>(
                        apiResponse.Response.ToString()!) ?? list;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("PermissionService.GetByRoleIdAsync error: {Message}", ex.Message);
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
                var result = await DoHttpPost<ApiResponse>(endpoint, request);
                if (result != null)
                    apiResponse = result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("PermissionService.SavePermissionsAsync error: {Message}", ex.Message);
            }
            return apiResponse;
        }
        #endregion
    }
}