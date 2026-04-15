using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.Interfaces;

public interface IAnhangService
{
    Task<AnhangDto> HochladenAsync(Stream dateiStream, string dateiname, string contentType, long dateigroesse,
        AnhangTyp typ, int hochgeladenVonId,
        int? antragId = null, int? kommentarId = null, int? pruefungId = null, int? vertragId = null);
    Task<(byte[] Inhalt, string ContentType, string Dateiname)> HerunterladenAsync(int anhangId);
    Task LoeschenAsync(int anhangId, int benutzerId);
    Task<IEnumerable<AnhangDto>> GetAnhaengeAsync(int? antragId = null, int? kommentarId = null, int? pruefungId = null);
}
