# Technik

## Sprachen
- C# (Sprachversion `latest`, Nullable aktiviert)

## Frameworks & Bibliotheken
- .NET 10 (LTS); App `net10.0-windows`, übrige Projekte `net10.0`
- WPF mit MVVM über `CommunityToolkit.Mvvm`
- SQLite über `Microsoft.Data.Sqlite`
- `Garmin.FIT.Sdk` für FIT-Dateien
- Bibliotheken für Diagramme und Karte werden erst bei der jeweiligen Funktion festgelegt

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
- Jede Funktion in einer eigenen Datei; Fachfunktionen als `static class` mit einer öffentlichen Methode `Ausfuehren(...)`
- Ausführliche Kommentare auf Deutsch (XML-Doc)
- Bezeichner deutsch ohne Umlaute; Oberfläche und Kommentare mit Umlauten
- Fachfunktionen erhalten Unit-Tests

## Befehle
- Bauen (inkl. Codeprüfung): `dotnet build AndyTours.slnx`
- Testen: `dotnet test AndyTours.slnx`
- Starten: `dotnet run --project src/AndyTours.App`
