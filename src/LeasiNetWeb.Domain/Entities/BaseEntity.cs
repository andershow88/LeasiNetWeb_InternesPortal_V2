namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Shared audit fields for all entities, replacing Intrexx's system fields
/// (DTINSERT, DTEDIT, LUSERIDINSERT, LUSERID).
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime ErstelltAm { get; set; }
    public DateTime GeaendertAm { get; set; }
    public int? ErstelltVonId { get; set; }
    public int? GeaendertVonId { get; set; }
}
