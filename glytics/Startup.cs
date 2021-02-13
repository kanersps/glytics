using System;
using glytics.Data.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace glytics
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
            services.AddControllers()
                .AddNewtonsoftJson( options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "glytics", Version = "v1"}); });

            services.AddScoped<Authentication>().AddDbContext<GlyticsDbContext>(options =>
            {
                string connectionString = Environment.GetEnvironmentVariable("connection_string");
                if (string.IsNullOrEmpty(connectionString))
                    connectionString = Configuration.GetConnectionString("home");
                
                options
                    .UseMySql(connectionString,
                        new MariaDbServerVersion(new Version(10, 5, 8)),
                        mariadbOptions =>
                        {
                            mariadbOptions.CharSetBehavior(CharSetBehavior.NeverAppend);
                            mariadbOptions.MigrationsAssembly("glytics.Data");
                        });
            });

            services.AddDbContext<GlyticsDbContext>(options =>
            {
                string connectionString = Environment.GetEnvironmentVariable("connection_string");
                if (string.IsNullOrEmpty(connectionString))
                    connectionString = Configuration.GetConnectionString("home");
                
                options
                    .UseMySql(connectionString,
                        new MariaDbServerVersion(new Version(10, 5, 8)),
                        mariadbOptions =>
                        {
                            mariadbOptions.CharSetBehavior(CharSetBehavior.NeverAppend);
                            mariadbOptions.MigrationsAssembly("glytics.Data");
                        });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "glytics v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthorization();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials());

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}