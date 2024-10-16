using System;

namespace NoteTaking.Api.Model;

public class UserSummary
{
    public required string UserID {get; set;}

    public int Number_Of_Notes {get; set;}
}
