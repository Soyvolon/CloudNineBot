using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Web.User;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using CloudNine.Web.State;
using DSharpPlus;
using CloudNine.Config.Bot;
using Newtonsoft.Json;
using System.IO;

namespace CloudNine.Web
{
    public class Startup
    {
        public DiscordRestClient Rest { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string json = "";
            using (FileStream fs = new FileStream("Config/bot_config.json", FileMode.Open))
            {
                using StreamReader sr = new StreamReader(fs);
                json = sr.ReadToEnd();
            }

            var botCfg = JsonConvert.DeserializeObject<DiscordBotConfiguration>(json);

            Rest = new DiscordRestClient(GetDiscordConfiguration(botCfg.Token));

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<LoginService>()
                .AddSingleton(x => new LoginManager(Rest, botCfg.Secret))
                .AddLogging(o => o.AddConsole())
                .AddDbContext<CloudNineDatabaseModel>()
                .AddHttpContextAccessor()
                .AddHttpClient()
                .AddScoped<HttpClient>()
                .AddTransient<StateManager>();
        }

        public DiscordConfiguration GetDiscordConfiguration(string botToken)
        {
            var dcfg = new DiscordConfiguration()
            {
                TokenType = TokenType.Bot,
                Token = botToken,
            };

            return dcfg;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
