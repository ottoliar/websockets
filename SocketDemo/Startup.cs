using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebSocketManager;

namespace SocketDemo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddScoped<IWebSocketConnectionManager, WebSocketConnectionManager>();
            services.AddWebSocketManager();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            app.UseWebSockets();
            // ChatHandler is a WebSocketConnectionManager
            app.MapWebSocketManager("/chat", app.ApplicationServices.GetService<ChatHandler>());
            app.MapWebSocketManager("/OldSearch", app.ApplicationServices.GetService<OldSearchHandler>());
            app.MapWebSocketManager("/NewSearch", app.ApplicationServices.GetService<ImprovedSearchHandler>());

            app.UseStaticFiles();
        }
    }
}
