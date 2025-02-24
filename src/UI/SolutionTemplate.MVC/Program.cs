using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SolutionTemplate.DAL.Context;
using SolutionTemplate.DAL.MySQL;
using SolutionTemplate.DAL.Sqlite;
using SolutionTemplate.DAL.SqlServer;
using SolutionTemplate.MVC.Infrastructure;
using System;

namespace SolutionTemplate.MVC;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        // Add services to the container.
        // Подключаем логер
        builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

        // Подключаем в конфигурацию файл appsettings.json
        IConfigurationBuilder configBuilder = new ConfigurationBuilder()
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Оборачиваем секцию SolutionTemplate в объектную форму для удобства
        IConfiguration configuration = configBuilder.Build();
        AppConfig config = configuration.GetSection("Project").Get<AppConfig>()!;

        switch (builder.Configuration["Database"])
        {
            case "SqlServer":
                var sqlServerConnectionString = builder.Configuration.GetConnectionString(
                    builder.Configuration["Database"]
                    ?? throw new InvalidOperationException("Database configuration is missing."))!;
                services.AddSolutionTemplateDbContextSqlServer(sqlServerConnectionString);
                break;

            case "MySql":
                var mysqlConnectionString = builder.Configuration.GetConnectionString(
                    builder.Configuration["Database"]
                    ?? throw new InvalidOperationException("Database configuration is missing."))!;
                services.AddSolutionTemplateDbContextMysql(mysqlConnectionString);
                break;

            case "Sqlite":
                var sqliteConnectionString = builder.Configuration.GetConnectionString(
                    builder.Configuration["Database"]
                    ?? throw new InvalidOperationException("Database configuration is missing."));
                services.AddSolutionTemplateDbContextSqlite(sqliteConnectionString);
                break;

            default:
                throw new InvalidOperationException("Unsupported database type.");
        }

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        // Настраиваем Identity систему 
        //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true);
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
        }).AddEntityFrameworkStores<SolutionTemplateDB>().AddDefaultTokenProviders();
        // Настраиваем Auth cookie
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "SolutionTemplateAuth";
            options.Cookie.HttpOnly = true;
            options.LoginPath = "/account/login";
            options.AccessDeniedPath = "/admin/accessdenied";
            options.SlidingExpiration = true;
        });


        builder.Services.AddControllersWithViews()
            .AddRazorRuntimeCompilation();

        //services.AddHealthChecks()
        //.AddCheck<BaseHealthCheck>("Base
        
        var app = builder.Build();

        //! Порядок следования middleware очень важен, они будут выполнятся согласно нему

        // Включаем первым логирование
        app.UseSerilogRequestLogging();

        // Инициализируем базу данных
        await using (var scope = app.Services.CreateAsyncScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            await initializer.InitializeAsync();
        }


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthorization();
        app.UseStatusCodePagesWithReExecute("~/Status/{0}");

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        //app.MapRazorPages()
        //   .WithStaticAssets();

        await app.RunAsync();
    }
}
