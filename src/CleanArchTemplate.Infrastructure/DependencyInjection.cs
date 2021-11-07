using System.Linq;
using System.Text.RegularExpressions;
#if (includeDB)
using CleanArchTemplate.Application.Interfaces.Persistence;
#endif
using CleanArchTemplate.Application.Interfaces.Services;
#if (includeDB)
using CleanArchTemplate.Infrastructure.Persistence;
#endif
using CleanArchTemplate.Infrastructure.Services;
#if (includeDB)
using Microsoft.EntityFrameworkCore;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchTemplate.Infrastructure
{
    public static class DependencyInjection
    {
        private static readonly Regex InterfacePattern = new Regex("I(?:.+)DataService", RegexOptions.Compiled);

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
#if (enablepostgres)
            services.AddDbContext<CleanArchTemplateDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory")
                ).UseSnakeCaseNamingConvention();
            });
#endif
#if (includeDB)
            services.AddScoped<ICleanArchTemplateDbContext, CleanArchTemplateDbContext>();
#endif

            (from c in typeof(Application.DependencyInjection).Assembly.GetTypes()
             where c.IsInterface && InterfacePattern.IsMatch(c.Name)
             from i in typeof(Infrastructure.DependencyInjection).Assembly.GetTypes()
             where c.IsAssignableFrom(i)
             select new
             {
                 Contract = c,
                 Implementation = i
             }).ToList()
            .ForEach(x => services.AddScoped(x.Contract, x.Implementation));

            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));

            return services;
        }
    }
}
