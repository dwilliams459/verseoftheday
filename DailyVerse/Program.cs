using DailyVerse.Service;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Logging.ClearProviders().AddConsole().AddAzureWebAppDiagnostics();

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        builder.Services.AddScoped<NetBibleVersesService>();
        builder.Services.AddScoped<EsvVerseService>();

        var config = builder.Configuration;
        //config.AddJsonFile("keys.json", optional: true, reloadOnChange: false);
        config.AddEnvironmentVariables();

        var app = builder.Build();

        // Get a logger
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        // Log a message
        logger.LogInformation("Verse of the Day has started.");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        //app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}