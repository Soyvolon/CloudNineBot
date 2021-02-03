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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using DSharpPlus.SlashCommands;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using CloudNine.Core.Configuration;

namespace CloudNine.Web
{
    public class Startup
    {
        public DiscordShardedClient Client { get; private set; }
        public static DiscordSlashClient SlashClient { get; private set; }
        public static string PublicKey { get; private set; }
        public static InfinityConfiguration InfinityConfig { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            PublicKey = Configuration["PublicKey"];
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            using (FileStream fs = new FileStream(Path.Join("Config", "infinity_config.json"), FileMode.Open))
            {
                using StreamReader sr = new(fs);
                var ison = sr.ReadToEnd();
                InfinityConfig = JsonConvert.DeserializeObject<InfinityConfiguration>(ison);
            }

            string json = "";
            using (FileStream fs = new FileStream(Path.Join("Config", "bot_config.json"), FileMode.Open))
            {
                using StreamReader sr = new StreamReader(fs);
                json = sr.ReadToEnd();
            }

            var botCfg = JsonConvert.DeserializeObject<DiscordBotConfiguration>(json);

            Client = new DiscordShardedClient(GetDiscordConfiguration(botCfg.Token));

            Client.StartAsync().GetAwaiter().GetResult();

            services.AddScoped<LoginService>()
                .AddSingleton(x => new LoginManager(Client, botCfg.Secret, InfinityConfig.AuthorizedUsers.ToHashSet()))
                .AddLogging(o => o.AddConsole())
                .AddDbContext<CloudNineDatabaseModel>(ServiceLifetime.Transient, ServiceLifetime.Scoped)
                .AddHttpContextAccessor()
                .AddHttpClient()
                .AddScoped<HttpClient>()
                .AddTransient<StateManager>()
                .Configure<CookiePolicyOptions>(options =>
                {
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                })
                .AddScoped<IRefreshRequestService, RefreshRequestService>();

            services.AddControllers();
            services.AddRazorPages().AddRazorPagesOptions(o =>
            {
                o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            });
            services.AddServerSideBlazor();

            IServiceCollection c = new ServiceCollection()
                .AddDbContext<CloudNineDatabaseModel>(ServiceLifetime.Transient, ServiceLifetime.Scoped);

            SlashClient = new DiscordSlashClient(new DiscordSlashConfiguration()
            {
                ShardedClient = Client,
                Token = botCfg.Token,
                DefaultResponseType = DSharpPlus.SlashCommands.Enums.InteractionResponseType.ACKWithSource
            }, c);

            SlashClient.RegisterCommands(Assembly.GetExecutingAssembly());
        }

        public DiscordConfiguration GetDiscordConfiguration(string botToken)
        {
            var dcfg = new DiscordConfiguration()
            {
                TokenType = TokenType.Bot,
                Token = botToken
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
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });

            SlashClient.StartAsync().GetAwaiter().GetResult();
        }
    }
}
