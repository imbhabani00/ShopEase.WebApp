using System.Net;

namespace Ecommerce.Web.Middleware
{
    public class ExceptionMiddleware
    {
        #region Properties
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        #endregion

        #region Constructor
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        #endregion

        #region Invoke
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                context.Response.Redirect("/Home/Error");
            }
        }
        #endregion
    }
}