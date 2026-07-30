using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using Serilog;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.Role;

namespace ShopEase.WebApp.Services
{
    #region Interface
    public interface IModuleService
    {
        Task<ModuleViewModelList> GetAllAsync();
    }
    #endregion

    public class ModuleService : BaseService, IModuleService
    {
        #region Properties
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ModuleService> _logger;
        #endregion

        #region Constructor
        public ModuleService(
            IHttpClientFactory httpClientFactory,
            AppSettings appSettings,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ModuleService> logger)
            : base(httpClientFactory, appSettings, httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        #endregion

        #region GetAllAsync
        public async Task<ModuleViewModelList> GetAllAsync()
        {
            var list = new ModuleViewModelList();
            try
            {
                var endpoint = $"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/module/list";
                var response = await DoHttpGet(endpoint);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.Status == true && apiResponse.Response != null)
                    list = JsonConvert.DeserializeObject<ModuleViewModelList>(
                        apiResponse.Response.ToString()!) ?? list;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("GetAllAsync error: {Message}", ex.Message);
            }
            return list;
        }
        #endregion
    }
}