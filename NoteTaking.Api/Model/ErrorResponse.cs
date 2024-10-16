namespace NoteTaking.Api.Model;

public class ErrorResponse
{
    public string? Title {get; set;}
    public required string ErrorMessage {get; set;}
}