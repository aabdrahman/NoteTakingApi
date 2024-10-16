using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoteTaking.Api.Model;

public class Note
{
    [Key]
    public int Id {get; set;}

    public string? UserNote {get; set;}

    public DateTime CreatedDate {get; set;}

    public bool IsDeleted {get; set;} = false;

    public DateTime UpdateDate {get; set;}

    public DateTime DeleteDate {get; set;}

    //Configuring Relationship

    [Required]
    public required string UserIdentificationNumber {get; set;}

    [ForeignKey(nameof(UserIdentificationNumber))]
    public User user {get; set;}

}
