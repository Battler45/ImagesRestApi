using System;
using System.Net.Http;
using AutoMapper;
using ImagesRestApi.Databases.Images;
using ImagesRestApi.Repositories;
using ImagesRestApi.Repositories.Interfaces;
using ImagesRestApi.Services;
using ImagesRestApi.Services.Interfaces;
using ImagesRestApi.Wrappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;

namespace ImagesRestApi
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
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



            


            //TODO: services.AddDbContextPool<>()
            services.AddDbContext<ImagesContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("ImagesDB");
                options.UseSqlServer(connectionString);
                //options.UseInMemoryDatabase("Images");
            });

            #region services

            services.AddScoped<IImagesRepository, ImagesRepository>();
            services.AddScoped<IImagesStorageService, ImagesStorageService>();
            services.AddScoped<IImagesService, ImagesService>();
            services.AddHttpClient<IUploader, Uploader>()
                .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(600)))
                //i cannot add it here, cause my service gets requests from different sites
                //but it's really useful, so i added here that comment to not to forget that option
                //.AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true
                });
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost";
            });
            #endregion services

            #region helpers

            services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();
            #endregion

            //services.AddScoped<IRedisClient>(s => new RedisClient(GetEnvironmentValue("redis:connection")));

            #region wrappers
            services.AddSingleton<IDirectoryWrapper, DirectoryWrapper>();
            services.AddSingleton<IFileWrapper, FileWrapper>();
            services.AddSingleton<IPathWrapper, PathWrapper>();
            services.AddSingleton<IContentDispositionHeaderValueWrapper, ContentDispositionHeaderValueWrapper>();
            #endregion wrappers


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            //app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
