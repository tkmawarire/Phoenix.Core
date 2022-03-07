﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mix.MixQuartz.Jobs;
using Mix.MixQuartz.Models;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mix.MixQuartz.Extensions;
using Mix.MixQuartz.Helpers;
using Mix.Shared.Services;

namespace Mix.MixQuartz
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMixQuartzServices(this IServiceCollection services, IConfiguration configuration)
        {
            // base configuration from appsettings.json
            services.Configure<QuartzOptions>(configuration.GetSection("Quartz"));

            services.AddQuartz(q =>
            {
                // we could leave DI configuration intact and then jobs need
                // to have public no-arg constructor
                // the MS DI is expected to produce transient job instances
                // this WONT'T work with scoped services like EF Core's DbContext
                q.UseMicrosoftDependencyInjectionJobFactory();

                // or for scoped service support like EF Core DbContext
                q.UseMicrosoftDependencyInjectionScopedJobFactory();

                // these are the defaults
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 10;
                });

                if (!GlobalConfigService.Instance.AppSettings.IsInit)
                {
                    q.AddMixQuartzJobsAsync().GetAwaiter().GetResult();
                }
            });
            // ASP.NET Core hosting
            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
            return services;
        }


        private static async Task<IServiceCollectionQuartzConfigurator> AddMixQuartzJobsAsync(this IServiceCollectionQuartzConfigurator quartzConfiguration)
        {
            List<MixJobModel> jobConfiguraions = MixQuartzHelper.LoadJobConfiguraions();
            var assembly = Assembly.GetExecutingAssembly();
            var mixJobs = assembly
                .GetExportedTypes()
                .Where(m => m.BaseType.Name == typeof(BaseJob).Name);
            StdSchedulerFactory factory = new();
            IScheduler scheduler = await factory.GetScheduler();
            foreach (var job in mixJobs)
            {
                var jobConfig = jobConfiguraions.FirstOrDefault(j => j.JobType == job);
                if (jobConfig == null)
                {
                    jobConfig = GetDefaultJob(job);
                }

                var jobKey = new JobKey(jobConfig.Key, jobConfig.Group);
                Action<IJobConfigurator> jobConfigurator = j => j.WithDescription(jobConfig.Description);

                var applyGenericMethod = typeof(Quartz.ServiceCollectionExtensions)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .FirstOrDefault(m => m.Name == nameof(Quartz.ServiceCollectionExtensions.AddJob) &&  m.GetParameters()[1].ParameterType == typeof(JobKey));
                var parameters = applyGenericMethod.GetParameters();
                var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(jobConfig.JobType);
                applyConcreteMethod.Invoke(quartzConfiguration, new object[] { quartzConfiguration, jobKey, jobConfigurator });

                quartzConfiguration.AddTrigger(t => t
                        .WithIdentity("trigger_" + jobConfig.Key, jobConfig.Group)
                        .ForJob(jobKey)
                        .StartNowIf(jobConfig.Trigger.IsStartNow)
                        .StartAtIf(jobConfig.Trigger.StartAt.HasValue, jobConfig.Trigger.StartAt.Value)
                        .WithMixSchedule(jobConfig.Trigger.Interval, jobConfig.Trigger.IntervalType, jobConfig.Trigger.RepeatCount)
                        .WithDescription(jobConfig.Description));
            }

            return quartzConfiguration;
        }

        private static MixJobModel GetDefaultJob(Type job)
        {
            return new MixJobModel()
            {
                Key = job.Name,
                Group = null,
                Description = null,
                JobType = job
            };
        }

        public static IApplicationBuilder UseMixScheduler(this IApplicationBuilder app)
        {
            return app;
        }
    }
}