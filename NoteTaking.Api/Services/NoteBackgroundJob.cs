using System;
using NoteTaking.Api.Interfaces;
using Quartz;

namespace NoteTaking.Api.Services;

internal class NoteBackgroundJob : IJob
{
    //private readonly INoteTakingServices _notetakingServices;
    private readonly ILogger<NoteBackgroundJob> _logger;

    private readonly IServiceProvider _services;

    public NoteBackgroundJob(ILogger<NoteBackgroundJob> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        
        using(var scope = _services.CreateScope())
        {
            var noteTakingServices = scope.ServiceProvider.GetRequiredService<INoteTakingServices>();
            var NotesToDelete = noteTakingServices.GetNotesToDelete();

            if(NotesToDelete.Count() == 0)
            {
                _logger.LogInformation("No Notes Found for Today.");
            }

            foreach(var note in NotesToDelete)
            {
                await noteTakingServices.RemoveNoteViaBackgroundService(note.Id);
                _logger.LogInformation($"Note deleted for user: {note.Id}");
            }
        }
        
        //var UsersToDelete = _notetakingServices.GetUsersToDelete();
    }
}
