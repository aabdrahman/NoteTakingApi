using System;

namespace NoteTaking.Api.Interfaces;

public interface IPasswordHasher
{
    public string Hash(string password);

    public bool Verify(string password, string HashPassword);
}
