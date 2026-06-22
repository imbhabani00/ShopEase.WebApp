namespace ShopEase.WebApp.Models.Common
{
    public class ApiResponse
    {
        public bool Status { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public object? Response { get; set; }
    }
}
