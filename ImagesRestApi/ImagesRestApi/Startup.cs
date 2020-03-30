using AutoMapper;
using ImagesRestApi.Databases.Images;
using ImagesRestApi.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using ImagesRestApi.Repositories.Interfaces;
using ImagesRestApi.Services;
using ImagesRestApi.Services.Interfaces;
using ImagesRestApi.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace ImagesRestApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddDbContext<ImagesContext>(options => options.UseInMemoryDatabase("Images"));

            services.AddScoped<IImagesRepository, ImagesRepository>();

            services.AddScoped<IImagesService, ImagesService>();

            //wrappers
            services.AddTransient<IDirectoryWrapper, DirectoryWrapper>();
            services.AddTransient<IFileWrapper, FileWrapper>();
            services.AddTransient<IPathWrapper, PathWrapper>();
            services.AddTransient<IContentDispositionHeaderValueWrapper, ContentDispositionHeaderValueWrapper>();

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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
