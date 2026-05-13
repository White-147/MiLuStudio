namespace MiLuStudio.Infrastructure.Auth;

using global::System.Security.Cryptography;
using global::System.Text;
using MiLuStudio.Application.Abstractions;

public sealed class LocalAuthTokenService : IAuthTokenService
{
    public string CreateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public string HashToken(string token)
    {
        return HashSecret(token);
    }

    public string HashSecret(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
