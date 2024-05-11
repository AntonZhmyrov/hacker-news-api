using AspNetCoreRateLimit;
using HackerNewsAPI.Configuration;
using HackerNewsAPI.HackerNews;
using HackerNewsAPI.Services;
using Microsoft.Extensions.Options;
using Polly;
using System.Net;

namespace HackerNewsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<HackerNewsOptions>(builder.Configuration.GetSection(HackerNewsOptions.Section));
            builder.Services.AddHttpClient<IHackerNewsApiService, HackerNewsApiService>()
                .AddTransientHttpErrorPolicy(policyBuilder => 
                    policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var hackerNewsOptions = serviceProvider.GetRequiredService<IOptions<HackerNewsOptions>>();

                    httpClient.BaseAddress = new Uri(hackerNewsOptions.Value.ApiHost);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    }
                });

            builder.Services.AddTransient<IBestStoriesService, BestStoriesService>();

            builder.Services.AddLogging();

            // Add Rate Limiting
            builder.Services.AddMemoryCache();

            builder.Services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = false;
                options.HttpStatusCode = (int)HttpStatusCode.TooManyRequests;
                options.RealIpHeader = "X-Real-IP";
                options.ClientIdHeader = "X-ClientId";
                options.QuotaExceededMessage = "Requests quota limit reached! The maximum amount of requests in 10 seconds is 2.";
                options.GeneralRules = new List<RateLimitRule> 
                { 
                    new() 
                    {
                        Endpoint = "*",
                        Period = "10s",
                        Limit = 2
                    } 
                };
            });

            builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            builder.Services.AddInMemoryRateLimiting();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.UseIpRateLimiting();

            app.Run();
        }
    }
}
