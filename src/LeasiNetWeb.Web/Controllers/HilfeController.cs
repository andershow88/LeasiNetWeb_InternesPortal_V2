using LeasiNetWeb.Application.Interfaces;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Web.Controllers;

public class HilfeController : BaseController
{
    private readonly IApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public HilfeController(IApplicationDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    public async Task<IActionResult> Index(string? schluessel = null)
    {
        var hilfeTexte = await _db.HilfeTexte
            .Where(h => h.IstAktiv && (schluessel == null || h.Schluessel == schluessel))
            .OrderBy(h => h.Titel)
            .ToListAsync();

        // Anwenderdokumentation aus docs/Anwenderdokumentation.md laden und in HTML konvertieren
        var docsPath = Path.Combine(_env.ContentRootPath, "..", "..", "docs", "Anwenderdokumentation.md");
        if (!System.IO.File.Exists(docsPath))
            docsPath = Path.Combine(_env.ContentRootPath, "docs", "Anwenderdokumentation.md");

        if (System.IO.File.Exists(docsPath))
        {
            var markdown = await System.IO.File.ReadAllTextAsync(docsPath);
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
            ViewBag.AnwenderdokumentationHtml = Markdown.ToHtml(markdown, pipeline);
        }

        return View(hilfeTexte);
    }
}
