using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoteTaking.Api.Model;

public class UserVerification
{
    public Guid Id {get; set;}
    public string? Token {get; set;}
    public DateTime CreatedOn {get; set;}
    public DateTime ExpiresOn {get; set;}

    [Required]
    public string? UserID {get; set;}
    [ForeignKey(nameof(UserID))]
    public User user {get; set;}
}
