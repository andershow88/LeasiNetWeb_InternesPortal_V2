using System.Security.Cryptography;
using System.Text;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;

    public AuthService(IApplicationDbContext db) => _db = db;

    public async Task<Benutzer?> ValidateAsync(string benutzername, string passwort)
    {
        var benutzer = await _db.Benutzer
            .Include(b => b.Leasinggesellschaft)
            .FirstOrDefaultAsync(b => b.Benutzername == benutzername && b.IstAktiv);

        if (benutzer is null) return null;
        return VerifyPasswort(passwort, benutzer.PasswortHash) ? benutzer : null;
    }

    public string HashPasswort(string passwort)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(passwort));
        return Convert.ToHexString(bytes).ToLower();
    }

    public bool VerifyPasswort(string passwort, string hash)
        => HashPasswort(passwort) == hash;
}
