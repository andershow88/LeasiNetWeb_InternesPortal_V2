using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LeasiNetWeb.Web.Controllers;

[Authorize]
public class AiAssistentController : BaseController
{
    private readonly IApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AiAssistentController(
        IApplicationDbContext db,
        IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Fragen([FromBody] ChatAnfrageDto anfrage)
    {
        var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
            ?? _config["AnthropicApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            return Json(new { error = "Kein ANTHROPIC_API_KEY konfiguriert. Bitte in Railway-Umgebungsvariablen eintragen." });

        if (string.IsNullOrWhiteSpace(anfrage?.Frage))
            return Json(new { error = "Keine Frage angegeben." });

        try
        {
            // ── Datenbankkontext aufbauen ────────────────────────────────────
            var kontext = await BaueKontextAsync(anfrage.Frage);

            // ── Nachrichtenverlauf aufbauen ──────────────────────────────────
            var messages = new List<object>();

            foreach (var msg in anfrage.Verlauf ?? [])
                messages.Add(new { role = msg.Rolle, content = msg.Text });

            var userContent = string.IsNullOrWhiteSpace(kontext)
                ? anfrage.Frage
                : $"{anfrage.Frage}\n\n---\n*Aktuelle Systemdaten (automatisch geladen):*\n{kontext}";

            messages.Add(new { role = "user", content = userContent });

            // ── System-Prompt ────────────────────────────────────────────────
            const string systemPrompt = """
                Du bist der KI-Assistent des LeasiNet Internen Portals der Merkur Privatbank KGaA.
                Du hilfst Mitarbeitern mit Fragen zur Software, zu Workflows und zu konkreten Antragsdaten.

                **Software-Übersicht:**
                LeasiNet ist ein Leasing-Antrags-Managementsystem. Leasinggesellschaften (LG) reichen Anträge ein,
                Mitarbeiter der Merkur Bank (MB) bearbeiten diese, interne Prüfer führen Kontrollen durch,
                Genehmiger entscheiden über Freigabe.

                **Antragsstatus (Reihenfolge im Workflow):**
                1. Entwurf – noch nicht eingereicht
                2. EingereichtVonLG – LG hat eingereicht, wartet auf MB
                3. BeiMitarbeiter – MB-Sachbearbeiter bearbeitet
                4. InPruefung – wird gerade geprüft
                5. InterneKontrolleErforderlich – interne Prüfung läuft (sequentielle Prüfschritte)
                6. BeiGenehmiger – wartet auf Genehmigung
                7. BeiZweitemGenehmiger – 2. Vote erforderlich (bei hohem Obligo)
                8. Genehmigt – freigegeben, Vertrag kann erstellt werden
                9. Abgelehnt – abgelehnt (mit Ablehnungsgrund)
                10. Storniert – vom Antragsteller storniert
                11. Archiviert – archiviert nach Abschluss

                **Rollen:**
                - SachbearbeiterMB: Mitarbeiter Merkur Bank, bearbeitet Anträge
                - SachbearbeiterLG: Mitarbeiter Leasinggesellschaft, reicht Anträge ein
                - Genehmiger: Entscheidet über Freigabe, hat Selbstkompetenzlimit
                - InternerPruefer: Führt interne Kontrollen durch (KYC, Bonität, GwG etc.)
                - Auswerter: Hat Lesezugriff auf Auswertungen
                - Administrator: Vollzugriff, verwaltet Benutzer und Systemkonfiguration

                **Interne Kontrolle (Workflow-Details):**
                - Kann vom MB-Sachbearbeiter für jeden Antrag gestartet werden
                - Mehrere Prüfschritte werden sequentiell abgearbeitet (jeder Schritt hat einen zugewiesenen Prüfer)
                - Checkliste: KYC, Bonitätsprüfung, Geldwäsche-Check, Sanktionslisten, Unterlagen, Obligo-Limit
                - Automatische Prüfnummer: Format {LG-Kürzel}/{Jahr}/{Seq:D3}, z.B. MB-AG/2026/001
                - Nach Abschluss: Antrag geht zurück zu BeiMitarbeiter

                **Genehmigung:**
                - Genehmiger hat Selbstkompetenzlimit (MaxObligoBetrag)
                - Über ZweiteVoteSchwellenwert: zweite Stimme eines weiteren Genehmigers nötig
                - Ablehnung: Ablehnungsgrund wird dokumentiert

                **Wichtige Felder eines Antrags:**
                AntragNummer (eindeutig), Status, Obligo (€), Leasinggesellschaft, SachbearbeiterMB,
                Leasingobjekte (Bezeichnung, Listenpreis, Rabatt, NAK), ErstelltAm, GeaendertAm

                Antworte immer auf Deutsch. Sei präzise, freundlich und strukturiert.
                Nutze Markdown für Listen und Hervorhebungen.
                Bei Antragslisten: zeige Nummer, Status, Obligo und Leasinggesellschaft.
                Wenn du keine Daten zu einer spezifischen Anfrage hast, sage es klar.
                """;

            // ── Anthropic API aufrufen ───────────────────────────────────────
            var client = _httpClientFactory.CreateClient("Anthropic");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var requestBody = new
            {
                model = "claude-haiku-4-5-20251001",
                max_tokens = 1500,
                system = systemPrompt,
                messages
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(requestBody, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[AI] Anthropic error {response.StatusCode}: {body}");
                return Json(new { error = $"API-Fehler ({response.StatusCode}). Bitte API-Key prüfen." });
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<AnthropicResponse>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var antwort = result?.Content?.FirstOrDefault()?.Text ?? "Keine Antwort erhalten.";
            return Json(new { antwort });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[AI] Exception: {ex.Message}");
            return Json(new { error = "Interner Fehler beim Aufrufen des KI-Assistenten." });
        }
    }

    // ── Datenbankkontext aufbauen ────────────────────────────────────────────

    private async Task<string> BaueKontextAsync(string frage)
    {
        var sb = new StringBuilder();
        var frageLower = frage.ToLowerInvariant();

        // ── Immer: Statistik ─────────────────────────────────────────────────
        var stats = await _db.Leasingantraege
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key.ToString(), Anzahl = g.Count() })
            .ToListAsync();

        sb.AppendLine("**Antrag-Statistik (aktuell):**");
        foreach (var s in stats.OrderBy(s => s.Status))
            sb.AppendLine($"- {s.Status}: {s.Anzahl} Anträge");

        // ── Gesamtobligo ────────────────────────────────────────────────────
        var gesamtObligo = await _db.Leasingantraege.SumAsync(a => a.Obligo);
        sb.AppendLine($"- **Gesamt-Obligo aktiver Anträge:** {gesamtObligo:N2} €");
        sb.AppendLine();

        // ── Spezifische Antrag-Nummer in der Frage? ──────────────────────────
        var antragNummerMatch = Regex.Match(frage, @"\b([A-Z]{2,6}-?\d{4,8})\b|(\d{4,8}-[A-Z]{2,6})",
            RegexOptions.IgnoreCase);
        if (antragNummerMatch.Success)
        {
            var suchNummer = antragNummerMatch.Value.ToUpper();
            var gefunden = await _db.Leasingantraege
                .Include(a => a.Leasinggesellschaft)
                .Include(a => a.SachbearbeiterMB)
                .Include(a => a.Objekte)
                .Where(a => a.AntragNummer.Contains(suchNummer))
                .Take(5)
                .ToListAsync();

            if (gefunden.Any())
            {
                sb.AppendLine($"**Suchergebnis für '{suchNummer}':**");
                foreach (var a in gefunden)
                {
                    sb.AppendLine($"- **{a.AntragNummer}** | Status: {a.Status} | Obligo: {a.Obligo:N2}€ | LG: {a.Leasinggesellschaft?.Name ?? "–"} | Erstellt: {a.ErstelltAm:dd.MM.yyyy}");
                    if (a.Objekte.Any())
                        sb.AppendLine($"  Objekte: {string.Join(", ", a.Objekte.Take(3).Select(o => o.Bezeichnung))}");
                }
                sb.AppendLine();
            }
        }

        // ── Anträge auflisten / suchen ───────────────────────────────────────
        bool sucheAntraege = frageLower.Contains("antrag") || frageLower.Contains("liste")
            || frageLower.Contains("zeig") || frageLower.Contains("such")
            || frageLower.Contains("offen") || frageLower.Contains("aktuell");

        if (sucheAntraege && !antragNummerMatch.Success)
        {
            var neueste = await _db.Leasingantraege
                .Include(a => a.Leasinggesellschaft)
                .Include(a => a.SachbearbeiterMB)
                .Where(a => a.Status != AntragStatus.Archiviert && a.Status != AntragStatus.Storniert)
                .OrderByDescending(a => a.GeaendertAm)
                .Take(15)
                .ToListAsync();

            sb.AppendLine("**Aktuelle Anträge (zuletzt geändert, ohne Archiv):**");
            foreach (var a in neueste)
                sb.AppendLine($"- {a.AntragNummer} | {a.Status} | {a.Obligo:N2}€ | {a.Leasinggesellschaft?.Name ?? "–"}");
            sb.AppendLine();
        }

        // ── Benutzer / Prüfer ────────────────────────────────────────────────
        if (frageLower.Contains("benutzer") || frageLower.Contains("prüfer") || frageLower.Contains("mitarbeiter") || frageLower.Contains("user"))
        {
            var benutzer = await _db.Benutzer
                .Where(b => b.IstAktiv)
                .OrderBy(b => b.Nachname)
                .Select(b => new { b.Vorname, b.Nachname, Rolle = b.Rolle.ToString() })
                .ToListAsync();

            sb.AppendLine("**Aktive Benutzer:**");
            foreach (var b in benutzer)
                sb.AppendLine($"- {b.Vorname} {b.Nachname} ({b.Rolle})");
            sb.AppendLine();
        }

        // ── Leasinggesellschaften ────────────────────────────────────────────
        if (frageLower.Contains("gesellschaft") || frageLower.Contains("leasinggesellschaft") || frageLower.Contains("lg"))
        {
            var lgs = await _db.Leasinggesellschaften
                .OrderBy(l => l.Name)
                .Select(l => new { l.Name, l.Kurzbezeichnung, l.ObligoLimit })
                .ToListAsync();

            sb.AppendLine("**Leasinggesellschaften:**");
            foreach (var lg in lgs)
                sb.AppendLine($"- {lg.Name} ({lg.Kurzbezeichnung}) | Obligo-Limit: {lg.ObligoLimit:N2}€");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record ChatAnfrageDto(string Frage, List<ChatNachrichtDto>? Verlauf);
public record ChatNachrichtDto(string Rolle, string Text);

internal class AnthropicResponse
{
    public List<AnthropicContentBlock>? Content { get; set; }
}
internal class AnthropicContentBlock
{
    public string? Text { get; set; }
}
