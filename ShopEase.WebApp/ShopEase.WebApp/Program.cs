using Ecommerce.Web.Middleware;
using Serilog;
using ShopEase.WebApp.Extensions;
using ShopEase.WebApp.Models.Email;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Ecommerce Web...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, config) =>
    {
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .WriteTo.Console();
    });

    // MVC
    builder.Services.AddControllersWithViews();

    // Session
    builder.Services.AddSessionConfig(builder.Configuration);

    // HttpContextAccessor
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddSignalR();

    // Services + HttpClient + AppSettings
    builder.Services.AddApplicationServices(builder.Configuration);

    // Routing
    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    });

    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "RequestVerificationToken";
    });

    builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();
    app.UseMiddleware<AuthMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Web app failed to start");
}
finally
{
    Log.CloseAndFlush();
}