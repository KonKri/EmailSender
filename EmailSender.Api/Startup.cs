using EmailSender.Api;
using EmailSender.Api.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EmailSender.Api
{
    public class Startup : FunctionsStartup
    {
        private readonly IConfigurationRoot _config;

        public Startup()
        {
            _config = new ConfigurationBuilder().AddEnvironmentVariables()
                                                .Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ApiAuthorization>(factory =>
            {
                return new ApiAuthorization
                {
                    ValidKey = _config["ApiKey"]
                };
            });
        }
    }
}
