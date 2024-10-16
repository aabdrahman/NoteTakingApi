using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using NoteTaking.Api.DatabaseConfigurations;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.Context;

public class NoteTakingDbContext : DbContext
{
    //Declare fields for connection string
    private readonly DbSettings _dbSettings;
    //private readonly DatabaseEnum _databaseEnum;

    //Constructor that injects the Connection string from DbSettings class
    public NoteTakingDbContext(IOptions<DbSettings> dbSettings, DbContextOptions<NoteTakingDbContext> options) : base(options)
    {
        _dbSettings = dbSettings.Value;
    }


    //Declare all the tables properties
    public DbSet<User> Users {get; set;} 
    public DbSet<Note> Notes {get; set;}
    public DbSet<UserVerification> UserVerificationTokens {get; set;}

    public DbSet<UserSummary> UserSummary {get; set;}

    //Manage Connection
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_dbSettings.ConnectionString);
    }

    //Coniguring table using Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Configuration for users and its relationship and idex column
        //modelBuilder.Entity<User>()
        //                .HasQueryFilter(u => !u.IsDeleted)
        //                .HasIndex(u => u.UserIdentificationNumber)
        //                .IsUnique();

       // modelBuilder.Entity<Note>()
       //                     .HasQueryFilter(n => !n.IsDeleted)
       //                     .HasOne(n => n.user)
       //                     .WithMany(n => n.UserNotes)
       //                     .HasForeignKey(n => n.UserIdentificationNumber)
       //                     .HasPrincipalKey(n => n.UserIdentificationNumber)
       //                     .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.ApplyConfiguration(new UserConfig());
        modelBuilder.ApplyConfiguration(new NoteConfig());
        modelBuilder.ApplyConfiguration(new UserVerificationConfig());

        modelBuilder.Entity<UserSummary>()
                    .HasNoKey()
                    .ToView("UserNoteSummaryView");

    }

}   
