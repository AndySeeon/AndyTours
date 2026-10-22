# Struktur

- `src/AndyTours.Core/` — Datenmodell und Fachfunktionen (Berechnungen, Statistiken), ohne Oberfläche und Datenbank
- `src/AndyTours.Daten/` — SQLite-Datenbank: Schema, Lesen, Schreiben
- `src/AndyTours.Import/` — Einlesen von Garmin-FIT-Dateien
- `src/AndyTours.App/` — WPF-Oberfläche (`Views/`, `ViewModels/`, `Funktionen/`)
- `tools/AndyTours.Altdatenuebernahme/` — Kommandozeilenprogramm für die einmalige Umwandlung der BikeXperience-Daten
- `tests/AndyTours.Tests/` — Unit-Tests (xUnit)
- `Daten/` — BikeXperience-Altdaten, Quelle der einmaligen Umwandlung; nicht versioniert
- `AppDaten/` — Datenordner von AndyTours während der Entwicklung (Debug-Build); nicht versioniert
- `docs/` — Projektgrundlagen, Konventionen, Analyseplan
- `docs/adr/` — Architekturentscheidungen
- `specs/<NNN-funktion>/` — Spezifikation, Plan und Tasks je eigenständiger Funktion
- `specs/<bereich>/map.md` — Funktionsübersicht eines großen Bereichs
- `specs/<bereich>/<NNN-funktion>/` — Spezifikation, Plan und Tasks je Funktion dieses Bereichs
