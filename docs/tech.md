# Technik

## Sprachen
- C# `latest`, Nullable aktiviert, ImplicitUsings

## Frameworks & Bibliotheken
- .NET 10 (LTS): App `net10.0-windows10.0.19041.0`, alle übrigen Projekte `net10.0`
- WPF (nur Windows, kein Avalonia)
- Erscheinungsbild: eingebautes Fluent-Design von WPF, immer hell, siehe `docs/adr/0006-grundlage-der-oberflaeche.md`
- Kontrastfarben auf dem Fluent-Design (Karteikarten, Hauptmenü, weiße Felder und Listen), siehe `docs/adr/0008-kontrastfarben-der-oberflaeche.md`
- Skalierung der Oberfläche: zentrale Vergrößerung des Fensterinhalts (100–200 %), siehe `docs/adr/0007-skalierung-der-oberflaeche.md`
- SQLite über `Microsoft.Data.Sqlite` 10.0.13
- `CommunityToolkit.Mvvm` 8.4.2 (`[ObservableProperty]`, `[RelayCommand]`)
- `Garmin.FIT.Sdk` 21.218.0
- `OxyPlot.Wpf` 2.2.0 (Diagramme)
- `MediaDevices` 2.0.0, nur App (Zugriff auf den Edge per MTP)
- `Mapsui.Wpf` 5.1.0, nur App (Karten)
- Routing für die Tourenplanung: BRouter (voreingestellt) oder openrouteservice
- Tests: `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.4, `Microsoft.NET.Test.Sdk` 17.14.1, `coverlet.collector` 6.0.4

## Werkzeuge
- .NET SDK 10.0.4xx, festgelegt per `global.json` (`10.0.402`, `rollForward: latestFeature`, keine Vorabversionen) — `winget install Microsoft.DotNet.SDK.10`
- Git — `winget install Git.Git`
- Visual Studio 2026 mit „.NET-Desktopentwicklung“ (WPF-Designer, Debugging)
- Inno Setup 6 (Installations- und Deinstallationsprogramm) — `winget install JRSoftware.InnoSetup`
- VS Code (optional) — `winget install Microsoft.VisualStudioCode`
- Codeprüfung: eingebauter .NET-Analyzer, Meldungen als Warnungen beim Build

## Coding-Standards
- `Directory.Build.props` für alle Projekte: `LangVersion latest`, `Nullable enable`, `ImplicitUsings enable`, `GenerateDocumentationFile true`, `NoWarn CS1591`, `AnalysisLevel latest-recommended`, `EnforceCodeStyleInBuild true`, `Version` (einzige Stelle der Versionsnummer), `Product AndyTours`, `Authors Andreas Hensel`, `Company AndyTours`, `Copyright © Andreas Hensel` (Autor in der exe und in „Über AndyTours“), `NeutralLanguage de-DE`
- `.editorconfig`: `utf-8-bom`, `crlf`, abschließende Leerzeile, 4 Leerzeichen; `csproj/props/slnx/xml/xaml` mit 2 Leerzeichen; dateibezogene Namespaces; `var` nur bei offensichtlichem Typ; geschweifte Klammern immer; private Felder `_camelCase`; CA1707 in `tests/**.cs` aus; CA1822 in `src/AndyTours.App/ViewModels/**.cs` aus
- Eine Datei je Aufgabe; Dateiname = Klassenname
- Eigenständige Fachfunktionen: `static class` mit Verb als Namen und genau einer öffentlichen Methode `Ausfuehren(...)` (z. B. `BerechneAuswahlsummen`)
- Zusammengehöriges: `static class` mit Substantiv als Namen (`…Tabelle.Laden/Speichern`, `…Einstellung.Laden/Speichern`, `DatumSpalte.Lesen/AlsText`)
- Modelle, ViewModels, Fenster: je eine Klasse pro Datei, ohne eigene Fachlogik
- Bezeichner deutsch ohne Umlaute; Oberfläche und Kommentare mit Umlauten
- `PascalCase` für Klassen, Methoden, Eigenschaften; `camelCase` für lokale Variablen und Parameter; Einheiten im Namen (`DistanzKm`, `FahrzeitSekunden`)
- Ausführliche deutsche XML-Doc-Kommentare für jede Klasse, Methode und öffentliche Eigenschaft; Inline-Kommentare erklären das Warum, Formeln, Einheiten und Sonderfälle
- XAML-Dateien beginnen mit einem Kommentar zum Aufbau des Fensters
- Nachgebildete BikeXperience-Klassen werden genannt (z. B. „entspricht `datamodel.PowerCalc`“)
- Oberfläche nach MVVM mit `CommunityToolkit.Mvvm`
- Fenster nutzen Standardsteuerelemente ohne eigene Vorlagen; eigene Farben nur für fachliche Hervorhebungen
- Eigene Stile für Steuerelemente bauen auf dem Fluent-Stil auf (`BasedOn="{StaticResource {x:Type …}}"`), sonst fällt das Steuerelement auf das klassische Aussehen zurück; ausgenommen sind `Border` und `ContentPresenter`, für die es keinen Fluent-Stil gibt
- Eigene Vorlagen nur zwei, beide app-weit in `App.xaml`: der Zellstil für Tabellen in `App.xaml` (baut auf `DefaultDataGridCellStyle` auf, stellt den Inhalt senkrecht mittig und übernimmt die Hintergrundfarbe der Zelle für fachliche Farben); eigene Zellstile der Ansichten bauen mit `BasedOn="{StaticResource {x:Type DataGridCell}}"` darauf auf; und der Reiterstil für Karteikarten (baut auf dem Fluent-Reiter auf, eigene Vorlage, weil Fluent den gewählten Reiter fest weiß animiert)
- Ausnahme bei den Farben, app-weit in `App.xaml` (ADR 0008): Karteikarten haben eine hellgraue Seite, mit der der gewählte Reiter (deutlicher Rahmen, halbfette Beschriftung) eine Fläche bildet, nicht gewählte Reiter und die Hauptmenüpunkte sind dunkler grau, beim Darüberfahren bzw. geöffnet noch etwas dunkler (auch die Einträge aufklappender Menüs); Eingabe-, Auswahl- und Datumsfelder sowie Listen sind deckend weiß, Listen mit feinem Rahmen
- Jedes Fenster meldet sich nach `InitializeComponent` bei `Oberflaechenskalierung` an; Dialoge haben unten eine feste Schaltflächenleiste und darüber rollbaren Inhalt; Kartensteuerelemente werden mit `Oberflaechenskalierung.Ausnehmen` ausgenommen
- Fachfunktionen bekommen Unit-Tests
- Equipment-Namen immer über `EquipmentAnzeigename.Bilden` bilden (Zusatz „(ausgemustert)“) und über `EquipmentAnzeigename.Passt` vergleichen
- Einstellungen, die zum Datenbestand gehören, in der Tabelle `Einstellung` (Schlüssel/Wert mit Bereichsvorsilbe, z. B. `Sicherung.Intervall`); unter `%APPDATA%\AndyTours` nur die Merkdatei `datenordner.txt`
- Die Datenbank speichert nur relative Pfade zu den Aufzeichnungen

## Befehle
- Bauen (inkl. Codeprüfung): `dotnet build AndyTours.slnx`
- Testen: `dotnet test AndyTours.slnx`
- Linten: `dotnet build AndyTours.slnx` (Analyzer-Warnungen beim Build)
- Ausführen: `dotnet run --project src/AndyTours.App`
- Installationsprogramm bauen: `powershell tools/AndyTours.Installation/Erstelle-Installation.ps1` (Ergebnis: `tools/AndyTours.Installation/bin/Setup/AndyTours-Setup-<Version>.exe`)
- Programmicon neu erzeugen (nur nach Änderung von `Programmicon.xaml`): `powershell tools/AndyTours.Programmicon/Erstelle-Programmicon.ps1`
