﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


using QuartzNetDemo4.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Quartz;
using QuartzNetDemo4.Model.DataModel;

namespace QuartzNetDemo4
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {

            //var cronJobs = new Dictionary<string, string>();
            var cronJobs = new List<CronMethod>();
            Configuration.GetSection("CronJob").Bind(cronJobs);
            
            services.AddSingleton(cronJobs);


            //connection注放
            services.AddTransient<NpgsqlConnection, NpgsqlConnection>();
            services.AddTransient<SqlConnection, SqlConnection>();
            //注入PostgreSqlDapperPlus类
            services.AddTransient<IPostgreSqlDapperPlusDB, PostgreSqlDapperPlusDB>();
            //注入SqlServerDapperPlus类
            services.AddTransient<ISqlServerDapperPlusDB, SqlServerDapperPlusDB>();

            services.AddTransient<IBackgroundRepository, BackgroundRepository>();

            services.UseQuartz(typeof(BackgroundJob));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IScheduler scheduler, List<CronMethod> cronJobs)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            foreach (var cronJob in cronJobs)
            {
                QuartzServicesUtilities.StartJob<BackgroundJob>(scheduler, cronJob.CronExpression,cronJob.MethodName);
            }

            app.UseMvc();
        }
    }
}