using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CloudNine.Core.Database;

using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CloudNine
{
    public class Startup
    {
        public static DiscordSlashClient SlashClient { get; private set; }
        public static string PublicKey { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            PublicKey = Configuration["PublicKey"];
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CloudNine", Version = "v1" });
            });
            services.AddDbContext<CloudNineDatabaseModel>(ServiceLifetime.Transient, ServiceLifetime.Scoped)
                .AddSingleton<HttpClient>();

            SlashClient = new DiscordSlashClient(new DiscordSlashConfiguration()
            {
                ClientId = Program.Discord.Client.CurrentApplication.Id,
                Token = Program.DiscordConfig.Value.Token,
                DefaultResponseType = DSharpPlus.SlashCommands.Enums.InteractionResponseType.ACKWithSource
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudNine v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SlashClient.StartAsync().GetAwaiter().GetResult();
        }
    }
}
