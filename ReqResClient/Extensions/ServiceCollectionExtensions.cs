using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using ReqResClient.Configuration;
using ReqResClient.Interfaces;
using ReqResClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReqResClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReqResClient(this IServiceCollection services,
            IConfiguration configuration)
        {
            //Register options
            services.Configure<ReqResApiOptions>(
                configuration.GetSection(ReqResApiOptions.SectionName));

            //Register memory cache if not already registered
            services.AddMemoryCache();

            //Register HttpClient with resilience policies
            services.AddHttpClient<IReqResApiClient, ReqResApiClient>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            //Register services
            services.AddTransient<IExternalUserService, ExternalUserService>();

            return services;
        }
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt =>  TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}
