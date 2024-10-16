using System.ComponentModel.DataAnnotations;

namespace NoteTaking.Api.Validations;

public class UpdatedDateNoteTakingValidation : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var DateString = value as string;
        DateTime ValeDate = DateTime.Parse(DateString!);

        if (ValeDate.Date > DateTime.Now.Date || ValeDate.Date < DateTime.Now.Date)
            return new ValidationResult("Date cannot be greater than or less than today", new [] {validationContext.MemberName}!);
    	
        return ValidationResult.Success;
    }
}