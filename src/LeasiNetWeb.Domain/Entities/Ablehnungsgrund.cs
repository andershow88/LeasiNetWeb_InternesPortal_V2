namespace LeasiNetWeb.Domain.Entities;

public class Ablehnungsgrund : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Bezeichnung { get; set; } = string.Empty;
    public bool IstAktiv { get; set; } = true;

    public ICollection<Leasingantrag> Leasingantraege { get; set; } = [];
}
