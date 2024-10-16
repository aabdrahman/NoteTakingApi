using System;
using System.Globalization;
using NoteTaking.Api.Services;
using Quartz;

namespace NoteTaking.Api.Middleware;

[DisallowConcurrentExecution]
internal static class BackgroundService
{
    internal static IServiceCollection AddQuartzService(this IServiceCollection services)
    {
        var NoteJobKey = JobKey.Create(nameof(NoteBackgroundJob));
        var UserJobKey = JobKey.Create(nameof(UserBackgroundJob));
        
        services.AddQuartz(options => {
            options.AddJob<NoteBackgroundJob>(NoteJobKey)
                    .AddTrigger(trigger => {
                        trigger.ForJob(NoteJobKey)
                            .StartAt(DateBuilder.DateOf(07, 40, 0))
                            .WithSimpleSchedule(schedule => {
                                    schedule.WithIntervalInHours(24).RepeatForever();
                            })
                            //.EndAt(DateBuilder.DateOf(09, 00, 0))
                            ;
                            
                    });
        });

        services.AddQuartz(options => {
            options.AddJob<UserBackgroundJob>(UserJobKey)
                    .AddTrigger(trigger => {
                        trigger.ForJob(UserJobKey)
                        .StartAt(DateBuilder.DateOf(16, 30, 0))
                        .WithSimpleSchedule(schedule => {
                            schedule.WithIntervalInHours(24).RepeatForever();
                        });
                    });
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    } 
}
