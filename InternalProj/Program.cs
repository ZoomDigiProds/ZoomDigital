using InternalProj.Data;
using InternalProj.Models;
using InternalProj.Service;
using Microsoft.EntityFrameworkCore;

namespace InternalProj
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<WorkOrderService>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            // ? Add session services
            int timeoutValue = builder.Configuration.GetValue<int>("SessionSettings:TimeoutValue");
            string timeoutUnit = builder.Configuration.GetValue<string>("SessionSettings:TimeoutUnit") ?? "Minutes";

            TimeSpan idleTimeout = timeoutUnit.ToLower() switch
            {
                "seconds" => TimeSpan.FromSeconds(timeoutValue),
                "minutes" => TimeSpan.FromMinutes(timeoutValue),
                _ => TimeSpan.FromMinutes(timeoutValue)
            };

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = idleTimeout;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //builder.WebHost.UseUrls("http://192.168.1.62:5190");
            int timeoutSeconds = (int)idleTimeout.TotalSeconds;
            app.Use(async (context, next) =>
            {
                context.Items["SessionTimeoutSeconds"] = timeoutSeconds;
                await next();
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseMiddleware<SessionRefreshMiddleware>(); // ?? Add this

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
