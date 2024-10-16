using System;
using System.ComponentModel.DataAnnotations;

namespace NoteTaking.Api.Model;

public class User
{
    [Key]
    public Guid UserId {get; set;}

    [Required]
    //[RegularExpression(@"^[a-zA-Z''-'\s]", ErrorMessage = "Username should not contain special characters and must not be more than 18 characters.")]
    public required string UserName {get; set;}

    [Required]
    //[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,12}$", ErrorMessage = "Your password is expected to contain special character, alphabet and number.Password must not be more than 12 character.")]
    public required string UserPassword {get; set;}

    public DateTime DeleteDate { get; set; }

    public string? UserEmail {get; set;}
    public bool EmailVerified {get; set;} = false;

    [Required]
    [Length(6,6)]
    public required string UserIdentificationNumber {get; set;}

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedDate {get; set;}

    public ICollection<Note>? UserNotes {get; set;}
    public ICollection<UserVerification>? UserTokenVerifications {get; set;}

}
