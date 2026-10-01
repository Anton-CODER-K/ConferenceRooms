using ConferenceRooms.Database;
using ConferenceRooms.Repositories;
using ConferenceRooms.Repositories.Interfaces;
using ConferenceRooms.Services;
using ConferenceRooms.Services.Interfaces;

namespace ConferenceRooms.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<RoomService>();
            services.AddScoped<JwtService>();
            services.AddScoped<ServiceCatalog>();
            services.AddScoped<BookingService>();
            services.AddScoped<ReportService>();
            services.AddScoped<IPricingService, PricingService>();



            return services;
        }

        public static IServiceCollection AddRepositories(
            this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IPricingRepository, PricingRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

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
