using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Domain.Entities;

public class Benutzer : BaseEntity
{
    public string Benutzername { get; set; } = string.Empty;
    public string PasswortHash { get; set; } = string.Empty;
    public string Vorname { get; set; } = string.Empty;
    public string Nachname { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public BenutzerRolle Rolle { get; set; }
    public bool IstAktiv { get; set; } = true;

    // Optional: linked to a leasing company (for LG-role users)
    public int? LeasinggesellschaftId { get; set; }
    public Leasinggesellschaft? Leasinggesellschaft { get; set; }

    // Navigation
    public ICollection<Leasingantrag> EingereichteLeasingantraege { get; set; } = [];
    public ICollection<Leasingantrag> ZugewieseneAntraegeMB { get; set; } = [];
    public ICollection<Leasingantrag> ZugewieseneAntraegeLG { get; set; } = [];
    public ICollection<Kommentar> Kommentare { get; set; } = [];
    public ICollection<Nachricht> GesendeteNachrichten { get; set; } = [];

    public string Anzeigename => $"{Vorname} {Nachname}".Trim();
}
