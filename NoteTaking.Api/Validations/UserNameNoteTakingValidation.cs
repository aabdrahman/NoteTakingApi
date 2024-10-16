using System;
using System.ComponentModel.DataAnnotations;

namespace NoteTaking.Api.Validations;

public class UserNameNoteTakingValidation : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var username = value as string;

        if (username!.Length > 12 || username.Length < 1)
            return new ValidationResult("Username must be between 1 and 12 characters", new [] {validationContext.MemberName}!);
    	
        return ValidationResult.Success;
    }
}
