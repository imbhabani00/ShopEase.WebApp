using Newtonsoft.Json;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Constants;
using System.Net.Http.Headers;
using System.Text;

namespace Ecommerce.Web.Services.Base
{
    public abstract class BaseService
    {
        #region Properties
        private string _ShopEaseApiUrl = "";
        private string _ShopEaseApiVersion = "";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected string ShopEaseApiUrl => _appSettings.EcommerceApi.BaseUrl;
        protected string ShopEaseApiVersion => _appSettings.EcommerceApi.Version;
        #endregion

        #region Constructor
        protected BaseService(
            IHttpClientFactory httpClientFactory,
            AppSettings appSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region GetHttpClient
        private HttpClient GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient("EcommerceApi");
            var token = _httpContextAccessor.HttpContext?.Session
                .GetString(SessionConstants.AccessToken);

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
        #endregion

        #region DoHttpGet
        protected async Task<HttpResponseMessage?> DoHttpGet(string endpoint, bool useAuth = true)
        {
            try
            {
                var client = useAuth ? GetHttpClient() : _httpClientFactory.CreateClient("EcommerceApi");
                return await client.GetAsync(endpoint);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region DoHttpPut
        protected async Task<T?> DoHttpPut<T>(string endpoint, object payload, bool useAuth = true)
        {
            try
            {
                var client = useAuth ? GetHttpClient() : _httpClientFactory.CreateClient("EcommerceApi");

                var json = JsonConvert.SerializeObject(payload);

                using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await client.PutAsync(endpoint, httpContent);

                if (!response.IsSuccessStatusCode)
                    return default;

                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region DoHttpPost
        protected async Task<HttpResponseMessage?> DoHttpPost(string endpoint, object payload, bool useAuth = true)
        {
            try
            {
                var client = useAuth ? GetHttpClient() : _httpClientFactory.CreateClient("EcommerceApi");

                var json = JsonConvert.SerializeObject(payload);

                using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                return await client.PostAsync(endpoint, httpContent);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region DoHttpDelete
        protected async Task<HttpResponseMessage?> DoHttpDelete(string endpoint, bool useAuth = true)
        {
            try
            {
                var client = useAuth ? GetHttpClient() : _httpClientFactory.CreateClient("EcommerceApi");

                return await client.DeleteAsync(endpoint);
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}