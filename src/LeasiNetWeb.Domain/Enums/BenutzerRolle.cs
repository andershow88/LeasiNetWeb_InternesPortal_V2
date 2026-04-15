namespace LeasiNetWeb.Domain.Enums;

public enum BenutzerRolle
{
    Mitarbeiter = 0,           // MB: Standard employee — can submit and view own applications
    SachbearbeiterMB = 1,      // MB editor — assigned to applications as processor
    SachbearbeiterLG = 2,      // LG editor — leasing company side processor
    Genehmiger = 3,            // Can approve/reject applications
    InternerPruefer = 4,       // Internal audit/compliance
    Administrator = 5,         // Full admin access
    Auswerter = 6              // Read-only reporting access
}
