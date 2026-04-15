using LeasiNetWeb.Application.DTOs;

namespace LeasiNetWeb.Application.Interfaces;

public interface IAdminService
{
    // ── Benutzer ──────────────────────────────────────────────────────────────
    Task<IEnumerable<BenutzerListeDto>> GetBenutzerAsync();
    Task<BenutzerDetailDto?> GetBenutzerByIdAsync(int id);
    Task<BenutzerDetailDto> BenutzerErstellenAsync(BenutzerErstellenDto dto);
    Task BenutzerBearbeitenAsync(BenutzerBearbeitenDto dto);
    Task BenutzerDeaktivierenAsync(int id);
    Task BenutzerAktivierenAsync(int id);

    // ── Leasinggesellschaften ─────────────────────────────────────────────────
    Task<IEnumerable<LeasinggesellschaftListeDto>> GetLeasinggesellschaftenAsync();
    Task<LeasinggesellschaftDetailDto?> GetLeasinggesellschaftByIdAsync(int id);
    Task<LeasinggesellschaftDetailDto> LeasinggesellschaftErstellenAsync(LeasinggesellschaftDetailDto dto);
    Task LeasinggesellschaftBearbeitenAsync(LeasinggesellschaftDetailDto dto);

    // ── Ablehnungsgründe ──────────────────────────────────────────────────────
    Task<IEnumerable<AblehnungsgrundDto>> GetAblehnungsgruendeAsync();
    Task AblehnungsgrundSpeichernAsync(AblehnungsgrundDto dto);  // create or update
    Task AblehnungsgrundLoeschenAsync(int id);

    // ── Gerätetypen ───────────────────────────────────────────────────────────
    Task<IEnumerable<GeraetetypDto>> GetGeraetetypenAsync();
    Task GeraetetypSpeichernAsync(GeraetetypDto dto);            // create or update
    Task GeraetetypLoeschenAsync(int id);
}
