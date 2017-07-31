using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using UserServices.Repositories;
using UserServices.Entities;
using System.Net;
using Microsoft.AspNetCore.Http;
using UserServices.Middlewares;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using AutoMapper;
using UserServices.Helpers;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;

namespace UserServices {
    public class Startup {
        private IConfigurationRoot _configuration { get; }

        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddSingleton(_configuration);


            // This section adds the database context using the default connection from the appsettings.json file
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("Development")));

            services.Configure<FormOptions>(options => options.BufferBody = true);

            //DI
            services.AddTransient<IUserServiceRepository, UserServiceRepository>();
            services.AddTransient<IPasswordVerify, PasswordVerify>();
            services.AddTransient<ITokenService, TokenService>();

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options => {
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddAutoMapper();
            services.AddMvc(opt => {/**opt.Filters.Add(new RequireHttpsAttribute());**/
            })
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, AppDbContext context) {
            loggerFactory.AddConsole(LogLevel.Error);
            loggerFactory.AddConsole(LogLevel.Information);

            app.UseRequestLogger();

            //app.UseGoogleAuthentication(new GoogleOptions() {
            //    ClientId = "asdasd3d32d3423",
            //    ClientSecret = "zxczxc32d3"
            //});

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

            app.UseJwtBearerAuthentication(new JwtBearerOptions {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWTSecurity:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWTSecurity:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSecurity:SecretKey"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }
            });

            app.UseMvc();
        }
    }
}
