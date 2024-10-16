using System;

namespace NoteTaking.Api.Model;

public class Response
{
    public required string ResponseCode {get; set;}

    public object? ResponseData {get; set;}

    public string? ResponseDescription {get; set;}

    public ErrorResponse? errorResponse {get; set;}

}
