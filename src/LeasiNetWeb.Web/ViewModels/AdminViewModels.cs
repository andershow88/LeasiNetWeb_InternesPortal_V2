using System.ComponentModel.DataAnnotations;
using LeasiNetWeb.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeasiNetWeb.Web.ViewModels;

// ── Benutzer ──────────────────────────────────────────────────────────────────

public class BenutzerFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Benutzername")]
    [StringLength(100)]
    public string Benutzername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Vorname")]
    [StringLength(100)]
    public string Vorname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Nachname")]
    [StringLength(100)]
    public string Nachname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pflichtfeld")]
    [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
    [Display(Name = "E-Mail")]
    public string EMail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Rolle")]
    public BenutzerRolle Rolle { get; set; }

    [Display(Name = "Aktiv")]
    public bool IstAktiv { get; set; } = true;

    [Display(Name = "Leasinggesellschaft")]
    public int? LeasinggesellschaftId { get; set; }

    // Required only on create (Id == 0)
    [Display(Name = "Passwort")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mindestens 8 Zeichen")]
    public string? Passwort { get; set; }

    [Display(Name = "Passwort bestätigen")]
    [DataType(DataType.Password)]
    [Compare(nameof(Passwort), ErrorMessage = "Passwörter stimmen nicht überein")]
    public string? PasswortBestaetigen { get; set; }

    // Selectlists for dropdowns
    public IEnumerable<SelectListItem> LeasinggesellschaftenListe { get; set; }
        = Enumerable.Empty<SelectListItem>();
}

// ── Leasinggesellschaft ───────────────────────────────────────────────────────

public class LeasinggesellschaftFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Name")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Kurzbezeichnung")]
    [StringLength(20)]
    public string? Kurzbezeichnung { get; set; }

    [Display(Name = "Straße")]
    [StringLength(200)]
    public string? Strasse { get; set; }

    [Display(Name = "PLZ")]
    [StringLength(10)]
    public string? PLZ { get; set; }

    [Display(Name = "Ort")]
    [StringLength(100)]
    public string? Ort { get; set; }

    [Display(Name = "Land")]
    [StringLength(10)]
    public string? Land { get; set; } = "DE";

    [Display(Name = "Telefon")]
    [StringLength(50)]
    public string? Telefon { get; set; }

    [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
    [Display(Name = "E-Mail")]
    [StringLength(200)]
    public string? EMail { get; set; }

    [Display(Name = "Ansprechpartner")]
    [StringLength(200)]
    public string? Ansprechpartner { get; set; }

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Obligo-Limit (€)")]
    [Range(0, 999_999_999, ErrorMessage = "Ungültiger Betrag")]
    public decimal ObligoLimit { get; set; }

    [Display(Name = "Aktiv")]
    public bool IstAktiv { get; set; } = true;
}

// ── Ablehnungsgrund ───────────────────────────────────────────────────────────

public class AblehnungsgrundFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Code")]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Bezeichnung")]
    [StringLength(300)]
    public string Bezeichnung { get; set; } = string.Empty;

    [Display(Name = "Aktiv")]
    public bool IstAktiv { get; set; } = true;
}

// ── Gerätetyp ─────────────────────────────────────────────────────────────────

public class GeraetetypFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pflichtfeld")]
    [Display(Name = "Bezeichnung")]
    [StringLength(200)]
    public string Bezeichnung { get; set; } = string.Empty;

    [Display(Name = "Beschreibung")]
    [StringLength(500)]
    public string? Beschreibung { get; set; }

    [Display(Name = "Aktiv")]
    public bool IstAktiv { get; set; } = true;

    [Display(Name = "Übergeordnete Kategorie")]
    public int? ElterntypId { get; set; }

    public IEnumerable<SelectListItem> ElterntypListe { get; set; }
        = Enumerable.Empty<SelectListItem>();
}
