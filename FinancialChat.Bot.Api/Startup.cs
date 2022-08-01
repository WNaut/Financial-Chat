using FinancialChat.Bot.Receivers;
using FinancialChat.Bot.Senders;
using FinancialChat.Core.Contracts;
using FinancialChat.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FinancialChat.Bot.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private const string RabbitConfigKey = "RabbitConfiguration";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            ConfigureRabbit(services);
            AddServices(services);
        }

        private void ConfigureRabbit(IServiceCollection services)
        {
            var serviceClientSettingsConfig = Configuration.GetSection(RabbitConfigKey).Get<RabbitConfiguration>();
            services.AddSingleton(serviceClientSettingsConfig);
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<IMessageSender, BotMessageSender>();
            services.AddHostedService<BotMessageReceiver>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
