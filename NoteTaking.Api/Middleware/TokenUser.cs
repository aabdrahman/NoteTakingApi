namespace NoteTaking.Api.Middleware;

public class TokenUser
{
    public required string UserName {get; set;}
    public required string Password {get; set;}
}