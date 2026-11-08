# AndyTours

Windows-Desktop-App zur Verwaltung der eigenen Fahrradtouren.

AndyTours löst das Programm BikeXperience 6.0.4 ab und übernimmt dessen Daten.
Umgesetzt sind die Funktionen, die ich selbst tatsächlich nutze. Die App ist für
einen einzelnen Benutzer gedacht.

## Funktionen

- **Touren** in einer Tabelle mit Filter, Suche und Mehrfachauswahl, Bearbeiten einzelner Touren
- **Import vom Garmin Edge 1040** per USB (Mediengerät/MTP), FIT-Dateien über das Garmin FIT SDK
- **Höhenprofil und Streckenkarte**, Mehrtagestouren, Track bearbeiten, GPX-Export
- **Statistiken** über die Jahre: Jahresfortschritt, Saisonziele, Bestleistungen
- **Wartung der Räder**: Wartungen erfassen, Wartungsalarme, Haltbarkeit von Teilen, Einsatz der Equipments
- **Tourenplanung** auf der Karte mit BRouter oder openrouteservice, Overlays, Übertragen von Courses auf den Edge
- **Datensicherung** und Wiederherstellung
- **Einmalige Übernahme** der BikeXperience-Altdaten mit einem eigenen Kommandozeilenprogramm

## Technik

- C# mit .NET 10, WPF mit MVVM (`CommunityToolkit.Mvvm`) im Fluent-Design
- SQLite über `Microsoft.Data.Sqlite`
- `Garmin.FIT.Sdk`, `OxyPlot.Wpf` (Diagramme), `Mapsui.Wpf` (Karten), `MediaDevices` (MTP)
- Tests mit xUnit

Einzelheiten stehen in [`docs/tech.md`](docs/tech.md), der Aufbau des Projekts in
[`docs/structure.md`](docs/structure.md), die Architekturentscheidungen in [`docs/adr/`](docs/adr/).
Für jedes Feature gibt es unter [`specs/`](specs/) Spec, Plan und Aufgaben.

## Bauen und Starten

Voraussetzung: Windows und das .NET SDK 10.0.4xx (`winget install Microsoft.DotNet.SDK.10`).

```powershell
dotnet build AndyTours.slnx                 # Bauen
dotnet test AndyTours.slnx                  # Testen
dotnet run --project src/AndyTours.App      # Starten
```

Installationsprogramm bauen (benötigt Inno Setup 6):

```powershell
powershell tools/AndyTours.Installation/Erstelle-Installation.ps1
```

## Hinweise

Dies ist ein privates Hobbyprojekt. Es steht in keiner Verbindung zu BikeXperience oder
Garmin. Die genannten Marken gehören ihren jeweiligen Inhabern.

## Lizenz

[MIT](LICENSE) © Andreas Hensel
