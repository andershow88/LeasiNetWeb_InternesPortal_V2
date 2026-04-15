using System.ComponentModel.DataAnnotations;

namespace LeasiNetWeb.Web.ViewModels;

public class NachrichtSendenViewModel
{
    [Required(ErrorMessage = "Bitte wählen Sie einen Empfänger.")]
    [Display(Name = "Empfänger")]
    public int EmpfaengerId { get; set; }

    [Display(Name = "Bezug: Antrag")]
    public int? LeasingantragId { get; set; }

    [Required(ErrorMessage = "Betreff ist erforderlich.")]
    [StringLength(300)]
    [Display(Name = "Betreff")]
    public string Betreff { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nachricht darf nicht leer sein.")]
    [StringLength(10000)]
    [Display(Name = "Nachricht")]
    public string Text { get; set; } = string.Empty;
}
