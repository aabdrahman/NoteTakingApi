using System;
using NoteTaking.Api.Interfaces;
using Quartz;

namespace NoteTaking.Api.Services;

[DisallowConcurrentExecution]
internal class UserBackgroundJob : IJob
{
    private readonly ILogger<UserBackgroundJob> _logger;
    private readonly IServiceProvider _services;

    //private readonly INoteTakingServices _noteTakingServices;

    public UserBackgroundJob(ILogger<UserBackgroundJob> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        using(var scope = _services.CreateScope())
        {
            var noteTakingServices = scope.ServiceProvider.GetRequiredService<INoteTakingServices>();

            var UsersToDelete = noteTakingServices.GetUsersToDelete();

            if(UsersToDelete.Count == 0)
            {
                _logger.LogInformation("No Users to delete");
            }

            foreach(var user in UsersToDelete)
            {
                await noteTakingServices.RemoveDeletedUser(user.UserID);
                _logger.LogInformation($"Deleted User with Id: {user.UserID}");
            }
        }
    }
}
