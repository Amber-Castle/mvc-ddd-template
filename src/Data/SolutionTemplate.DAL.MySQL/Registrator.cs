using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolutionTemplate.DAL.Context;
using SolutionTemplate.DAL.Repositories;

namespace SolutionTemplate.DAL.MySQL;

/// <summary>Регистратор сервисов слоя данных для MySql</summary>
public static class Registrator
{
    /// <summary>Добавить контекст данных в контейнер сервисов для подключения к MySql</summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="ConnectionString">Строка подключения к серверу</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddSolutionTemplateDbContextMysql(this IServiceCollection services, string ConnectionString) => 
        services
           .AddDbContext<SolutionTemplateDB>(opt => opt
           .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString),
               o => o.MigrationsAssembly(typeof(Registrator).Assembly.FullName)))
           .AddScoped<IDbInitializer, SolutionTemplateDBInitializer>()
           .AddSolutionTemplateRepositories();

    /// <summary>Добавить фабрику контекста данных в контейнер сервисов для подключения к MySql</summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="ConnectionString">Строка подключения к серверу</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddSolutionTemplateDbContextFactoryMysql(this IServiceCollection services, string ConnectionString) => 
        services
           .AddDbContextFactory<SolutionTemplateDB>(opt => opt
           .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString),
               o => o.MigrationsAssembly(typeof(Registrator).Assembly.FullName)))
           .AddScoped<IDbInitializer, SolutionTemplateDBInitializer>()
           .AddSolutionTemplateRepositoryFactories();
}