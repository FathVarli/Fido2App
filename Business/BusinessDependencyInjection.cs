using Business.Service.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Business
{
    public static class BusinessDependencyInjection
    {
        public static void AddBusinessService(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();

        }
    }
}