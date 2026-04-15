namespace LeasiNetWeb.Domain.Entities;

public class Leasinggesellschaft : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Kurzbezeichnung { get; set; }
    public string? Strasse { get; set; }
    public string? PLZ { get; set; }
    public string? Ort { get; set; }
    public string? Land { get; set; } = "DE";
    public string? Telefon { get; set; }
    public string? EMail { get; set; }
    public string? Ansprechpartner { get; set; }
    public bool IstAktiv { get; set; } = true;
    public decimal ObligoLimit { get; set; }

    // Navigation
    public ICollection<Benutzer> Benutzer { get; set; } = [];
    public ICollection<Leasingantrag> Leasingantraege { get; set; } = [];
    public ICollection<LgRegistrierung> Registrierungen { get; set; } = [];
}
