using LeasiNetWeb.Application.DTOs;

namespace LeasiNetWeb.Application.Interfaces;

public interface IKommentarService
{
    Task<IEnumerable<KommentarDto>> GetKommentare(int antragId, bool nurOeffentliche = false);
    Task<int> HinzufuegenAsync(KommentarErstellenDto dto, int autorId);
}
