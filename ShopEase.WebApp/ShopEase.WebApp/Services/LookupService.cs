using Ecommerce.Web.Services.Base;
using Newtonsoft.Json;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Models.Common;
using ShopEase.WebApp.Models.lookup;
using System.Text;

namespace ShopEase.WebApp.Services
{
    public interface ILookupService
    {
        Task<LookupList> GetRolesAsync();
    }
    public class LookupService : BaseService, ILookupService
    {
        #region Properties
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSettings _appSettings;
        private readonly ILogger<LookupService> _logger;
        #endregion

        #region Constructor
        public LookupService(IHttpClientFactory httpClientFactory, AppSettings appSettings, IHttpContextAccessor httpContextAccessor, ILogger<LookupService> logger) :
            base(httpClientFactory, appSettings, httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        #endregion

        #region GetRolesAsync
        public async Task<LookupList> GetRolesAsync()
        {
            var lookupList = new LookupList();
            try
            {
                var query = new StringBuilder();
                query.Append($"{ShopEaseApiUrl}/api/v{ShopEaseApiVersion}/lookup/roles");

                var response = await DoHttpGet(query.ToString());
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.StatusCode == 200 && apiResponse.Response != null)
                {
                    lookupList = JsonConvert.DeserializeObject<LookupList>(
                        apiResponse.Response.ToString()
                    ) ?? new LookupList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching roles.");
            }

            return lookupList;
        }
        #endregion
    }
}
