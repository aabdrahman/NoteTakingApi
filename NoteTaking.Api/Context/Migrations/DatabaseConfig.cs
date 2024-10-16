using System;
using Microsoft.EntityFrameworkCore;

namespace NoteTaking.Api.Context.Migrations;

public static class DatabaseConfig
{
    public static void MigrateDatabase(this WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var DbSettings = scope.ServiceProvider.GetRequiredService<NoteTakingDbContext>();

        DbSettings.Database.Migrate();
    }
}
