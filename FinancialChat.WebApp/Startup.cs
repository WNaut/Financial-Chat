using FinancialChat.Core.Contracts;
using FinancialChat.Core.Helpers;
using FinancialChat.Jwt.Constants;
using FinancialChat.Jwt.Helpers;
using FinancialChat.Jwt.Managers;
using FinancialChat.Messaging.Hubs;
using FinancialChat.Messaging.Receivers;
using FinancialChat.Messaging.Senders;
using FinancialChat.Persistence;
using FinancialChat.Persistence.Repositories;
using FinancialChat.WebApp.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SpaServices.AngularCli;

namespace FinancialChat.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private const string ConnectionStringName = "DefaultConnection";
        private const string RabbitConfigKey = "RabbitConfiguration";
        private const string JwtConfigKey = "JwtTokenConfig";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddAuthorization();

            services.AddDbContext<FinancialChatDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString(ConnectionStringName)));

            services.AddSignalR(hubOptions =>
            {
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(30);
            }).AddJsonProtocol();

            services.AddAutoMapper(typeof(Startup));

            ConfigureRabbit(services);
            AddServices(services);
            AddManagers(services);
            ConfigureJwt(services);

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        private void ConfigureRabbit(IServiceCollection services)
        {
            var serviceClientSettingsConfig = Configuration.GetSection(RabbitConfigKey).Get<RabbitConfiguration>();
            services.AddSingleton(serviceClientSettingsConfig);
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IMessageSender, ChatMessageSender>();
            services.AddHostedService<ChatMessageReceiver>();
        }

        private void AddManagers(IServiceCollection services)
        {
            services.AddSingleton<IJwtAuthManager, JwtAuthManager>();
        }

        private void ConfigureJwt(IServiceCollection services)
        {
            var jwtTokenConfig = Configuration.GetSection(JwtConfigKey).Get<JwtTokenConfig>();
            services.AddSingleton(jwtTokenConfig);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtTokenConfig.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenConfig.Secret)),
                    ValidAudience = jwtTokenConfig.Audience,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // Configure Authorization for SignalR hub
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query[JwtConstants.TokenName];
                        
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub/chat"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            AddExceptionHandlerMiddleware(app);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                endpoints.MapHub<ChatHub>("/hub/chat");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer("start");
                }
            });
        }

        private void AddExceptionHandlerMiddleware(IApplicationBuilder app)
        {
            app.UseExceptionHandler(c => c.Run(async context =>
            {
                Exception exception = context.Features
                    .Get<IExceptionHandlerPathFeature>()
                    .Error;

                context.Response.ContentType = "application/json";

                string result = JsonSerializer.Serialize(new BaseResponse(exception.Message));
                await context.Response.WriteAsync(result);
            }));
        }
    }
}
