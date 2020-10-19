﻿using AutoMapper;
using Demo.Contracts.Database;
using Demo.Contracts.RabbitMQ;
using Demo.Contracts.Repository;
using Demo.Infra.Data;
using Demo.Infra.Mappings;
using Demo.Infra.RabbitMQ;
using Demo.Infra.RabbitMQ.HostedServices;
using Demo.Infra.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.API
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
            MongoDBPersistence.Setup();

            services.AddHostedService<ConsumerHostedService>();

            services.AddAutoMapper(typeof(Startup));
            
            services.AddSingleton<IMongoDBContext, MongoDBContext>();                        
            services.AddSingleton<IRepositoryResearch, RespositoryResearch>();            
            services.AddScoped<IQueueManagementResearch, QueueManagementResearch>();
            services.AddScoped<ISetupConnection, SetupConnection>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
