# LeasiNetWeb – Internes Portal V2

> Webbasiertes Leasingmanagement-Portal für die interne Verwaltung von Leasinganträgen, Verträgen, Obligos und Compliance-Prüfungen.

---

## Inhaltsverzeichnis

- [Überblick](#überblick)
- [Funktionsumfang](#funktionsumfang)
- [Technologie-Stack](#technologie-stack)
- [Architektur](#architektur)
- [Projektstruktur](#projektstruktur)
- [Voraussetzungen](#voraussetzungen)
- [Installation & Start](#installation--start)
- [Konfiguration](#konfiguration)
- [Deployment](#deployment)
- [Demo-Zugang](#demo-zugang)
- [Dokumentation](#dokumentation)
- [Lizenz](#lizenz)
- [Changelog](#changelog)

---

## Überblick

**LeasiNetWeb Internes Portal V2** ist eine ASP.NET Core 8 MVC-Webanwendung, die den kompletten Lebenszyklus von Leasinganträgen – von der Erstellung über Prüfung und Genehmigung bis zur Vertragsverwaltung – digital abbildet. Die Anwendung ersetzt ein vorheriges Intrexx-basiertes System und bietet ein modernes, rollenbasiertes Portal für Mitarbeiter, Sachbearbeiter, Genehmiger, interne Prüfer und Administratoren.

### Kernaufgaben

| Bereich | Beschreibung |
|---|---|
| **Antragsverwaltung** | Leasinganträge anlegen, einreichen, prüfen, genehmigen oder ablehnen – mit vollständigem Status-Workflow |
| **Vertragsverwaltung** | Aus genehmigten Anträgen Verträge erzeugen, bearbeiten, aktivieren, beenden oder kündigen |
| **Interne Prüfung** | Compliance-Checklisten führen, Prüfungspflichten abhaken und mehrstufige Prüfschritte sequentiell abarbeiten |
| **Obligo-Management** | Obligo-Beträge pro Antrag und Leasinggesellschaft tracken |
| **Nachrichtensystem** | Internes Messaging zwischen Benutzern mit Antragsbezug |
| **Auswertung & Export** | Kennzahlen-Dashboard und CSV-Export von Anträgen und Verträgen |
| **Administration** | Benutzer-, Leasinggesellschafts- und Stammdatenverwaltung |
| **KI-Assistent** | Kontextbewusster Chat-Assistent in der Topbar – Fragen zu Anträgen, Workflows und Systemdaten direkt stellen |
| **Dark Mode** | Zwischen hellem und dunklem Theme wechseln – Einstellung wird im Browser gespeichert |

---

## Funktionsumfang

### Rollen und Berechtigungen

| Rolle | Rechte |
|---|---|
| **Mitarbeiter** | Eigene Anträge erstellen und einsehen |
| **SachbearbeiterMB** | Anträge bearbeiten, prüfen, Sachbearbeitung durchführen |
| **SachbearbeiterLG** | Anträge auf Leasinggesellschaft-Seite bearbeiten |
| **Genehmiger** | Anträge genehmigen, ablehnen, zweite Vote anfordern |
| **InternerPrüfer** | Compliance-Prüfungen durchführen und abschließen |
| **Auswerter** | Lesender Zugriff auf Auswertungen und Reports |
| **Administrator** | Vollzugriff auf alle Bereiche inkl. Benutzerverwaltung |

### Antragsstatus-Workflow

```
Entwurf → Eingereicht → InPruefung → BeiMitarbeiter ──────────────────────────────→ Genehmigt
                                    ↘ BeiLeasinggesellschaft              ↘ Vertrag erstellen
                                    ↘ ZweiteVoteErforderlich
                                    ↘ InterneKontrolleErforderlich → (Prüfschritte) → BeiMitarbeiter
                                    ↘ Abgelehnt
                                    ↘ Storniert
                                       → Archiviert
```

### Vertragsstatus-Workflow

```
InVorbereitung → Aktiv → Beendet
                       → Gekuendigt
                       → Archiviert
```

### Hintergrund-Jobs (Hangfire)

- **Anträge archivieren** – monatlich, archiviert genehmigte/abgelehnte/stornierte Anträge nach 24 Monaten
- **Synchronisierungsanfragen bereinigen** – täglich, löscht verarbeitete Einträge älter als 30 Tage
- **Verwaiste Dateien bereinigen** – täglich, entfernt Upload-Dateien ohne zugehörigen Anhang-Eintrag in der Datenbank

### KI-Assistent

Der integrierte KI-Assistent ist über die **Topbar-Suchleiste** erreichbar. Eingabe einer Frage und Bestätigung mit **Enter** öffnet ein Modal direkt unterhalb der Topbar.

- **Modell:** OpenAI `gpt-4o-mini`
- **Kontextbewusstsein:** Lädt bei jeder Anfrage automatisch relevante Echtzeit-Daten aus der Datenbank (Antrag-Statistiken, aktuelle Anträge, Benutzer, Leasinggesellschaften)
- **Folgefragen:** Im Modal können weitere Fragen gestellt werden; der Gesprächsverlauf bleibt erhalten
- **Neue Konversation:** Jede Topbar-Anfrage startet eine frische Konversation (kein alter Kontext)
- **Voraussetzung:** Umgebungsvariable `OPENAI_API_KEY` oder Eintrag `OpenAiApiKey` in `appsettings.json`

---

## Technologie-Stack

| Komponente | Technologie |
|---|---|
| **Framework** | ASP.NET Core 8.0 (MVC mit Razor Views) |
| **Sprache** | C# 12 |
| **ORM** | Entity Framework Core 8.0 |
| **Datenbanken** | PostgreSQL (Produktion), SQL Server, SQLite (Entwicklung/Fallback) |
| **Background Jobs** | Hangfire mit In-Memory-Storage |
| **Authentifizierung** | Cookie-basiert mit Claims-Autorisierung |
| **Containerisierung** | Docker (Multi-Stage Build) |
| **Hosting** | Railway (oder jede Docker-fähige Plattform) |
| **Frontend** | Bootstrap (via wwwroot/lib), jQuery, eigenes MerkurConnect Design-System (`merkur.css`) |
| **KI-Integration** | OpenAI API (`gpt-4o-mini`) via `IHttpClientFactory` |

---

## Architektur

Die Lösung folgt der **Clean Architecture** mit vier Schichten:

```
┌──────────────────────────────────────────────────┐
│                  LeasiNetWeb.Web                 │  ← Präsentation (Controllers, Views, ViewModels)
│                  ASP.NET Core MVC                │
├──────────────────────────────────────────────────┤
│              LeasiNetWeb.Application             │  ← Geschäftslogik (Services, DTOs, Interfaces)
├──────────────────────────────────────────────────┤
│             LeasiNetWeb.Infrastructure           │  ← Datenzugriff (EF Core DbContext, Seeder, Jobs)
├──────────────────────────────────────────────────┤
│                LeasiNetWeb.Domain                │  ← Domänenmodell (Entities, Enums)
└──────────────────────────────────────────────────┘
```

**Abhängigkeitsrichtung:** Web → Application → Domain ← Infrastructure

### Domänen-Entitäten

| Entität | Beschreibung |
|---|---|
| `Benutzer` | Systembenutzer mit Rolle, Leasinggesellschafts-Zuordnung |
| `Leasingantrag` | Kernentität – ein Leasingantrag mit Status-Machine |
| `Leasingobjekt` | Gerät/Anlage als Gegenstand des Leasings |
| `Vertrag` | Aktiver Leasingvertrag, erstellt aus genehmigtem Antrag |
| `Leasinggesellschaft` | Leasinggeber mit Obligo-Limit und Kontaktdaten |
| `InternePruefung` | Compliance-Prüfung mit zugehörigen Pflichten |
| `PruefungsPflicht` | Einzelne Prüfungsposition (Checkliste) |
| `Obligo` | Obligoeintrag pro Antrag und Gesellschaft |
| `Nachricht` | Interne Nachricht zwischen Benutzern |
| `Ereignis` | Audit-Event mit Benachrichtigungs-Tracking |
| `Kommentar` | Kommentar zu einem Antrag (intern/extern) |
| `Anhang` | Dateianhang (Antragsdokument, Vertragsdokument, Prüfungsdokument) |
| `PruefungsSchritt` | Einzelner sequentieller Prüfschritt innerhalb einer internen Kontrolle (verschiedene Prüfer möglich) |
| `DokumentAustausch` | Dokumentenaustausch-Eintrag (eingehend/ausgehend) für Anträge und Verträge |
| `LgRegistrierung` | Onboarding-Registrierung einer Leasinggesellschaft (Kontaktperson, Status, Abschluss) |
| `SynchronisierungsAnfrage` | Verarbeitungsprotokoll für externe Synchronisierungsanfragen |
| `Ablehnungsgrund` | Stammdatum für Ablehnungsgründe |
| `Geraetetyp` | Stammdatum für Gerätekategorien |
| `Vertragstyp` | Stammdatum für Vertragsarten (KFZ, IT, Maschinen, Immobilien) |
| `Ratentabelle` / `Rate` | Ratentabellen für Leasingkonditionen |
| `Selbstkompetenz` | Genehmigungslimits pro Benutzer |
| `HilfeText` | Verwaltbare Hilfetexte pro Seite/Bereich |

---

## Projektstruktur

```
LeasiNetWeb_InternesPortal_V2/
├── LeasiNetWeb.sln                    # Visual Studio Solution
├── Dockerfile                         # Multi-Stage Docker Build
├── railway.json                       # Railway Deployment-Konfiguration
├── global.json                        # .NET SDK Version (8.0)
├── docs/
│   └── Anwenderdokumentation.md       # Anwenderdokumentation
└── src/
    ├── LeasiNetWeb.Domain/            # Domänenmodell
    │   ├── Entities/                  # Geschäftsentitäten
    │   └── Enums/                     # Status- und Typ-Enumerationen
    ├── LeasiNetWeb.Application/       # Anwendungslogik
    │   ├── DTOs/                      # Data Transfer Objects
    │   ├── Interfaces/                # Service-Verträge
    │   └── Services/                  # Service-Implementierungen
    ├── LeasiNetWeb.Infrastructure/    # Infrastruktur
    │   ├── Data/                      # EF Core DbContext, Seeder, Anhang-Service
    │   └── Jobs/                      # Hangfire Hintergrund-Jobs
    └── LeasiNetWeb.Web/               # Web-Präsentation
        ├── Controllers/               # MVC Controller
        │   ├── AiAssistentController.cs   # KI-Assistent (OpenAI-Integration)
        │   └── …                      # Account, Admin, Antraege, Vertraege, …
        ├── Views/                     # Razor Views
        ├── ViewModels/                # View-spezifische Modelle
        ├── wwwroot/                   # Statische Dateien (CSS, JS, Bilder)
        │   └── css/merkur.css         # Eigenes MerkurConnect Design-System
        └── Program.cs                 # Anwendungs-Einstiegspunkt
```

---

## Voraussetzungen

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (oder höher via `rollForward: latestFeature`)
- Optional: [Docker](https://www.docker.com/) für Container-Deployment
- Optional: PostgreSQL oder SQL Server (SQLite wird als Fallback automatisch verwendet)

---

## Installation & Start

### Lokale Entwicklung

```bash
# Repository klonen
git clone https://github.com/andershow88/LeasiNetWeb_InternesPortal_V2.git
cd LeasiNetWeb_InternesPortal_V2

# Abhängigkeiten wiederherstellen und starten
dotnet restore
dotnet run --project src/LeasiNetWeb.Web
```

Die Anwendung ist danach unter `https://localhost:5001` (oder `http://localhost:5000`) erreichbar.

Beim ersten Start wird automatisch eine SQLite-Datenbank (`leasinetweb.db`) erstellt und mit Stamm- und Demo-Daten befüllt.

### Mit Docker

```bash
docker build -t leasinetweb .
docker run -p 8080:8080 -e PORT=8080 leasinetweb
```

---

## Konfiguration

### Datenbank

Die Datenbankverbindung wird in folgender Priorität ausgewählt:

1. **`DATABASE_URL`** (Umgebungsvariable) – PostgreSQL-URL im Format `postgres://user:pass@host:port/db` (Railway-Standard)
2. **`ConnectionStrings:DefaultConnection`** in `appsettings.json` – SQL Server
3. **SQLite-Fallback** – `Data Source=leasinetweb.db`

### Umgebungsvariablen

| Variable | Beschreibung | Beispiel |
|---|---|---|
| `DATABASE_URL` | PostgreSQL-Verbindungsstring (Railway-Format) | `postgres://user:pass@host:5432/dbname` |
| `PORT` | HTTP-Port (Railway injiziert diesen automatisch) | `8080` |
| `DOTNET_EnableDiagnostics` | .NET Diagnostics deaktivieren (Container) | `0` |
| `OPENAI_API_KEY` | API-Schlüssel für den KI-Assistenten (OpenAI) | `sk-…` |

> **Hinweis KI-Assistent:** Alternativ kann `OpenAiApiKey` in `appsettings.json` gesetzt werden. Ohne gültigen Key gibt der KI-Assistent eine entsprechende Fehlermeldung aus, alle anderen Funktionen bleiben unberührt.

---

## Deployment

### Railway

Das Projekt enthält eine vorkonfigurierte `railway.json`:

- **Builder:** Dockerfile
- **Restart-Policy:** Bei Fehler, max. 3 Retries

Deployment erfolgt automatisch bei Push auf den konfigurierten Branch.

### Andere Plattformen

Jede Plattform, die Docker unterstützt, kann verwendet werden. Das Dockerfile nutzt ein Multi-Stage-Build:

1. **Build-Stage:** .NET SDK 8.0 – kompiliert und publiziert
2. **Runtime-Stage:** .NET ASP.NET Runtime 8.0 – minimaler Container

---

## Demo-Zugang

Beim ersten Start werden folgende Demo-Benutzer erstellt:

| Benutzername | Passwort | Rolle |
|---|---|---|
| `admin` | `Admin1234!` | Administrator |
| `sachbearbeiter.mb` | `Demo1234!` | SachbearbeiterMB |
| `genehmiger.mb` | `Demo1234!` | Genehmiger |
| `sachbearbeiter.lg` | `Demo1234!` | SachbearbeiterLG |
| `pruefer.mb` | `Demo1234!` | InternerPrüfer |

Zusätzlich werden 100 Demo-Leasinganträge mit zugehörigen Verträgen und Leasingobjekten generiert.

> **Hinweis:** Demo-Daten können jederzeit über **Admin → Demo-Daten laden** neu generiert werden.

---

## Dokumentation

- **[Anwenderdokumentation](docs/Anwenderdokumentation.md)** – Ausführliche Benutzeranleitung für alle Module

> Die Anwenderdokumentation wird fortlaufend aktualisiert. Bei Änderungen an der Benutzeroberfläche oder neuen Funktionen muss die Dokumentation in `docs/Anwenderdokumentation.md` entsprechend angepasst werden.

---

## Lizenz

Dieses Projekt ist proprietäre Software. Alle Rechte vorbehalten.

---

## Changelog

### v2.x – April 2026

#### Neu: KI-Assistent in der Topbar
- Neue Suchleiste in der Topbar ersetzt den Breadcrumb-Bereich
- Eingabe einer Frage + **Enter** öffnet ein Chat-Modal unterhalb der Topbar
- Kontextbewusste Antworten dank Echtzeit-Datenbankabfragen (Statistiken, Anträge, Benutzer, Leasinggesellschaften)
- Folgefragen im Modal möglich; jede neue Topbar-Anfrage startet eine frische Konversation
- Backend: `AiAssistentController` → OpenAI `gpt-4o-mini` via `IHttpClientFactory`
- Konfiguration: Umgebungsvariable `OPENAI_API_KEY` oder `OpenAiApiKey` in `appsettings.json`

#### Neu: Dark Mode
- Umschalter (☀/🌙) in der Topbar (rechts neben dem Benutzernamen)
- Wechsel zwischen hellem und dunklem Theme; Einstellung wird in `localStorage` gespeichert

#### Topbar-Redesign
- KI-Suchleiste nimmt die gesamte Topbar-Mitte ein (Breadcrumb entfernt)
- Dark-Mode-Icon und Benutzername wieder korrekt rechtsbündig (`flex-shrink:0`)
- Suchleiste verbreitert, größere Schrift und mehr Padding für bessere Bedienung

#### Neue Domänen-Entitäten
- `PruefungsSchritt` – sequentieller Prüfschritt innerhalb einer internen Kontrolle (mehrere Prüfer möglich)
- `DokumentAustausch` – Dokumentenaustausch-Protokoll (eingehend/ausgehend) für Anträge und Verträge
- `LgRegistrierung` – Onboarding-Registrierung einer Leasinggesellschaft

#### Erweiterter Hintergrund-Job
- Dritter Bereinigungsjob: **Verwaiste Dateien bereinigen** – entfernt täglich Upload-Dateien ohne Datenbankeintrag
