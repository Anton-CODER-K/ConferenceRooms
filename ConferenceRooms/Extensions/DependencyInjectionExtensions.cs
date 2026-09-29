using ConferenceRooms.Database;
using ConferenceRooms.Repositories;
using ConferenceRooms.Repositories.Interfaces;
using ConferenceRooms.Services;

namespace ConferenceRooms.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<JwtService>();
        

            return services;
        }

        public static IServiceCollection AddRepositories(
            this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            
            return services;
        }

        public static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<DbConnectionFactory>();
            services.AddScoped<TransactionExecutor>();

            return services;
        }
    }
}
