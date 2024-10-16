using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Context;
using NoteTaking.Api.Controllers;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.Middleware;

public sealed class EmailVerificationLinkFactory(
    IHttpContextAccessor httpContextAccessor,
    LinkGenerator linkGenerator,
    NoteTakingDbContext context
)
{
    public string CreteToken(UserVerification userVerification)
    {
        string? VerificationLink = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext!, action: "VerifyUser", controller: "NoteTaking", new {token = userVerification.Id});

        return VerificationLink ?? throw new Exception("Could not generate verification link.");
    }

    public async Task<(bool IsSuccessful, string ResponseCode)> VerifyEmail(Guid token)
    {

        UserVerification? userToken = await context.UserVerificationTokens
                                                    .Include(u => u.user)
                                                    .IgnoreQueryFilters()
                                                    .FirstOrDefaultAsync(u => u.Id == token);
        
        if (userToken is null)
        {
            return (false, "90");
        }
        if(userToken.ExpiresOn < DateTime.UtcNow)
        {
            return (false, "99");
        }

        if(userToken.user.DeleteDate != DateTime.MinValue)
        {
            userToken.user.IsDeleted = false;
        }
            
        userToken.user.EmailVerified = true;
        context.UserVerificationTokens.Remove(userToken);

        await context.SaveChangesAsync();

        return (true, "00");
    }
}
