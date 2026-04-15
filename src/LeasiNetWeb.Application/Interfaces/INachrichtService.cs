using LeasiNetWeb.Application.DTOs;

namespace LeasiNetWeb.Application.Interfaces;

public interface INachrichtService
{
    Task<IEnumerable<NachrichtDto>> GetPosteingang(int benutzerId);
    Task<int> UngeleseneAnzahl(int benutzerId);
    Task<NachrichtDto?> GetNachricht(int id, int benutzerId);
    Task<int> Senden(NachrichtSendenDto dto, int absenderId);
    Task AlsGelesenMarkieren(int nachrichtId, int benutzerId);
}
