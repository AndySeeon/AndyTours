@docs/product.md
@docs/tech.md
@docs/structure.md

In diesem Projekt bauen wir AndyTours, eine Touren- Verwaltung für Radltouren

## Tech Stack
Programmiersprache: C#
- Plattform: .NET 10 (LTS), App `net10.0-windows`, übrige Projekte `net10.0`
- Oberfläche: WPF mit MVVM (`CommunityToolkit.Mvvm`)
- Datenhaltung: SQLite über `Microsoft.Data.Sqlite`
- FIT-Import: `Garmin.FIT.Sdk`
- Tests: xUnit
- Diagramme: OxyPlot (`OxyPlot.Wpf`), siehe `docs/adr/0001-diagrammbibliothek.md`
- Mediengeräte (Edge 1040 per MTP): `MediaDevices`, siehe `docs/adr/0002-zugriff-auf-mediengeraete.md`
- Bibliothek für die Karte wird erst bei der jeweiligen Funktion festgelegt

## Werkzeuge (Entwicklungsumgebung)
| Werkzeug | Zweck | Installation |
|---|---|---|
| .NET SDK 10.0.4xx | Bauen, Testen (`dotnet build`, `dotnet test`); per `global.json` festgelegt, Vorabversionen (z. B. .NET 11 RC) werden nicht verwendet | `winget install Microsoft.DotNet.SDK.10` |
| Git | Versionsverwaltung | `winget install Git.Git` |
| Visual Studio 2026 (Workload „.NET-Desktopentwicklung") | WPF-Designer, Debugging | Visual Studio Installer |
| VS Code (optional) | Bearbeiten von Code und Doku | `winget install Microsoft.VisualStudioCode` |
| NuGet | Paketquelle nuget.org, wird über das SDK automatisch genutzt | – |

Befehle (im Projektordner):
- Bauen: `dotnet build AndyTours.slnx`
- Testen: `dotnet test AndyTours.slnx`
- Starten: `dotnet run --project src/AndyTours.App`
 
 ## Arbeitsweise
 Jede Aufgabe bekommt eine eigene Datei: eigenständige Fachfunktionen einzeln, zusammengehörige Funktionen (z. B. Laden und Speichern derselben Tabelle oder Einstellung) gemeinsam in einer Klasse. Alles wird ausführlich mit Kommentaren in deutscher Sprache erläutert. Details in `docs/Konventionen.md`.