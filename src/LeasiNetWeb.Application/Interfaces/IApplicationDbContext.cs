using Microsoft.EntityFrameworkCore;
using LeasiNetWeb.Domain.Entities;

namespace LeasiNetWeb.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Benutzer> Benutzer { get; }
    DbSet<Leasinggesellschaft> Leasinggesellschaften { get; }
    DbSet<Leasingantrag> Leasingantraege { get; }
    DbSet<Leasingobjekt> Leasingobjekte { get; }
    DbSet<InternePruefung> InternePruefungen { get; }
    DbSet<PruefungsPflicht> PruefungsPflichten { get; }
    DbSet<PruefungsSchritt> PruefungsSchritte { get; }
    DbSet<Vertrag> Vertraege { get; }
    DbSet<Kommentar> Kommentare { get; }
    DbSet<Anhang> Anhaenge { get; }
    DbSet<Nachricht> Nachrichten { get; }
    DbSet<Ereignis> Ereignisse { get; }
    DbSet<Ablehnungsgrund> Ablehnungsgruende { get; }
    DbSet<Vertragstyp> Vertragstypen { get; }
    DbSet<Geraetetyp> Geraetetypen { get; }
    DbSet<Rate> Raten { get; }
    DbSet<Ratentabelle> Ratentabellen { get; }
    DbSet<DokumentAustausch> DokumentAustausche { get; }
    DbSet<LgRegistrierung> LgRegistrierungen { get; }
    DbSet<Obligo> Obligos { get; }
    DbSet<SynchronisierungsAnfrage> SynchronisierungsAnfragen { get; }
    DbSet<Selbstkompetenz> Selbstkompetenzen { get; }
    DbSet<HilfeText> HilfeTexte { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
