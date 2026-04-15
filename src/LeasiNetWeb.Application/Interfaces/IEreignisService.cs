using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.Interfaces;

public interface IEreignisService
{
    Task EreignisAufzeichnen(int antragId, EreignisTyp typ, int ausgeloestVonId, string? beschreibung = null, object? nutzlast = null);
    Task BenachrichtigungenVersenden(int ereignisId);
}
