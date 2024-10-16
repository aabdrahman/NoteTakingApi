using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.DatabaseConfigurations;

public class NoteConfig : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> entity)
    {
        entity.HasOne(n => n.user)
            .WithMany(n => n.UserNotes)
            .HasForeignKey(u => u.UserIdentificationNumber)
            .HasPrincipalKey(u => u.UserIdentificationNumber)
            .OnDelete(DeleteBehavior.Cascade);
        
        entity.HasQueryFilter(n => !n.IsDeleted);

        entity.Property(n => n.CreatedDate)
             .HasDefaultValueSql("getdate()");

    }
}
