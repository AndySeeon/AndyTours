# Struktur

- `AndyTours.slnx` — Projektmappe
- `src/AndyTours.Core/` — Datenmodell (`Modell/`) und Fachfunktionen (`Berechnungen/`), ohne Oberfläche und Datenbank
- `src/AndyTours.Daten/` — SQLite-Datenbank (Schema, Lesen, Schreiben, `Datenordner/`, `Datensicherung/`); verweist auf Core
- `src/AndyTours.Import/` — FIT-Dateien einlesen (`Garmin/`) und BikeXperience-Altdaten lesen (`Altdaten/`); verweist auf Core
- `src/AndyTours.App/` — WPF-Oberfläche mit `Views/`, `ViewModels/`, `Funktionen/` und `Ressourcen/`; verweist auf Core, Daten und Import
- `src/AndyTours.App/Ressourcen/` — Programmicon: Vektorzeichnung `Programmicon.xaml` (einzige Quelle des Motivs) und daraus erzeugte `AndyTours.ico`, auch für Umwandlungs- und Installationsprogramm
- `tools/AndyTours.Altdatenuebernahme/` — Kommandozeilenprogramm für die einmalige Umwandlung der BikeXperience-Daten; verweist auf Daten und Import
- `tools/AndyTours.Installation/` — Inno-Setup-Skript `AndyTours.iss` und Bauskript `Erstelle-Installation.ps1`
- `tools/AndyTours.Programmicon/` — Skript `Erstelle-Programmicon.ps1`, erzeugt `AndyTours.ico` aus `Programmicon.xaml`
- `tests/AndyTours.Tests/` — Unit-Tests (xUnit); verweist auf Core, Daten, Import und Altdatenuebernahme
- `Daten/` — BikeXperience-Altdaten, nicht versioniert, nur lesend
- `AppDaten/` — Datenordner der Entwicklung (Debug-Build), nicht versioniert
- `docs/` — Projektgrundlagen
- `docs/adr/` — Architekturentscheidungen (ADRs)
- `specs/<NNN-feature>/` — Spec, Plan und Aufgaben je eigenständigem Feature
- `specs/<area>/map.md` — Feature-Map eines großen Bereichs (z. B. `specs/tourenverwaltung/map.md`)
- `specs/<area>/<NNN-feature>/` — Spec, Plan und Aufgaben je Feature dieses Bereichs
