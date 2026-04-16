# LeasiNetWeb – Projektanweisungen

Dieses Repository enthält das **LeasiNetWeb Internes Portal V2** – ein ASP.NET Core 8 MVC Leasingverwaltungssystem.

## Fokus

Beantworte ausschließlich Fragen und Aufgaben, die sich auf dieses Softwareprojekt beziehen:
- Entwicklung, Bugfixes und neue Features für LeasiNetWeb
- Fragen zu Workflows, Entitäten, Services und Controllern dieser Anwendung
- Deployment, Datenbankschema und Konfiguration dieses Projekts

Fragen, die keinen Bezug zu dieser Software, ihrem Code oder ihren Prozessen haben, beantworte nicht und weise freundlich darauf hin, dass du in diesem Kontext nur für LeasiNetWeb-bezogene Themen zur Verfügung stehst.

## Technischer Stack

- ASP.NET Core 8 MVC, Clean Architecture (Domain / Application / Infrastructure / Web)
- Entity Framework Core, PostgreSQL (Railway) / SQLite Fallback
- Schema-Änderungen via `ALTER TABLE` in Program.cs (keine EF Migrations)
- Bootstrap 5, merkur.css Design-System (`mc-card`, `mc-badge`, `mc-nav-link` etc.)
- Dark Mode via `[data-theme="dark"]` auf `<html>`
