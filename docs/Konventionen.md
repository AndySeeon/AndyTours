# AndyTours – Konventionen

Verbindliche Regeln für den Quellcode. Grundlage ist die Projektvorgabe in `CLAUDE.md`:
*„Jede Aufgabe bekommt eine eigene Datei: eigenständige Fachfunktionen einzeln, zusammengehörige Funktionen gemeinsam in einer Klasse. Alles wird ausführlich mit Kommentaren erläutert."*

## Projektstruktur

| Ordner / Projekt | Inhalt | darf verweisen auf |
|---|---|---|
| `src/AndyTours.Core` | Domänenmodell (`Modell/`) und Fachfunktionen (`Berechnungen/`) – ohne Oberfläche, ohne Datenbank | – |
| `src/AndyTours.Daten` | SQLite-Datenbank: Schema, Lesen und Schreiben | Core |
| `src/AndyTours.Import` | Einlesen von FIT/GPX/TCX und Übernahme der BikeXperience-Altdaten (`Altdaten/`) | Core |
| `src/AndyTours.App` | WPF-Oberfläche: `Views/` (XAML), `ViewModels/` und `Funktionen/` (Abläufe, die Daten und Import verbinden) | Core, Daten, Import |
| `tools/AndyTours.Altdatenuebernahme` | Konsolenprogramm für die einmalige Umwandlung der BikeXperience-Altdaten in einen AndyTours-Datenordner | Daten, Import |
| `tests/AndyTours.Tests` | Unit-Tests (xUnit) | Core, Daten, Import, Altdatenuebernahme |
| `Daten/` | Ihre Nutzdaten (Altdaten, FIT-Dateien, später die Datenbank) – **kein Quellcode** | – |
| `docs/` | Plan und Dokumentation | – |

## „Eine Aufgabe pro Datei"

- **Eigenständige Fachfunktionen** (Berechnungen, Import-Schritte, Prüfungen) werden als `static class` mit genau **einer** öffentlichen Methode `Ausfuehren(...)` geschrieben. Der Klassenname beschreibt die Funktion als Verb:
  `Berechnungen/BerechneDurchschnittsgeschwindigkeit.cs` → `BerechneDurchschnittsgeschwindigkeit.Ausfuehren(distanzKm, fahrzeit)`
- **Zusammengehörige Funktionen**, die dasselbe Format oder dieselbe Tabelle teilen, stehen gemeinsam in einer `static class`, deren Name das Thema als Substantiv nennt. Die Methoden heißen nach ihrer Tätigkeit:
  - Tabellen der Datenbank: `…Tabelle` mit `Laden`/`Speichern`, z. B. `TrainingseinheitenTabelle.Laden(verbindung)`
  - Einstellungen (Tabelle `Einstellung`): `…Einstellung` mit `Laden`/`Speichern`, z. B. `FensterpositionEinstellung.Speichern(verbindung, position)`
  - Hin- und Rückumwandlung eines Formats, z. B. `DatumSpalte.Lesen`/`DatumSpalte.AlsText`
- Faustregel: Ändert sich das eine, muss fast immer auch das andere geändert werden → eine Datei. Sonst → getrennte Dateien.
- **Modelle, ViewModels, Fenster** sind je eine Klasse pro Datei; sie enthalten nur Daten bzw. das Weiterreichen an Fachfunktionen, keine eigene Fachlogik.
- Dateiname = Klassenname.

## Kommentare

- Jede Klasse, Methode und öffentliche Eigenschaft bekommt einen XML-Doc-Kommentar (`/// <summary>`), bei Bedarf mit `<param>`, `<returns>` und `<remarks>`.
- Im Methodenrumpf erklären Inline-Kommentare das **Warum** und die einzelnen Rechenschritte, insbesondere Formeln, Einheiten und Sonderfälle.
- XAML-Dateien beginnen mit einem Kommentar, der den Aufbau des Fensters beschreibt.
- Wo eine Funktion dem Original BikeXperience nachgebildet ist, wird die Java-Klasse genannt (z. B. „entspricht `datamodel.PowerCalc`").

## Benennung

- Bezeichner auf **Deutsch**, ohne Umlaute im Code (`Aktivitaet`, `Hoehenmeter`, `Ausfuehren`); in Texten, Kommentaren und der Oberfläche mit korrekten Umlauten.
- Klassen, Methoden, Eigenschaften: `PascalCase`; lokale Variablen und Parameter: `camelCase`; private Felder: `_camelCase`.
- Einheiten im Namen, wenn sie nicht offensichtlich sind: `DistanzKm`, `GewichtKg`, `FahrzeitSekunden`.

## Technik

- .NET 10 (`net10.0`, App: `net10.0-windows`), C# `latest`, Nullable aktiviert – gemeinsam festgelegt in `Directory.Build.props`; die SDK-Version (10.0.4xx, keine Vorabversionen) legt `global.json` fest.
- Formatierung nach `.editorconfig` (4 Leerzeichen, dateibezogene Namespaces, geschweifte Klammern immer).
- Oberfläche nach MVVM mit `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`).
- Fachfunktionen in Core bekommen Unit-Tests in `tests/AndyTours.Tests`.
- Einstellungen, die zum Datenbestand gehören (z. B. Datensicherung, später persönliche Daten), liegen in der Tabelle `Einstellung` (Schlüssel/Wert) der Datenbank im Datenordner – nicht unter `%APPDATA%`. Sie wandern so mit dem Datenordner mit. Die Schlüssel stehen in `Einstellungsschluessel` und tragen den Bereich als Vorsilbe (`Sicherung.Intervall`). Unter `%APPDATA%\AndyTours` liegt nur die Merkdatei des Datenordners.
