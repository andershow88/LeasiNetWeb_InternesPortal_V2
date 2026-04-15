using LeasiNetWeb.Domain.Entities;

namespace LeasiNetWeb.Application.Interfaces;

public interface IAuthService
{
    Task<Benutzer?> ValidateAsync(string benutzername, string passwort);
    string HashPasswort(string passwort);
    bool VerifyPasswort(string passwort, string hash);
}
