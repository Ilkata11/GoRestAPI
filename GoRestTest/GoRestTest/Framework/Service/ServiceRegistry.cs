using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoRestTest.Framework
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            var testConfig = configuration.GetSection("TestConfiguration").Get<TestConfiguration>();
            services.AddSingleton(testConfig);
            services.AddSingleton(provider => new HttpClientProvider(testConfig.BaseUrl, testConfig.Token));
            return services;
        }
    }
}
