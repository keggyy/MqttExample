using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet.AspNetCore;
using MQTTnet.Diagnostics;
using NLog.Extensions.Logging;
using NLog.Web;

namespace MQTTServer
{
    public class Startup
    {
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            this.loggerFactory = loggerFactory;
            loggerFactory.AddConsole(LogLevel.Trace, true);
            logger = loggerFactory.CreateLogger(typeof(Startup));

            MqttNetGlobalLogger.LogMessagePublished += (s, e) =>
            {
                var trace = $">> [{e.TraceMessage.Timestamp:O}] [{e.TraceMessage.ThreadId}] [{e.TraceMessage.Source}] [{e.TraceMessage.Level}]: {e.TraceMessage.Message}";
                if (e.TraceMessage.Exception != null)
                {
                    trace += Environment.NewLine + e.TraceMessage.Exception.ToString();
                    logger.LogError(trace);
                }
                else
                {
                    logger.LogInformation(trace);
                }
            };
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // MQTT Configuration
            services.AddHostedMqttServer(builder =>
             builder.WithDefaultEndpointPort(1883)
            .WithConnectionValidator(x =>
            {
                x.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
            }).WithSubscriptionInterceptor(x =>
            {
                x.AcceptSubscription = true;
            }).WithApplicationMessageInterceptor(x =>
            {
                x.AcceptPublish = true;
                var t = x.ApplicationMessage;
            })


            );

            services.AddMqttConnectionHandler();
            //services.AddMqttWebSocketServerAdapter();
            services.AddMqttTcpServerAdapter();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            env.ConfigureNLog($"./conf/Nlog.{env.EnvironmentName}.config.xml");
            loggerFactory.AddNLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
