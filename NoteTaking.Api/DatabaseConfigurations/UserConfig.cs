using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.DatabaseConfigurations;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasIndex(u => u.UserIdentificationNumber)
              .IsUnique();
        
        entity.HasQueryFilter(u => !u.IsDeleted);

        entity.Property(q => q.CreatedDate)
              .HasDefaultValueSql("getdate()");
    }
}
