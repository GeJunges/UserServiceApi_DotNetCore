using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using UserServices.Repositories;
using UserServices.Entities;
using UserServicesDotNetCore.Entities;
using UserServices.Models;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace UserServices {
    public class Startup {
        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            // This section adds the database context using the default connection from the appsettings.json file
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Development")));


            // Add framework services.
            services.AddMvc(setupAction => {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            });

            //DI
            services.AddTransient<IUserServiceRepository, UserServiceRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, AppDbContext context) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment()) {
                // In development mode, you will see a detailed error description
                app.UseDeveloperExceptionPage();

                // Seed the database with the inital data
                DbInitializer.Seed(context);

            }
            else {
                // In production
                app.UseExceptionHandler(appBuilder => {
                    appBuilder.Run(async res => {
                        res.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await res.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            AutoMapper.Mapper.Initialize(cfg => {
                cfg.CreateMap<UserEntity, User>();
                cfg.CreateMap<User, UserEntity>();
            });

            app.UseMvc();
        }
    }
}
