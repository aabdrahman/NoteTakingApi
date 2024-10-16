using System;
using NoteTaking.Api.Context;

namespace NoteTaking.Api.Model;

public class DbSettings
{
    public required string ConnectionString {get; set;}
}
