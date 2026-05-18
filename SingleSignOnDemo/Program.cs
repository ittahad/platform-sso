using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SaaSDataLayer;
using System.Net;
using static SaaSDataLayer.MongoDbConnectionCache;

namespace SingleSignOnDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            var options = new WebApplicationOptions { Args = args };

            var builder = WebApplication.CreateBuilder(options);

            builder.Logging.AddConsole();

            var httpPort = int.Parse(Environment.GetEnvironmentVariable("HTTP_PORT") ?? "80");
            var enableHttps = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_HTTPS") ?? "false");
            var httpsPort = int.Parse(Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "443");

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Any, httpPort);
                if (enableHttps)
                {
                    options.Listen(IPAddress.Any, httpsPort, listenOptions =>
                    {
                        //listenOptions.UseHttps("Certificates/client1.com+6.pfx", "12345");
                    });
                }
            });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto |
                    ForwardedHeaders.XForwardedHost;

                // Trust reverse proxy headers in containerized environments.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.Authority = "https://test.examr.xyz";

                jwtBearerOptions.RequireHttpsMetadata = false;

                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddRoutingCore();

            builder.Services.AddSingleton<IRepository, Repository>();

            builder.Services.AddSingleton<IUserContextProvider, UserContextProvider>();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            app.UseForwardedHeaders();

            app.UseCors("AllowAll");

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapGet("/protected/data", async ([FromServices] IRepository repository) =>
            {
                var loanProposal = await repository.GetLoanProposal();
                return Results.Ok(loanProposal);
            }).RequireAuthorization();

            app.MapPost("/protected/submit", async ([FromServices] IRepository repository) =>
            {
                await repository.SaveLoanProposal();
                return Results.Ok("Success");
            }).RequireAuthorization();

            app.MapPost("/tenants/update", async (HttpContext context) =>
            {
                BuildCache(await context.Request.ReadFromJsonAsync<Tenant[]>());
                return Results.Ok();
            });

            app.Run();
        }
    }
}
