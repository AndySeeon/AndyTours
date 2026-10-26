# Technik

## Sprachen
- C# (Sprachversion `latest`, Nullable aktiviert)

## Frameworks & Bibliotheken
- .NET 10 (LTS); App `net10.0-windows10.0.19041.0`, übrige Projekte `net10.0`
- WPF mit MVVM über `CommunityToolkit.Mvvm`
- SQLite über `Microsoft.Data.Sqlite`
- `Garmin.FIT.Sdk` für FIT-Dateien
- Diagramme: OxyPlot (`OxyPlot.Wpf`), siehe `docs/adr/0001-diagrammbibliothek.md`
- Zugriff auf Mediengeräte (Garmin Edge 1040 per MTP): `MediaDevices`, nur im App-Projekt, siehe `docs/adr/0002-zugriff-auf-mediengeraete.md`
- Karte: Mapsui (`Mapsui.Wpf`), nur im App-Projekt, siehe `docs/adr/0003-kartenbibliothek.md`
- Routing (Tourenplanung): BRouter (voreingestellt) oder openrouteservice, wählbar in den Einstellungen, siehe `docs/adr/0004-routingdienst.md`

## Werkzeuge
- .NET SDK 10.0.4xx, festgelegt in `global.json` (keine Vorabversionen)
- NuGet (nuget.org)
- xUnit (Tests)
- Formatierung nach `.editorconfig`
- Codeprüfung: eingebauter .NET-Analyzer (`AnalysisLevel`, `EnforceCodeStyleInBuild` in `Directory.Build.props`); Meldungen als Warnungen beim Build
- Git
- Visual Studio 2026 (Workload „.NET-Desktopentwicklung“), optional VS Code

## Code-Regeln
- Siehe `docs/Konventionen.md`
- Eine Datei je Aufgabe: eigenständige Fachfunktionen als `static class` mit einer öffentlichen Methode `Ausfuehren(...)`; zusammengehörige Funktionen (Laden/Speichern derselben Tabelle oder Einstellung, Hin- und Rückumwandlung) gemeinsam in einer `static class` (z. B. `FensterpositionEinstellung.Laden/Speichern`)
- Ausführliche Kommentare auf Deutsch (XML-Doc)
- Bezeichner deutsch ohne Umlaute; Oberfläche und Kommentare mit Umlauten
- Fachfunktionen erhalten Unit-Tests

## Befehle
- Bauen (inkl. Codeprüfung): `dotnet build AndyTours.slnx`
- Testen: `dotnet test AndyTours.slnx`
- Starten: `dotnet run --project src/AndyTours.App`
