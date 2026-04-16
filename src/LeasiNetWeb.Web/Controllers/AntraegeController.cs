using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using LeasiNetWeb.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace LeasiNetWeb.Web.Controllers;

public class AntraegeController : BaseController
{
    private readonly ILeasingantragService _antraege;
    private readonly IKommentarService _kommentare;
    private readonly IAnhangService _anhaenge;
    private readonly IApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AntraegeController(ILeasingantragService antraege, IKommentarService kommentare,
        IAnhangService anhaenge, IApplicationDbContext db,
        IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _antraege = antraege;
        _kommentare = kommentare;
        _anhaenge = anhaenge;
        _db = db;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    // ── Übersicht ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(AntragStatus? status = null)
    {
        var liste = await _antraege.GetAlleAntraege(status: status);
        ViewBag.AktuellerStatus = status;
        return View(liste);
    }

    public async Task<IActionResult> MeinePruefungen()
    {
        ViewBag.SeitenTitel      = "Meine Prüfungen";
        ViewBag.SeitenUntertitel = "Anträge, die auf Ihre Prüfung warten";
        ViewBag.SeitenIcon       = "bi-clipboard-check";
        return View("Index", await _antraege.GetMeineZuPruefendenAntraege(AktuellerBenutzerId));
    }

    public async Task<IActionResult> MeineGenehmigungen()
    {
        ViewBag.SeitenTitel      = "Genehmigungen";
        ViewBag.SeitenUntertitel = "Anträge, die Ihre Genehmigung erfordern";
        ViewBag.SeitenIcon       = "bi-check2-square";
        return View("Index", await _antraege.GetMeineZuGenehmigendenAntraege(AktuellerBenutzerId));
    }

    public async Task<IActionResult> MeineZweiteVoten()
    {
        ViewBag.SeitenTitel      = "2. Vote";
        ViewBag.SeitenUntertitel = "Anträge, bei denen ein zweites Votum erforderlich ist";
        ViewBag.SeitenIcon       = "bi-person-check";
        return View("Index", await _antraege.GetMeineZweiteVotenAntraege(AktuellerBenutzerId));
    }

    public async Task<IActionResult> Archiv()
    {
        ViewBag.SeitenTitel      = "Archiv";
        ViewBag.SeitenUntertitel = "Archivierte Leasinganträge";
        ViewBag.SeitenIcon       = "bi-archive";
        var alle = await _db.Leasingantraege
            .Include(a => a.EingereichtVon)
            .Include(a => a.Leasinggesellschaft)
            .Include(a => a.SachbearbeiterMB)
            .Where(a => a.Archiviert)
            .OrderByDescending(a => a.ArchiviertAm)
            .Select(a => new AntragListeDto(a.Id, a.AntragNummer, a.AntragTyp, a.Status,
                a.EingereichtVon.Anzeigename, a.Leasinggesellschaft != null ? a.Leasinggesellschaft.Name : null,
                a.SachbearbeiterMB != null ? a.SachbearbeiterMB.Anzeigename : null,
                a.Obligo, a.ErstelltAm, a.GeaendertAm, a.ZweiteVoteErforderlich, a.KiErstellt))
            .ToListAsync();
        return View("Index", alle);
    }

    // ── Detail ─────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Details(int id)
    {
        var detail = await _antraege.GetAntragDetail(id);
        if (detail is null) return NotFound();

        var vertragId = await _db.Vertraege
            .Where(v => v.LeasingantragId == id)
            .Select(v => (int?)v.Id)
            .FirstOrDefaultAsync();
        ViewBag.VertragId = vertragId;

        var ablehnungsgruende = await _db.Ablehnungsgruende
            .Where(ag => ag.IstAktiv)
            .OrderBy(ag => ag.Bezeichnung)
            .Select(ag => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(ag.Bezeichnung, ag.Id.ToString()))
            .ToListAsync();
        ViewBag.Ablehnungsgruende = ablehnungsgruende;

        return View(detail);
    }

    // ── Neu anlegen ────────────────────────────────────────────────────────────

    public async Task<IActionResult> Neu(bool ki = false, int? at = null, decimal? ob = null,
        string? ab = null, int? lgId = null, string? token = null, string? fn = null)
    {
        await FuelleDropdowns();
        var vm = new AntragErstellenViewModel
        {
            KiErstellt    = ki,
            PdfTempToken  = token,
            PdfDateiname  = fn
        };
        if (ki)
        {
            if (at.HasValue && Enum.IsDefined(typeof(AntragTyp), at.Value))
                vm.AntragTyp = (AntragTyp)at.Value;
            if (ob.HasValue && ob.Value > 0) vm.Obligo = ob.Value;
            if (!string.IsNullOrWhiteSpace(ab))  vm.Abrechnungsart = ab;
            if (lgId.HasValue) vm.LeasinggesellschaftId = lgId;
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Neu(AntragErstellenViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FuelleDropdowns();
            return View(model);
        }

        var id = await _antraege.ErstelleAntrag(
            new AntragErstellenDto(model.AntragTyp, model.LeasinggesellschaftId,
                model.Obligo, model.Abrechnungsart, model.KiErstellt),
            AktuellerBenutzerId);

        // KI-Antrag: hochgeladenes PDF aus Temp-Verzeichnis als Anhang speichern
        if (model.KiErstellt && !string.IsNullOrWhiteSpace(model.PdfTempToken))
        {
            var tempPfad = Path.Combine(Path.GetTempPath(), "leasinetweb_ki",
                $"{model.PdfTempToken}.pdf");
            if (System.IO.File.Exists(tempPfad))
            {
                try
                {
                    await using var fs = System.IO.File.OpenRead(tempPfad);
                    var dateiname = string.IsNullOrWhiteSpace(model.PdfDateiname)
                        ? "Antragsdokument_KI.pdf" : model.PdfDateiname;
                    await _anhaenge.HochladenAsync(fs, dateiname, "application/pdf",
                        new FileInfo(tempPfad).Length, AnhangTyp.Antragsdokument,
                        AktuellerBenutzerId, antragId: id);
                }
                catch { /* Anhang-Fehler darf den Antrag nicht blockieren */ }
                finally
                {
                    try { System.IO.File.Delete(tempPfad); } catch { }
                }
            }
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // ── KI-PDF-Analyse ────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> PdfAnalysieren(IFormFile pdf)
    {
        if (pdf == null || pdf.Length == 0)
            return Json(new { error = "Keine Datei hochgeladen." });
        if (!pdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            && pdf.ContentType != "application/pdf")
            return Json(new { error = "Nur PDF-Dateien werden unterstützt." });
        if (pdf.Length > 20 * 1024 * 1024)
            return Json(new { error = "Datei zu groß (max. 20 MB)." });

        // 1. PDF-Bytes lesen
        byte[] pdfBytes;
        using (var ms = new MemoryStream())
        {
            await pdf.CopyToAsync(ms);
            pdfBytes = ms.ToArray();
        }

        // 2. Text extrahieren (PdfPig)
        string extractedText;
        try
        {
            var sb = new StringBuilder();
            using var doc = PdfDocument.Open(pdfBytes);
            foreach (var page in doc.GetPages())
            {
                foreach (var word in page.GetWords())
                    sb.Append(word.Text).Append(' ');
                sb.AppendLine();
            }
            extractedText = sb.ToString().Trim();
        }
        catch (Exception ex)
        {
            return Json(new { error = $"PDF konnte nicht gelesen werden: {ex.Message}" });
        }

        if (string.IsNullOrWhiteSpace(extractedText))
            return Json(new { error = "Kein lesbarer Text im PDF (ggf. Scan-PDF ohne OCR)." });

        // 3. OpenAI-Analyse
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? _config["OpenAiApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return Json(new { error = "Kein OPENAI_API_KEY konfiguriert." });

        var lgNamen = await _db.Leasinggesellschaften
            .Where(l => l.IstAktiv).Select(l => l.Name).ToListAsync();

        var antragTypNamen = Enum.GetNames<AntragTyp>();
        var prompt = $$"""
            Analysiere das folgende Antragsdokument und extrahiere die Felder für einen Leasingantrag.

            Verfügbare Antragstypen: {{string.Join(", ", antragTypNamen)}}
            Verfügbare Leasinggesellschaften im System: {{string.Join(", ", lgNamen)}}

            Antworte NUR mit einem JSON-Objekt ohne Erklärungen:
            {
              "antragTyp": <einer der verfügbaren Antragstypen als String oder null>,
              "obligo": <Dezimalzahl ohne Währungssymbol oder null>,
              "abrechnungsart": <kurzer Text oder null>,
              "leasinggesellschaft": <exakter Name aus der Liste oder null>,
              "konfidenz": "hoch" oder "mittel" oder "niedrig"
            }

            Antragsdokument:
            {{extractedText[..Math.Min(extractedText.Length, 6000)]}}
            """;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var reqBody = new
            {
                model = "gpt-4o-mini",
                max_tokens = 300,
                messages = new[]
                {
                    new { role = "system", content = "Du bist ein Datenextraktions-Assistent. Antworte ausschließlich mit gültigem JSON." },
                    new { role = "user",   content = prompt }
                }
            };
            var json    = JsonSerializer.Serialize(reqBody,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var resp    = await client.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
                return Json(new { error = "OpenAI-Fehler – bitte API-Key prüfen." });

            using var stream = await resp.Content.ReadAsStreamAsync();
            var oaiResp  = await JsonSerializer.DeserializeAsync<KiExtraktionResponse>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var antwort  = oaiResp?.Choices?.FirstOrDefault()?.Message?.Content ?? "";

            var jsonMatch = Regex.Match(antwort, @"\{[\s\S]*\}");
            if (!jsonMatch.Success)
                return Json(new { error = "KI-Antwort konnte nicht interpretiert werden." });

            using var doc  = JsonDocument.Parse(jsonMatch.Value);
            var root       = doc.RootElement;

            // AntragTyp auflösen
            var atStr = root.TryGetProperty("antragTyp", out var atEl) && atEl.ValueKind != JsonValueKind.Null
                ? atEl.GetString() : null;
            AntragTyp? antragTyp = Enum.TryParse<AntragTyp>(atStr, true, out var at) ? at : null;

            // Leasinggesellschaft auflösen
            var lgName = root.TryGetProperty("leasinggesellschaft", out var lgEl) && lgEl.ValueKind != JsonValueKind.Null
                ? lgEl.GetString() : null;
            int? lgId = null;
            if (!string.IsNullOrEmpty(lgName))
                lgId = (await _db.Leasinggesellschaften
                    .Where(l => l.IstAktiv && l.Name == lgName)
                    .Select(l => (int?)l.Id).FirstOrDefaultAsync());

            decimal? obligo = root.TryGetProperty("obligo", out var obEl) && obEl.ValueKind == JsonValueKind.Number
                ? obEl.GetDecimal() : null;
            var abrechnungsart = root.TryGetProperty("abrechnungsart", out var abEl) && abEl.ValueKind != JsonValueKind.Null
                ? abEl.GetString() : null;
            var konfidenz = root.TryGetProperty("konfidenz", out var kEl) ? kEl.GetString() : "niedrig";

            // 4. PDF als Temp-Datei speichern
            var token   = Guid.NewGuid().ToString("N");
            var tempDir = Path.Combine(Path.GetTempPath(), "leasinetweb_ki");
            Directory.CreateDirectory(tempDir);
            await System.IO.File.WriteAllBytesAsync(
                Path.Combine(tempDir, $"{token}.pdf"), pdfBytes);

            return Json(new
            {
                ok = true,
                antragTyp     = (int?)antragTyp,
                antragTypName = atStr,
                obligo,
                abrechnungsart,
                leasinggesellschaftId   = lgId,
                leasinggesellschaftName = lgName,
                konfidenz,
                tempToken = token,
                dateiname = pdf.FileName
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = $"Fehler: {ex.Message}" });
        }
    }

    // ── KI-Antrag direkt erstellen ─────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KiAntragErstellen(int? antragTyp, decimal? obligo,
        string? abrechnungsart, int? leasinggesellschaftId, string? tempToken, string? dateiname)
    {
        if (antragTyp == null || !Enum.IsDefined(typeof(AntragTyp), antragTyp.Value))
            return Json(new { error = "Ungültiger Antragstyp." });

        var id = await _antraege.ErstelleKiAntrag(
            new AntragErstellenDto((AntragTyp)antragTyp.Value, leasinggesellschaftId,
                obligo ?? 0, abrechnungsart, KiErstellt: true),
            AktuellerBenutzerId);

        // PDF-Anhang aus Temp-Verzeichnis
        if (!string.IsNullOrWhiteSpace(tempToken))
        {
            var tempPfad = Path.Combine(Path.GetTempPath(), "leasinetweb_ki", $"{tempToken}.pdf");
            if (System.IO.File.Exists(tempPfad))
            {
                try
                {
                    await using var fs = System.IO.File.OpenRead(tempPfad);
                    var fn = string.IsNullOrWhiteSpace(dateiname) ? "Antragsdokument_KI.pdf" : dateiname;
                    await _anhaenge.HochladenAsync(fs, fn, "application/pdf",
                        new FileInfo(tempPfad).Length, AnhangTyp.Antragsdokument,
                        AktuellerBenutzerId, antragId: id);
                }
                catch { /* Anhang-Fehler blockiert nicht */ }
                finally
                {
                    try { System.IO.File.Delete(tempPfad); } catch { }
                }
            }
        }

        return Json(new { ok = true, antragId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KiBestaetigen(int id)
    {
        await _antraege.StatusWechsel(id, AntragStatus.InPruefung, AktuellerBenutzerId,
            "KI-Antrag von Mitarbeiter bestätigt und in Prüfung übernommen");
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Status-Aktionen ────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Einreichen(int id)
    {
        await _antraege.StatusWechsel(id, AntragStatus.Eingereicht, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InPruefung(int id)
    {
        await _antraege.StatusWechsel(id, AntragStatus.InPruefung, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PruefungAbschliessen(int id)
    {
        if (AktuelleRolle is not ("SachbearbeiterMB" or "Administrator"))
            return Forbid();
        await _antraege.StatusWechsel(id, AntragStatus.BeiMitarbeiter, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Genehmigen(int id)
    {
        if (!IstGenehmiger) return Forbid();
        await _antraege.Genehmigen(id, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ablehnen(int id, int ablehnungsgrundId, string? kommentar)
    {
        if (!IstGenehmiger) return Forbid();
        await _antraege.Ablehnen(id, AktuellerBenutzerId, ablehnungsgrundId, kommentar);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZweiteVoteAnfordern(int id)
    {
        if (!IstGenehmiger) return Forbid();
        await _antraege.ZweiteVoteAnfordern(id, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archivieren(int id)
    {
        await _antraege.Archivieren(id);
        return RedirectToAction(nameof(Index));
    }

    // ── Kommentar ──────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KommentarHinzufuegen(int antragId, string text, bool istIntern)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            TempData["Fehler"] = "Kommentar darf nicht leer sein.";
            return RedirectToAction(nameof(Details), new { id = antragId });
        }

        await _kommentare.HinzufuegenAsync(new KommentarErstellenDto(antragId, text, istIntern),
            AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id = antragId });
    }

    // ── Anhang ─────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnhangHochladen(int antragId, IFormFile datei)
    {
        if (datei is null || datei.Length == 0)
        {
            TempData["Fehler"] = "Bitte wählen Sie eine Datei aus.";
            return RedirectToAction(nameof(Details), new { id = antragId });
        }

        try
        {
            await using var stream = datei.OpenReadStream();
            await _anhaenge.HochladenAsync(stream, datei.FileName, datei.ContentType,
                datei.Length, AnhangTyp.Antragsdokument,
                AktuellerBenutzerId, antragId: antragId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Fehler"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = antragId });
    }

    [HttpGet]
    public async Task<IActionResult> AnhangHerunterladen(int anhangId)
    {
        try
        {
            var (inhalt, contentType, dateiname) = await _anhaenge.HerunterladenAsync(anhangId);
            return File(inhalt, contentType, dateiname);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task FuelleDropdowns()
    {
        var gesellschaften = await _db.Leasinggesellschaften
            .Where(l => l.IstAktiv)
            .OrderBy(l => l.Name)
            .Select(l => new { l.Id, l.Name })
            .ToListAsync();

        ViewBag.Leasinggesellschaften = new SelectList(gesellschaften, "Id", "Name");
        ViewBag.AntragTypen = Enum.GetValues<AntragTyp>()
            .Select(t => new SelectListItem(t.ToString(), ((int)t).ToString()));
    }
}

// ── Interne DTOs für OpenAI-Antwort ──────────────────────────────────────────
file class KiExtraktionResponse
{
    public List<KiExtraktionChoice>? Choices { get; set; }
}
file class KiExtraktionChoice
{
    public KiExtraktionMessage? Message { get; set; }
}
file class KiExtraktionMessage
{
    public string? Content { get; set; }
}
