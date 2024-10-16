using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.DatabaseConfigurations;

public class UserVerificationConfig : IEntityTypeConfiguration<UserVerification>
{
    public void Configure(EntityTypeBuilder<UserVerification> entity)
    {
        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.user)
            .WithMany(e => e.UserTokenVerifications)
            .HasForeignKey(e => e.UserID)
            .HasPrincipalKey(e => e.UserIdentificationNumber)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
