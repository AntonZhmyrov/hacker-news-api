using HackerNewsAPI.Configuration;
using HackerNewsAPI.HackerNews;
using HackerNewsAPI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Polly;
using System.Globalization;
using System.Threading.RateLimiting;

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
            builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection(RateLimitOptions.Section));

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

            var rateLimitOptions = new RateLimitOptions();
            builder.Configuration.GetSection(RateLimitOptions.Section).Bind(rateLimitOptions);

            builder.Services.AddRateLimiter(limiterOptions =>
            {
                limiterOptions.OnRejected = (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    return new ValueTask();
                };

                limiterOptions.AddFixedWindowLimiter(policyName: RateLimitOptions.PolicyName, options =>
                {
                    options.PermitLimit = rateLimitOptions.PermitLimit;
                    options.Window = TimeSpan.FromSeconds(rateLimitOptions.TimeWindowSeconds);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateLimitOptions.QueueLimit;
                });
            });

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
            app.UseRateLimiter();

            app.Run();
        }
    }
}
