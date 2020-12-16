using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CloudNine.Core.Database;

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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
            services.AddTransient<SlashCommandHandlingService>()
                .AddDbContext<CloudNineDatabaseModel>(ServiceLifetime.Transient, ServiceLifetime.Scoped)
                .AddSingleton<HttpClient>()
                .AddHttpClient("discord", x =>
                {
                    x.DefaultRequestHeaders.Authorization = new("Bot", Program.DiscordConfig.Value.Token);
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SlashCommandHandlingService slashCommands)
        {
            // Startup the slash commands.

            // Register the executing aseembly as the assembly with slash commands.
            slashCommands.WithCommandAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            // This runs after the bot is started, so we know these values will be loaded.
            // Start the slash command service.
            slashCommands.Start(Program.DiscordConfig.Value.Token, Program.Discord.Client.CurrentApplication.Id).GetAwaiter().GetResult();

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
        }
    }
}
