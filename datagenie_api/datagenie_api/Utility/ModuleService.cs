using datagenie_api.Interfaces;
using datagenie_api.Repositorys;
using datagenie_api.Services;

namespace datagenie_api.Utility
{
    public static class ModuleService
    {
        public static IServiceCollection AddDatagenieServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IHelperRepository, HelperRepository>();
            services.AddScoped<IRegistrationRepository, RegistrationRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();

            // Services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<JwtService>();
            services.AddScoped<EncryptionService>();

            return services;
        }
    }
}
