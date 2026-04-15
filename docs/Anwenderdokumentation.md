# LeasiNetWeb – Anwenderdokumentation

> **Zielgruppe:** Alle Anwender des LeasiNetWeb Internen Portals  
> **Stand:** April 2026  
> **Hinweis:** Diese Dokumentation muss bei Änderungen an der Benutzeroberfläche oder neuen Funktionen aktualisiert werden.

---

## Inhaltsverzeichnis

1. [Anmeldung und Abmeldung](#1-anmeldung-und-abmeldung)
2. [Dashboard](#2-dashboard)
3. [Leasinganträge](#3-leasinganträge)
   - [Anträge – Übersicht](#31-anträge--übersicht)
   - [Neuen Antrag erstellen](#32-neuen-antrag-erstellen)
   - [Antrag-Details und Bearbeitung](#33-antrag-details-und-bearbeitung)
   - [Status-Aktionen](#34-status-aktionen)
   - [Kommentare](#35-kommentare)
   - [Dateianhänge](#36-dateianhänge)
   - [Archiv](#37-archiv)
4. [Verträge](#4-verträge)
   - [Verträge – Übersicht](#41-verträge--übersicht)
   - [Vertrag erstellen](#42-vertrag-erstellen)
   - [Vertrag bearbeiten](#43-vertrag-bearbeiten)
   - [Vertrags-Statusaktionen](#44-vertrags-statusaktionen)
5. [Interne Prüfung](#5-interne-prüfung)
   - [Prüfungs-Übersicht](#51-prüfungs-übersicht)
   - [Prüfung starten und durchführen](#52-prüfung-starten-und-durchführen)
   - [Prüfung abschließen](#53-prüfung-abschließen)
6. [Nachrichten](#6-nachrichten)
   - [Posteingang](#61-posteingang)
   - [Neue Nachricht senden](#62-neue-nachricht-senden)
7. [Auswertung](#7-auswertung)
   - [Kennzahlen-Dashboard](#71-kennzahlen-dashboard)
   - [CSV-Export](#72-csv-export)
8. [Hilfe](#8-hilfe)
9. [Administration](#9-administration)
   - [Benutzerverwaltung](#91-benutzerverwaltung)
   - [Leasinggesellschaften](#92-leasinggesellschaften)
   - [Stammdaten](#93-stammdaten)
   - [Demo-Daten](#94-demo-daten)
   - [Hangfire-Dashboard](#95-hangfire-dashboard)
10. [Rollen und Berechtigungen](#10-rollen-und-berechtigungen)
11. [Häufige Fragen (FAQ)](#11-häufige-fragen-faq)

---

## 1. Anmeldung und Abmeldung

### Anmelden

1. Öffnen Sie die Anwendung in Ihrem Browser.
2. Sie werden auf die **Login-Seite** weitergeleitet, sofern Sie noch nicht angemeldet sind.
3. Geben Sie Ihren **Benutzernamen** und Ihr **Passwort** ein.
4. Optional: Aktivieren Sie **„Angemeldet bleiben"**, um bei erneutem Besuch automatisch angemeldet zu bleiben.
5. Klicken Sie auf **Anmelden**.

Nach erfolgreicher Anmeldung werden Sie zum **Dashboard** weitergeleitet.

> **Hinweis:** Nach 8 Stunden Inaktivität wird die Sitzung automatisch beendet. Bei aktiviertem „Angemeldet bleiben" verlängert sich die Sitzung bei jeder Aktivität.

### Abmelden

- Klicken Sie in der Navigation oben rechts auf **Abmelden**.
- Sie werden zur Login-Seite zurückgeleitet.

### Zugriff verweigert

Wenn Sie versuchen, auf einen Bereich zuzugreifen, für den Sie keine Berechtigung haben, wird die Seite **„Zugriff verweigert"** angezeigt. Wenden Sie sich in diesem Fall an Ihren Administrator.

---

## 2. Dashboard

Das Dashboard ist die **Startseite** nach der Anmeldung und bietet einen schnellen Überblick über Ihre wichtigsten Kennzahlen und anstehenden Aufgaben.

### Angezeigte Informationen

Je nach Ihrer Rolle sehen Sie:

- **Anzahl offener Anträge** – Anträge, die auf Bearbeitung warten
- **Anträge zur Prüfung** – Anträge, die Ihnen zur Sachbearbeitung zugewiesen sind
- **Anträge zur Genehmigung** – Anträge, die auf Ihre Genehmigung warten
- **Zweite Voten** – Anträge, die eine zweite Genehmigungsstimme benötigen
- **Ungelesene Nachrichten** – Anzahl neuer Nachrichten in Ihrem Posteingang

Die einzelnen Kennzahlen sind **klickbar** und führen direkt zur jeweiligen gefilterten Ansicht.

---

## 3. Leasinganträge

### 3.1 Anträge – Übersicht

Unter **Anträge** sehen Sie die Liste aller Leasinganträge, zu denen Sie Zugriff haben.

#### Filteroptionen

- **Nach Status filtern** – Zeigen Sie nur Anträge eines bestimmten Status an (z. B. „Eingereicht", „In Prüfung", „Genehmigt")
- **Schnellfilter:**
  - **Meine Prüfungen** – Anträge, die Ihnen als Sachbearbeiter zugewiesen sind
  - **Meine Genehmigungen** – Anträge, die auf Ihre Genehmigung warten
  - **Meine Zweiten Voten** – Anträge, bei denen eine zweite Stimme benötigt wird

#### Angezeigte Spalten

| Spalte | Beschreibung |
|---|---|
| Antragsnummer | Eindeutige Nummer im Format `LNW-JJJJ-NNNNN` |
| Typ | Neugeschäft, Prolongation, Ablösung oder Rahmenvertrag |
| Status | Aktueller Bearbeitungsstatus |
| Eingereicht von | Name des Antragstellers |
| Leasinggesellschaft | Zugeordnete Leasinggesellschaft |
| Sachbearbeiter | Zugewiesener Sachbearbeiter MB |
| Obligo | Obligo-Betrag in Euro |
| Erstellt am | Datum der Erstellung |

### 3.2 Neuen Antrag erstellen

1. Klicken Sie auf **„Neuer Antrag"** in der Antragsübersicht.
2. Füllen Sie das Formular aus:
   - **Antragstyp** – Wählen Sie den Typ (Neugeschäft, Prolongation, Ablösung, Rahmenvertrag)
   - **Leasinggesellschaft** – Wählen Sie die gewünschte Leasinggesellschaft aus der Dropdown-Liste
   - **Obligo** – Geben Sie den Obligo-Betrag ein
   - **Abrechnungsart** – Wählen Sie Monatlich, Quartalsweise oder Jährlich
3. Klicken Sie auf **Speichern**.

Der Antrag wird im Status **„Entwurf"** erstellt. Sie können ihn anschließend weiter bearbeiten, bevor Sie ihn einreichen.

### 3.3 Antrag-Details und Bearbeitung

Klicken Sie in der Antragsübersicht auf eine Antragsnummer, um die **Detailansicht** zu öffnen.

Die Detailseite zeigt:

- **Stammdaten** – Antragsnummer, Typ, Status, Leasinggesellschaft, Obligo
- **Beteiligte** – Eingereicht von, Sachbearbeiter MB/LG, Genehmiger
- **Leasingobjekte** – Zugehörige Geräte und Anlagen
- **Kommentare** – Interne und externe Kommentare
- **Anhänge** – Hochgeladene Dokumente
- **Ereignishistorie** – Vollständige Protokollierung aller Statusänderungen und Aktionen

### 3.4 Status-Aktionen

Je nach aktuellem Status und Ihrer Rolle stehen verschiedene Aktionen zur Verfügung:

| Aktion | Voraussetzung | Beschreibung |
|---|---|---|
| **Einreichen** | Status: Entwurf | Antrag zur Bearbeitung einreichen |
| **In Prüfung nehmen** | Status: Eingereicht | Antrag zur Sachbearbeitung übernehmen |
| **Prüfung abschließen** | Rolle: SachbearbeiterMB | Sachbearbeitung abschließen, Antrag geht an Mitarbeiter |
| **Genehmigen** | Rolle: Genehmiger | Antrag genehmigen |
| **Ablehnen** | Rolle: Genehmiger | Antrag mit Ablehnungsgrund und optionalem Kommentar ablehnen |
| **Zweite Vote anfordern** | Rolle: Genehmiger | Zweite Genehmigungsstimme anfordern |
| **Archivieren** | Alle berechtigten Rollen | Antrag ins Archiv verschieben |

#### Antrag ablehnen

1. Klicken Sie auf **Ablehnen** in der Detailansicht.
2. Wählen Sie einen **Ablehnungsgrund** aus der Dropdown-Liste.
3. Geben Sie optional einen **Kommentar** ein.
4. Bestätigen Sie die Ablehnung.

#### Zweite Vote anfordern

Wenn ein Antrag eine zweite Genehmigungsstimme benötigt (z. B. bei höheren Beträgen), kann der Genehmiger eine **Zweite Vote** anfordern. Der Antrag geht dann in den Status „Zweite Vote erforderlich" und ein weiterer Genehmiger muss zustimmen.

### 3.5 Kommentare

Sie können Kommentare zu einem Antrag hinzufügen:

1. Scrollen Sie in der Detailansicht zum Bereich **Kommentare**.
2. Geben Sie Ihren Kommentartext ein.
3. Wählen Sie, ob der Kommentar **intern** ist (nur für interne Mitarbeiter sichtbar) oder nicht.
4. Klicken Sie auf **Kommentar hinzufügen**.

### 3.6 Dateianhänge

#### Datei hochladen

1. Scrollen Sie in der Detailansicht zum Bereich **Anhänge**.
2. Klicken Sie auf **Datei auswählen** und wählen Sie die gewünschte Datei.
3. Klicken Sie auf **Hochladen**.

#### Datei herunterladen

Klicken Sie auf den Dateinamen des gewünschten Anhangs. Die Datei wird heruntergeladen.

### 3.7 Archiv

Unter **Anträge → Archiv** finden Sie alle archivierten Anträge. Archivierte Anträge können eingesehen, aber nicht mehr bearbeitet werden.

Anträge werden automatisch archiviert, wenn sie seit mehr als 24 Monaten im Status „Genehmigt", „Abgelehnt" oder „Storniert" sind (automatischer Hintergrund-Job).

---

## 4. Verträge

### 4.1 Verträge – Übersicht

Unter **Verträge** sehen Sie alle Leasingverträge mit ihrem aktuellen Status.

#### Filteroptionen

- **Nach Status filtern** – In Vorbereitung, Aktiv, Beendet, Gekündigt, Archiviert

#### Angezeigte Informationen

| Spalte | Beschreibung |
|---|---|
| Vertragsnummer | Eindeutige Nummer im Format `VTG-JJJJ-NNNNN` |
| Status | Aktueller Vertragsstatus |
| Antragsnummer | Zugehöriger Leasingantrag |
| Finanzierungsbetrag | Finanzierungssumme in Euro |
| Vertragsbeginn/-ende | Laufzeit des Vertrags |

### 4.2 Vertrag erstellen

Ein Vertrag kann nur aus einem **genehmigten Antrag** heraus erstellt werden:

1. Öffnen Sie die Detailansicht eines genehmigten Antrags.
2. Klicken Sie auf **Vertrag erstellen**.
3. Der Vertrag wird im Status **„In Vorbereitung"** angelegt.

### 4.3 Vertrag bearbeiten

1. Öffnen Sie die Vertrags-Detailansicht und klicken Sie auf **Bearbeiten**.
2. Bearbeitbare Felder:
   - **Vertragstyp** – KFZ-Leasing, Maschinenleasing, IT-Leasing, Immobilienleasing
   - **Vertragsbeginn** und **Vertragsende**
   - **Laufzeit (Monate)**
   - **Finanzierungsbetrag**, **Restwert**, **Monatliche Rate**, **Zinssatz**
3. Klicken Sie auf **Speichern**.

### 4.4 Vertrags-Statusaktionen

| Aktion | Beschreibung |
|---|---|
| **Aktivieren** | Vertrag aktivieren (Wechsel zu „Aktiv") |
| **Beenden** | Vertrag regulär beenden |
| **Kündigen** | Vertrag vorzeitig kündigen |
| **Archivieren** | Vertrag archivieren |

#### Dateianhänge an Verträgen

Analog zu Leasinganträgen können auch an Verträgen Dokumente hochgeladen und heruntergeladen werden (Vertragsdokumente).

---

## 5. Interne Prüfung

Die interne Prüfung dient der **Compliance-Kontrolle** von Leasinganträgen. Dieses Modul ist nur für Benutzer mit der Rolle **InternerPrüfer** und **Administrator** zugänglich.

### 5.1 Prüfungs-Übersicht

Unter **Interne Prüfung** sehen Sie:

- **Administratoren:** Alle laufenden Prüfungen
- **Interne Prüfer:** Nur die Ihnen zugewiesenen Prüfungen

### 5.2 Prüfung starten und durchführen

1. Öffnen Sie einen Antrag, der den Status **„Interne Kontrolle erforderlich"** hat.
2. Klicken Sie auf **Prüfung starten**.
3. In der Prüfungs-Detailansicht sehen Sie eine **Checkliste** mit Prüfungspflichten.
4. Für jede Pflicht können Sie:
   - **Pflicht erfüllen** – mit optionalen Bemerkungen
   - **Rückgängig machen** – falls die Pflicht doch nicht erfüllt ist
5. Laden Sie bei Bedarf **Prüfungsdokumente** hoch.

### 5.3 Prüfung abschließen

1. Wenn alle relevanten Pflichten erfüllt sind, klicken Sie auf **Prüfung abschließen**.
2. Geben Sie optional ein **Ergebnis/Fazit** ein.
3. Der zugehörige Antrag geht zurück in den Status **„Bei Mitarbeiter"**.

---

## 6. Nachrichten

Das interne Nachrichtensystem ermöglicht die Kommunikation zwischen Benutzern – optional mit Bezug zu einem Leasingantrag.

### 6.1 Posteingang

Unter **Nachrichten** sehen Sie Ihren Posteingang:

- **Ungelesene Nachrichten** sind hervorgehoben
- Klicken Sie auf eine Nachricht, um sie zu öffnen (wird automatisch als gelesen markiert)
- Nachrichten zeigen: Absender, Betreff, Text, Zeitpunkt, optionaler Antragsbezug

### 6.2 Neue Nachricht senden

1. Klicken Sie auf **Neue Nachricht**.
2. Wählen Sie einen **Empfänger** aus der Dropdown-Liste (nur aktive Benutzer).
3. Optional: Wählen Sie einen **Leasingantrag** als Bezug.
4. Geben Sie **Betreff** und **Text** ein.
5. Klicken Sie auf **Senden**.

---

## 7. Auswertung

Das Auswertungsmodul bietet Kennzahlen und Export-Funktionen.

### 7.1 Kennzahlen-Dashboard

Unter **Auswertung** sehen Sie eine Jahresauswertung mit:

- Anzahl Anträge pro Status
- Summe der Obligos
- Anzahl Verträge nach Status
- Finanzierungsvolumen

**Jahresfilter:** Wählen Sie das gewünschte Jahr aus dem Dropdown (letzten 5 Jahre verfügbar).

### 7.2 CSV-Export

Für weiterführende Analysen stehen CSV-Exporte zur Verfügung:

- **Anträge als CSV** – Alle Leasinganträge (optional nach Jahr gefiltert)
  - Enthält: Id, Antragsnummer, Typ, Status, Eingereicht von, Leasinggesellschaft, Sachbearbeiter, Obligo, Erstellt am, Geändert am
- **Verträge als CSV** – Alle Verträge
  - Enthält: Id, Vertragsnummer, Status, Antragsnummer, Leasinggesellschaft, Vertragstyp, Finanzierungsbetrag, Vertragsbeginn, Vertragsende, Laufzeit

> **Hinweis:** Die CSV-Dateien verwenden Semikolon (`;`) als Trennzeichen und sind UTF-8-kodiert (mit BOM). Sie können direkt in Microsoft Excel geöffnet werden.

---

## 8. Hilfe

Unter **Hilfe** finden Sie kontextbezogene Hilfetexte zu verschiedenen Bereichen der Anwendung. Die Hilfetexte werden vom Administrator gepflegt und können HTML-Formatierung enthalten.

Die Hilfeseite kann optional mit einem Schlüssel aufgerufen werden, um direkt den passenden Hilfetext anzuzeigen (z. B. `?schluessel=dashboard`).

---

## 9. Administration

Der Administrationsbereich ist nur für Benutzer mit der Rolle **Administrator** zugänglich.

### 9.1 Benutzerverwaltung

Unter **Admin → Benutzer** verwalten Sie die Systembenutzer.

#### Benutzer anlegen

1. Klicken Sie auf **Neuer Benutzer**.
2. Füllen Sie das Formular aus:
   - **Benutzername** (eindeutig, für die Anmeldung)
   - **Vorname** und **Nachname**
   - **E-Mail-Adresse**
   - **Rolle** – Mitarbeiter, SachbearbeiterMB, SachbearbeiterLG, Genehmiger, InternerPrüfer, Administrator, Auswerter
   - **Leasinggesellschaft** (optional, nur relevant für SachbearbeiterLG)
   - **Passwort** (Pflichtfeld beim Erstellen)
   - **Aktiv** – Deaktivierte Benutzer können sich nicht anmelden
3. Klicken Sie auf **Speichern**.

#### Benutzer bearbeiten

1. Klicken Sie auf den Benutzer in der Liste.
2. Ändern Sie die gewünschten Felder.
3. Das Passwortfeld kann leer gelassen werden – das bestehende Passwort bleibt erhalten.
4. Klicken Sie auf **Speichern**.

#### Benutzer aktivieren / deaktivieren

- Klicken Sie in der Benutzerliste auf **Deaktivieren** bzw. **Aktivieren**.
- Deaktivierte Benutzer können sich nicht mehr anmelden, bleiben aber im System erhalten.

### 9.2 Leasinggesellschaften

Unter **Admin → Leasinggesellschaften** verwalten Sie die Leasinggeber.

#### Felder

| Feld | Beschreibung |
|---|---|
| Name | Vollständiger Name der Gesellschaft |
| Kurzbezeichnung | Kürzel (z. B. „MB-AG") |
| Adresse | Straße, PLZ, Ort, Land |
| Kontakt | Telefon, E-Mail, Ansprechpartner |
| Obligo-Limit | Maximales Obligo in Euro |
| Aktiv | Deaktivierte Gesellschaften stehen bei neuen Anträgen nicht zur Auswahl |

### 9.3 Stammdaten

Unter **Admin → Stammdaten** verwalten Sie:

#### Ablehnungsgründe

- **Hinzufügen:** Code und Bezeichnung eingeben, auf Speichern klicken
- **Bearbeiten:** Vorhandenen Ablehnungsgrund ändern
- **Löschen:** Ablehnungsgrund entfernen (nur wenn nicht in Verwendung)

Vordefinierte Ablehnungsgründe:
- Unzureichende Bonität
- Unvollständige Unterlagen
- Obligo-Limit überschritten
- Sonstiger Grund

#### Gerätetypen

- Hierarchische Kategorisierung von Leasingobjekten
- **Hinzufügen:** Bezeichnung und optional Beschreibung und Elterntyp eingeben
- **Bearbeiten** und **Löschen** analog zu Ablehnungsgründen

Vordefinierte Gerätetypen:
- Fahrzeuge
- Maschinen & Anlagen
- IT-Ausstattung
- Medizintechnik

### 9.4 Demo-Daten

Unter **Admin → Demo-Daten laden** können alle bestehenden Anträge und Verträge gelöscht und durch 100 Demo-Anträge mit zugehörigen Verträgen ersetzt werden.

> **Achtung:** Diese Aktion löscht alle vorhandenen Leasinganträge und Verträge unwiderruflich! Nutzen Sie diese Funktion nur in Test- und Demonstrationsumgebungen.

### 9.5 Hangfire-Dashboard

Unter `/hangfire` ist das Hangfire-Dashboard erreichbar (nur für Administratoren). Hier können Sie:

- Laufende und geplante Hintergrund-Jobs einsehen
- Fehlgeschlagene Jobs analysieren und erneut starten
- Wiederkehrende Jobs überwachen:
  - **antraege-archivieren** – monatlich
  - **sync-anfragen-bereinigen** – täglich um 03:00 Uhr

---

## 10. Rollen und Berechtigungen

### Übersicht

| Rolle | Dashboard | Anträge | Verträge | Prüfung | Nachrichten | Auswertung | Admin |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Mitarbeiter | ✅ | Eigene | – | – | ✅ | – | – |
| SachbearbeiterMB | ✅ | Alle (bearbeiten) | ✅ | – | ✅ | – | – |
| SachbearbeiterLG | ✅ | Zugewiesene | – | – | ✅ | – | – |
| Genehmiger | ✅ | Alle (genehmigen) | ✅ | – | ✅ | – | – |
| InternerPrüfer | ✅ | Einsehen | – | ✅ | ✅ | – | – |
| Auswerter | ✅ | Einsehen | Einsehen | – | ✅ | ✅ | – |
| Administrator | ✅ | Alle | ✅ | ✅ | ✅ | ✅ | ✅ |

### Autorisierungs-Policies

Die Anwendung verwendet die folgenden Autorisierungs-Policies:

- **Genehmiger** – Zugriff für Rollen „Genehmiger" und „Administrator"
- **Administrator** – Zugriff nur für Rolle „Administrator"
- **InternerPruefer** – Zugriff für Rollen „InternerPrüfer" und „Administrator"
- **Auswerter** – Zugriff für Rollen „Auswerter" und „Administrator"

---

## 11. Häufige Fragen (FAQ)

### Ich habe mein Passwort vergessen. Was kann ich tun?

Wenden Sie sich an einen Administrator. Dieser kann Ihr Passwort über **Admin → Benutzer → Bearbeiten** zurücksetzen.

### Warum sehe ich bestimmte Menüpunkte nicht?

Die sichtbaren Menüpunkte hängen von Ihrer **Rolle** ab. Sprechen Sie mit Ihrem Administrator, wenn Sie zusätzliche Berechtigungen benötigen.

### Kann ich einen bereits eingereichten Antrag zurücknehmen?

Anträge können nicht direkt zurückgezogen werden. Wenden Sie sich an einen Sachbearbeiter oder Administrator, um den Antrag zu stornieren.

### Wie lange werden archivierte Anträge aufbewahrt?

Archivierte Anträge bleiben dauerhaft im System erhalten und können jederzeit über das Archiv eingesehen werden. Es findet keine automatische Löschung statt.

### Warum kann ich keinen Vertrag erstellen?

Ein Vertrag kann nur aus einem **genehmigten** Antrag heraus erstellt werden. Stellen Sie sicher, dass der Antrag den Status „Genehmigt" hat.

### Wie funktioniert die Zweite Vote?

Wenn ein Antrag eine zweite Genehmigung erfordert (z. B. bei hohen Obligo-Beträgen), kann der erste Genehmiger eine **Zweite Vote anfordern**. Ein weiterer Genehmiger muss dann den Antrag zusätzlich freigeben.

### In welchem Format werden CSV-Exporte erstellt?

CSV-Dateien verwenden **Semikolon (`;`)** als Trennzeichen und sind **UTF-8 mit BOM** kodiert. Sie können direkt in Microsoft Excel geöffnet werden, ohne dass ein Import-Assistent nötig ist.

---

> **Diese Dokumentation wird aktiv gepflegt.** Bei Fragen oder wenn Sie Fehler finden, wenden Sie sich bitte an das Entwicklungsteam oder erstellen Sie ein Issue im [GitHub-Repository](https://github.com/andershow88/LeasiNetWeb_InternesPortal_V2).
