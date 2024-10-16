using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using NoteTaking.Api.Interfaces;

namespace NoteTaking.Api.Middleware;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    private readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return string.Concat(Convert.ToHexString(hash), "-", Convert.ToHexString(salt));
    }

    public bool Verify(string password, string HashPassword)
    {
        //byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        //byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        string[] PasswordParts = HashPassword.Split('-');
        byte[] hash = Convert.FromHexString(PasswordParts[0]);  
        byte[] salt = Convert.FromHexString(PasswordParts[1]);
        
        byte[] hashedPassword = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, hashedPassword);
    }

}

