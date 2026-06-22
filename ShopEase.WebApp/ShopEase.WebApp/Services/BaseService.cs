using Newtonsoft.Json;
using ShopEase.WebApp.Configuration;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Models.Auth;
using ShopEase.WebApp.Models.Common;
using System.Net.Http.Headers;
using System.Text;

namespace Ecommerce.Web.Services.Base
{
    public abstract class BaseService
    {
        #region Properties
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
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

        #region GetAccessToken
        protected async Task<string?> GetAccessTokenAsync(string email, string password)
        {
            try
            {
                var endpoint = ApiSettings.AccessTokenEndpoint(
                    _appSettings.EcommerceApi.BaseUrl,
                    _appSettings.EcommerceApi.Version);

                var payload = new { Email = email, PasswordHash = password };
                var response = await DoHttpPost<ApiResponse>(endpoint, payload, useAuth: false);

                if (response?.Status == true && response.Response != null)
                {
                    var token = JsonConvert.DeserializeObject<TokenResponseModel>(
                        response.Response.ToString()!);
                    return token?.AccessToken;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region DoHttpGet
        protected async Task<T?> DoHttpGet<T>(string endpoint)
        {
            try
            {
                var client = GetHttpClient();
                var response = await client.GetAsync(endpoint);
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
        protected async Task<T?> DoHttpPost<T>(string endpoint, object payload, bool useAuth = true)
        {
            try
            {
                var client = useAuth ? GetHttpClient() : _httpClientFactory.CreateClient("EcommerceApi");
                var json = JsonConvert.SerializeObject(payload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, httpContent);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region DoHttpPut
        protected async Task<T?> DoHttpPut<T>(string endpoint, object payload)
        {
            try
            {
                var client = GetHttpClient();
                var json = JsonConvert.SerializeObject(payload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(endpoint, httpContent);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region DoHttpDelete
        protected async Task<T?> DoHttpDelete<T>(string endpoint)
        {
            try
            {
                var client = GetHttpClient();
                var response = await client.DeleteAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                return default;
            }
        }
        #endregion
    }
}