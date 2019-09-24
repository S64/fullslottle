using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S64.Bot.Builder.Adapters.Slack;
using S64.Bot.Builder.Adapters.Slack.AspNetCore;

namespace FullSlottle
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.AddSlackBot<FullSlottleBot>(options =>
            {
                options.Middleware = new List<IMiddleware> { new SlackMessageTypeMiddleware() };
                options.SlackOptions = Program.Options;
                options.Paths = new SlackBotPaths
                {
                    BasePath = "/api",
                    RequestPath = "events",
                };
                options.VerificationToken = Program.VerificationToken;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseSlack();
        }
    }
}
