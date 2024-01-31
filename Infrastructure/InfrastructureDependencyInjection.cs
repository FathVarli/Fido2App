using Infrastructure.Repository.Abstract;
using Infrastructure.Repository.Base;
using Infrastructure.Repository.Concrete;
using Infrastructure.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static void AddInfrastructureService(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddRepositories();

        }

        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IStoredCredentialDetailRepository, StoredCredentialDetailRepository>();
        }
    }
}