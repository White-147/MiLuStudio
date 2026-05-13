namespace MiLuStudio.Application.Abstractions;

public interface IAuthTokenService
{
    string CreateToken();

    string HashToken(string token);

    string HashSecret(string secret);
}
