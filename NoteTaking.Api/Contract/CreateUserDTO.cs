using System.ComponentModel.DataAnnotations;
using NoteTaking.Api.Validations;

namespace NoteTaking.Api.Context;

public record class CreateUserDTO
(
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9'\-\s]{6,12}$", ErrorMessage = "Username should not contain special characters and must not be more than 12 characters.")]
    string UserName,

    [Required]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&#]{8,16}$", ErrorMessage = "Your password is expected to contain special character, alphabet and number.Password must not be more than 12 character.")]
    string Password,

    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Provided.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$", ErrorMessage = "Pattern not matched.")]
    string UserEmail
);
