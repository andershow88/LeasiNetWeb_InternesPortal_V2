namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Defines approval authority limits per user/role.
/// Maps to bm_lnw_self_competence.
/// </summary>
public class Selbstkompetenz : BaseEntity
{
    public int BenutzerId { get; set; }
    public Benutzer Benutzer { get; set; } = null!;

    public decimal MaxObligoBetrag { get; set; }
    public bool ZweiteVoteErforderlichAbBetrag { get; set; }
    public decimal? ZweiteVoteSchwellenwert { get; set; }
    public DateTime GueltigAb { get; set; }
    public DateTime? GueltigBis { get; set; }
}
