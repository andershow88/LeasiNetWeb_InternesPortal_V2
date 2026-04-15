using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DataProtection keys — persisted in DB so they survive container restarts
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<Benutzer> Benutzer => Set<Benutzer>();
    public DbSet<Leasinggesellschaft> Leasinggesellschaften => Set<Leasinggesellschaft>();
    public DbSet<Leasingantrag> Leasingantraege => Set<Leasingantrag>();
    public DbSet<Leasingobjekt> Leasingobjekte => Set<Leasingobjekt>();
    public DbSet<InternePruefung> InternePruefungen => Set<InternePruefung>();
    public DbSet<PruefungsPflicht> PruefungsPflichten => Set<PruefungsPflicht>();
    public DbSet<PruefungsSchritt> PruefungsSchritte => Set<PruefungsSchritt>();
    public DbSet<Vertrag> Vertraege => Set<Vertrag>();
    public DbSet<Kommentar> Kommentare => Set<Kommentar>();
    public DbSet<Anhang> Anhaenge => Set<Anhang>();
    public DbSet<Nachricht> Nachrichten => Set<Nachricht>();
    public DbSet<Ereignis> Ereignisse => Set<Ereignis>();
    public DbSet<Ablehnungsgrund> Ablehnungsgruende => Set<Ablehnungsgrund>();
    public DbSet<Vertragstyp> Vertragstypen => Set<Vertragstyp>();
    public DbSet<Geraetetyp> Geraetetypen => Set<Geraetetyp>();
    public DbSet<Rate> Raten => Set<Rate>();
    public DbSet<Ratentabelle> Ratentabellen => Set<Ratentabelle>();
    public DbSet<DokumentAustausch> DokumentAustausche => Set<DokumentAustausch>();
    public DbSet<LgRegistrierung> LgRegistrierungen => Set<LgRegistrierung>();
    public DbSet<Obligo> Obligos => Set<Obligo>();
    public DbSet<SynchronisierungsAnfrage> SynchronisierungsAnfragen => Set<SynchronisierungsAnfrage>();
    public DbSet<Selbstkompetenz> Selbstkompetenzen => Set<Selbstkompetenz>();
    public DbSet<HilfeText> HilfeTexte => Set<HilfeText>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Benutzer ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Benutzer>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasIndex(b => b.Benutzername).IsUnique();
            e.Property(b => b.Benutzername).IsRequired().HasMaxLength(100);
            e.Property(b => b.PasswortHash).IsRequired().HasMaxLength(100);
            e.Property(b => b.Vorname).HasMaxLength(100);
            e.Property(b => b.Nachname).HasMaxLength(100);
            e.Property(b => b.EMail).HasMaxLength(200);
            e.HasOne(b => b.Leasinggesellschaft).WithMany(l => l.Benutzer)
                .HasForeignKey(b => b.LeasinggesellschaftId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── Leasinggesellschaft ───────────────────────────────────────────────
        modelBuilder.Entity<Leasinggesellschaft>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Name).IsRequired().HasMaxLength(200);
            e.Property(l => l.Kurzbezeichnung).HasMaxLength(20);
            e.Property(l => l.EMail).HasMaxLength(200);
            e.Property(l => l.Telefon).HasMaxLength(50);
            e.Property(l => l.ObligoLimit).HasPrecision(18, 2);
        });

        // ── Leasingantrag ─────────────────────────────────────────────────────
        modelBuilder.Entity<Leasingantrag>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.AntragNummer).IsRequired().HasMaxLength(30);
            e.HasIndex(a => a.AntragNummer).IsUnique();
            e.Property(a => a.Obligo).HasPrecision(18, 2);
            e.Property(a => a.Abrechnungsart).HasMaxLength(100);
            e.Property(a => a.AblehnungsKommentar).HasMaxLength(2000);

            e.HasOne(a => a.EingereichtVon).WithMany(b => b.EingereichteLeasingantraege)
                .HasForeignKey(a => a.EingereichtVonId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Leasinggesellschaft).WithMany(l => l.Leasingantraege)
                .HasForeignKey(a => a.LeasinggesellschaftId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.SachbearbeiterMB).WithMany(b => b.ZugewieseneAntraegeMB)
                .HasForeignKey(a => a.SachbearbeiterMBId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.SachbearbeiterLG).WithMany(b => b.ZugewieseneAntraegeLG)
                .HasForeignKey(a => a.SachbearbeiterLGId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.GenehmigerMB).WithMany()
                .HasForeignKey(a => a.GenehmigerMBId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.ZweiteVoteGenehmigerMB).WithMany()
                .HasForeignKey(a => a.ZweiteVoteGenehmigerMBId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Ablehnungsgrund).WithMany(ag => ag.Leasingantraege)
                .HasForeignKey(a => a.AblehnungsgrundId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── Leasingobjekt ─────────────────────────────────────────────────────
        modelBuilder.Entity<Leasingobjekt>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Bezeichnung).IsRequired().HasMaxLength(300);
            e.Property(o => o.Listenpreis).HasPrecision(18, 2);
            e.Property(o => o.Rabatt).HasPrecision(18, 2);
            e.Property(o => o.FinanzierungsBasis).HasPrecision(18, 2);
            e.Property(o => o.NAK).HasPrecision(18, 2);
            e.HasOne(o => o.Leasingantrag).WithMany(a => a.Objekte)
                .HasForeignKey(o => o.LeasingantragId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(o => o.Geraetetyp).WithMany(g => g.Objekte)
                .HasForeignKey(o => o.GeraetetypId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── InternePruefung ───────────────────────────────────────────────────
        modelBuilder.Entity<InternePruefung>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.PruefungNummer).HasMaxLength(50);
            e.HasOne(p => p.Leasingantrag).WithMany(a => a.InternePruefungen)
                .HasForeignKey(p => p.LeasingantragId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.PrueferMB).WithMany()
                .HasForeignKey(p => p.PrueferMBId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── PruefungsPflicht ──────────────────────────────────────────────────
        modelBuilder.Entity<PruefungsPflicht>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Bezeichnung).IsRequired().HasMaxLength(300);
            e.HasOne(p => p.InternePruefung).WithMany(ip => ip.Pflichten)
                .HasForeignKey(p => p.InternePruefungId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── PruefungsSchritt ──────────────────────────────────────────────────
        modelBuilder.Entity<PruefungsSchritt>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Bezeichnung).IsRequired().HasMaxLength(200);
            e.HasOne(s => s.InternePruefung).WithMany(ip => ip.Schritte)
                .HasForeignKey(s => s.InternePruefungId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.PrueferMB).WithMany()
                .HasForeignKey(s => s.PrueferMBId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Vertrag ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Vertrag>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.VertragNummer).IsRequired().HasMaxLength(50);
            e.HasIndex(v => v.VertragNummer).IsUnique();
            e.Property(v => v.Finanzierungsbetrag).HasPrecision(18, 2);
            e.Property(v => v.Restwert).HasPrecision(18, 2);
            e.Property(v => v.MonatlicheRate).HasPrecision(18, 4);
            e.Property(v => v.Zinssatz).HasPrecision(10, 4);
            e.HasOne(v => v.Leasingantrag).WithOne(a => a.Vertrag)
                .HasForeignKey<Vertrag>(v => v.LeasingantragId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Vertragstyp).WithMany(vt => vt.Vertraege)
                .HasForeignKey(v => v.VertragtypId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── Kommentar ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Kommentar>(e =>
        {
            e.HasKey(k => k.Id);
            e.Property(k => k.Text).IsRequired().HasMaxLength(4000);
            e.HasOne(k => k.Leasingantrag).WithMany(a => a.Kommentare)
                .HasForeignKey(k => k.LeasingantragId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(k => k.Autor).WithMany(b => b.Kommentare)
                .HasForeignKey(k => k.AutorId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Anhang ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Anhang>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Dateiname).IsRequired().HasMaxLength(500);
            e.Property(a => a.Dateipfad).IsRequired().HasMaxLength(1000);
            e.Property(a => a.ContentType).HasMaxLength(200);
            e.HasOne(a => a.HochgeladenVon).WithMany()
                .HasForeignKey(a => a.HochgeladenVonId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Leasingantrag).WithMany(x => x.Anhaenge)
                .HasForeignKey(a => a.LeasingantragId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Kommentar).WithMany(k => k.Anhaenge)
                .HasForeignKey(a => a.KommentarId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.InternePruefung).WithMany(p => p.Anhaenge)
                .HasForeignKey(a => a.InternePruefungId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Vertrag).WithMany(v => v.Anhaenge)
                .HasForeignKey(a => a.VertragId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Nachricht ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Nachricht>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Betreff).IsRequired().HasMaxLength(300);
            e.Property(n => n.Text).IsRequired().HasMaxLength(10000);
            e.HasOne(n => n.Absender).WithMany(b => b.GesendeteNachrichten)
                .HasForeignKey(n => n.AbsenderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(n => n.Empfaenger).WithMany()
                .HasForeignKey(n => n.EmpfaengerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(n => n.Leasingantrag).WithMany()
                .HasForeignKey(n => n.LeasingantragId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── Ereignis ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Ereignis>(e =>
        {
            e.HasKey(er => er.Id);
            e.Property(er => er.Beschreibung).HasMaxLength(1000);
            e.HasOne(er => er.Leasingantrag).WithMany(a => a.Ereignisse)
                .HasForeignKey(er => er.LeasingantragId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(er => er.AusgeloestVon).WithMany()
                .HasForeignKey(er => er.AusgeloestVonId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Rate / Ratentabelle ───────────────────────────────────────────────
        modelBuilder.Entity<Rate>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Faktor).HasPrecision(10, 6);
            e.Property(r => r.Restwertprozent).HasPrecision(10, 4);
            e.HasOne(r => r.Ratentabelle).WithMany(rt => rt.Raten)
                .HasForeignKey(r => r.RatentabelleId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Geraetetyp (self-referencing) ─────────────────────────────────────
        modelBuilder.Entity<Geraetetyp>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Bezeichnung).IsRequired().HasMaxLength(200);
            e.HasOne(g => g.Elterntyp).WithMany(g => g.Untertypen)
                .HasForeignKey(g => g.ElterntypId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Obligo ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Obligo>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Betrag).HasPrecision(18, 2);
            e.HasOne(o => o.Leasingantrag).WithMany(a => a.ObligoEintraege)
                .HasForeignKey(o => o.LeasingantragId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(o => o.Leasinggesellschaft).WithMany()
                .HasForeignKey(o => o.LeasinggesellschaftId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Selbstkompetenz ───────────────────────────────────────────────────
        modelBuilder.Entity<Selbstkompetenz>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.MaxObligoBetrag).HasPrecision(18, 2);
            e.Property(s => s.ZweiteVoteSchwellenwert).HasPrecision(18, 2);
            e.HasOne(s => s.Benutzer).WithMany()
                .HasForeignKey(s => s.BenutzerId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
