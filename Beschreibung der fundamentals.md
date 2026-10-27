# AndyTours – vollständige Projektbeschreibung

Stand: 27.10.2026 · Version 1.0.1 (Commit `0312424`) · 35 Funktionen umgesetzt, 1078 Tests grün, 0 Analyzer-Warnungen

Diese Datei fasst alles zusammen, was im Lauf des Projekts entschieden wurde. Sie enthält den ursprünglichen Auftrag,
alle Antworten auf Rückfragen, die Auswahl der Funktionen (umgesetzt und gestrichen), Technik, Struktur, Konventionen,
Architekturentscheidungen und zu jeder Funktion die abgestimmte Spezifikation mit allen Kriterien und den
Entscheidungen aus dem Plan. Mit ihr lässt sich das Projekt von vorn aufsetzen, ohne die Fragen erneut zu beantworten.

---

## 1. Ursprünglicher Auftrag

> Ein Tool zur Verwaltung meiner Fahrradtouren erstellen wie im Vorbild BikeXperience. Frage mich zu jeder Funktion,
> ob ich die tatsächlich nutzen und implementieren will. Jede Funktion soll ein eigener Task in der Map werden.

Ergänzt beim Aufruf von `/sdd-fundamentals`:

> Importiere alle vorhandenen Daten aus dem Ordner „Daten“ und konvertiere sie in das Format der neuen App. Name der
> App ist „AndyTours“. Die neue App selbst braucht keine Import-Funktion der Altdaten, nur den Import von meinem
> Garmin Edge 1040.

Der Arbeitsname war zuerst „RadlDB“ und wurde in „AndyTours“ geändert (Projekte, Namespaces, Projektmappe).

## 2. Antworten auf die Grundsatzfragen

| Frage | Antwort |
|---|---|
| Funktionsumfang | Zu jeder einzelnen BikeXperience-Funktion fragen, bevor sie in den Plan kommt (Ergebnis: Abschnitt 7) |
| Mehrbenutzer | Nein, ein Benutzer reicht |
| Plattform | Nur Windows (WPF, kein Avalonia) |
| Datenhaltung | SQLite |
| Parallelbetrieb mit BikeXperience | Nein, kein Rück-Export nach `workouts.csv` |
| Garmin-Gerät | Garmin Edge 1040 Solar. Er meldet sich per USB nur als Mediengerät (MTP), nicht als Laufwerk (siehe ADR 0002) |
| Altdaten | Kein Altdaten-Import in der App. Ein eigenes Kommandozeilenprogramm wandelt sie einmalig um |
| Datenordner | Frei wählbar, z. B. auf dem NAS. In der Entwicklung immer `AppDaten/` im Projektordner, sonst anfangs `Dokumente\AndyTours` |
| Codeprüfung | Eingebauter .NET-Analyzer, Meldungen als Warnungen beim Build |

## 3. Arbeitsweise mit Claude Code

### SDD-Ablauf
`/sdd-fundamentals` (einmalig) → `/sdd-map tourenverwaltung` → je Funktion:

1. `/sdd-spec tourenverwaltung: <funktion>`
2. `/sdd-plan`
3. (`/sdd-adr` bei offenen Entscheidungen)
4. `/sdd-tasks`
5. Umsetzung („Setze … tasks.md Aufgabe für Aufgabe um und hake jede erledigte Aufgabe ab“)
6. `/sdd-verify`
7. „Committe NNN Titel“

Ein Skill startet nie selbst den nächsten. Nach bestandenem `/sdd-verify` setzt der Skill den Eintrag in der Map auf `done`.
Wünsche, die der Benutzer bei der manuellen Prüfung äußert, werden sofort umgesetzt und als „Auf Wunsch des Benutzers
ergänzt“ bzw. „Nach … ergänzt“ unter `## Entscheidungen` im Plan festgehalten; wirken sie über die Funktion hinaus,
zusätzlich als ADR (z. B. 0008) und in `docs/tech.md`. Ändert sich ein Kriterium, wird auch `spec.md` angepasst.

### Regeln für die Antworten
- Immer Deutsch mit korrekten Umlauten.
- Auswahlmöglichkeiten in Rückfragen nummerieren (z. B. A1/A2, K1/K2, je Frage ein eigener Buchstabe) und die Empfehlung kennzeichnen.
- Jede Antwort endet mit der nächsten erwarteten Anweisung als kopierbarem Codeblock.

### Commits
- Nur auf ausdrückliche Bitte.
- Nachricht „NNN Titel“ (z. B. „017 Garmin-Import“), darunter `Co-Authored-By: Claude …`.
- `sdd-workflow-beispiel.*` nie mitcommitten (auch nicht das Löschen der Datei).
- „so starten wir von vorn.txt“ (persönliche Notiz des Benutzers für den Neustart) nie mitcommitten.
- Versionswechsel als eigener Commit „Version x.y.z“ (nur `Directory.Build.props`); das Installationsprogramm wird danach neu gebaut, aber nicht versioniert.
- Änderungen an dieser Beschreibung als eigener Commit, z. B. „Projektbeschreibung 1.0.1“.

### Daten und Sicherheit
- `Daten/` (BikeXperience-Altdaten) und `AppDaten/` (Entwicklungs-Datenordner) werden nie versioniert und nur lesend ausgewertet.
- Keine persönlichen FIT-Aufzeichnungen ins Repository; Testdaten erzeugen die Tests selbst.
- Dropbox-Schlüssel und WebAccess-Zugangsdaten aus `bikexperience.ini` werden nicht übernommen. Geheimwerte werden nie im Klartext ausgegeben.
- Der openrouteservice-Zugangsschlüssel wird nie angezeigt (nur maskiert) und nie protokolliert.
- Eine laufende App des Benutzers wird nicht beendet. Es wird keine zweite Instanz auf dieselbe Datenbank gestartet.
  Läuft sie beim Bauen, wird in einen Ausgabeordner im Arbeitsbereich von Claude gebaut oder der Benutzer gebeten, sie zu schließen.
- Eine selbst gestartete Instanz (für Bildschirmfotos) wird vor dem Bauen wieder geschlossen; offene Dialoge (z. B. Einstellungen)
  zuerst, sonst beendet sich die App nicht und der Build scheitert an gesperrten Dateien.
- Installationstests laufen nur mit einem Test-Datenordner, möglichst in der Windows-Sandbox. Die Merkdatei `%APPDATA%\AndyTours\datenordner.txt` wird dabei nicht unbedacht geändert.
- `bikeXperience.exe` (fremde Software) liegt nicht im Repository.

## 4. Produkt

- AndyTours: Windows-Desktop-App zur Verwaltung der eigenen Fahrradtouren.
- Vorbild ist BikeXperience 6.0.4 (Java/Swing, Launch4j). Umgesetzt werden nur die Funktionen, die der Benutzer tatsächlich nutzt.
- Neue Touren kommen per Import vom Garmin Edge 1040.
- Zielgruppe: nur der Benutzer selbst (Einzelbenutzer).
- Ziele:
  - BikeXperience ablösen und alle Daten aus `Daten/` übernehmen.
  - Statistiken über die Jahre wie in der Ansicht „Aktuell“.
  - Wartung der Räder wie in BikeXperience.

## 5. Technik

### Werkzeuge der Entwicklungsumgebung

| Werkzeug | Zweck | Installation |
|---|---|---|
| .NET SDK 10.0.4xx | Bauen, Testen; per `global.json` festgelegt (`10.0.402`, `rollForward: latestFeature`, keine Vorabversionen) | `winget install Microsoft.DotNet.SDK.10` |
| Git | Versionsverwaltung | `winget install Git.Git` |
| Visual Studio 2026 („.NET-Desktopentwicklung“) | WPF-Designer, Debugging | Visual Studio Installer |
| Inno Setup 6 | Installations- und Deinstallationsprogramm (ADR 0005) | `winget install JRSoftware.InnoSetup` |
| VS Code (optional) | Code und Doku bearbeiten | `winget install Microsoft.VisualStudioCode` |

### Plattform und NuGet-Pakete
- C# `latest`, Nullable aktiviert, ImplicitUsings.
- .NET 10 (LTS): die App zielt auf `net10.0-windows10.0.19041.0`, alle übrigen Projekte auf `net10.0`.
- NuGet-Pakete:
  - `CommunityToolkit.Mvvm` 8.4.2 (MVVM mit `[ObservableProperty]`, `[RelayCommand]`)
  - `Microsoft.Data.Sqlite` 10.0.13
  - `Garmin.FIT.Sdk` 21.218.0
  - `OxyPlot.Wpf` 2.2.0 (ADR 0001)
  - `MediaDevices` 2.0.0, nur App (ADR 0002)
  - `Mapsui.Wpf` 5.1.0, nur App (ADR 0003)
  - Tests: `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.4, `Microsoft.NET.Test.Sdk` 17.14.1, `coverlet.collector` 6.0.4
- Routing (Tourenplanung): BRouter (voreingestellt) oder openrouteservice (ADR 0004).
- Erscheinungsbild: eingebautes Fluent-Design von WPF, immer hell (`ThemeMode="Light"` in `App.xaml`, ADR 0006). Die Meldung
  `WPF0001` (experimentelle Schnittstelle) ist im App-Projekt per `NoWarn` abgeschaltet.
- Kontrastfarben auf dem Fluent-Design (Karteikarten, Hauptmenü, weiße Felder und Listen), app-weit in `App.xaml` (ADR 0008).
- Skalierung der Oberfläche: zentrale Vergrößerung des Fensterinhalts in Stufen 100–200 % (ADR 0007).

### `Directory.Build.props` (gilt für alle Projekte)
- `LangVersion latest`, `Nullable enable`, `ImplicitUsings enable`
- `GenerateDocumentationFile true`, `NoWarn CS1591`
- `AnalysisLevel latest-recommended`, `EnforceCodeStyleInBuild true`
- `Version 1.0.1`: die einzige Stelle der Versionsnummer, gültig für App, Umwandlungsprogramm, Setup und „Über AndyTours“
  (Versionen: 1.0.0 mit 033, 1.0.1 nach 034/035)
- `Product AndyTours`, `Authors Andreas Hensel`, `Company AndyTours`, `Copyright © Andreas Hensel` (Autor in der exe und in „Über AndyTours“), `NeutralLanguage de-DE`

### `.editorconfig`
- Grundeinstellungen: `utf-8-bom`, `crlf`, abschließende Leerzeile, 4 Leerzeichen.
- `csproj/props/slnx/xml/xaml` mit 2 Leerzeichen.
- Dateibezogene Namespaces; `var` nur bei offensichtlichem Typ; geschweifte Klammern immer.
- Private Felder `_camelCase`.
- In `tests/**.cs` ist CA1707 aus, damit Testnamen mit Unterstrichen wie Sätze lesbar sind.
- In `src/AndyTours.App/ViewModels/**.cs` ist CA1822 aus.

### Befehle (im Projektordner)
- Bauen inkl. Codeprüfung: `dotnet build AndyTours.slnx`
- Testen: `dotnet test AndyTours.slnx`
- Starten: `dotnet run --project src/AndyTours.App`
- Installationsprogramm bauen: `powershell tools/AndyTours.Installation/Erstelle-Installation.ps1`
  - Ergebnis: `tools/AndyTours.Installation/bin/Setup/AndyTours-Setup-<Version>.exe`
  - Ablauf des Skripts:
    1. Version aus `Directory.Build.props` lesen und `bin/` leeren.
    2. App self-contained für win-x64 nach `bin\App` veröffentlichen.
    3. Umwandlungsprogramm self-contained als einzelne Datei nach `bin\Werkzeug` veröffentlichen.
    4. `ISCC.exe` mit `/DAppVersion /DAppQuelle /DWerkzeugQuelle /O bin\Setup` auf `AndyTours.iss` aufrufen.
  - Aufruf immer so, ohne `-ExecutionPolicy Bypass` (vom Benutzer nicht freigegeben).
- Programmicon neu erzeugen (nur nach Änderung von `Programmicon.xaml`): `powershell tools/AndyTours.Programmicon/Erstelle-Programmicon.ps1`
- Hinweis: Nach Änderungen an `App.xaml` (Stile, Ressourcen) ggf. `dotnet build AndyTours.slnx --no-incremental`, damit sicher neu übersetzt wird.

## 6. Struktur und Konventionen

### Ordner
- `src/AndyTours.Core/`: Datenmodell (`Modell/`) und Fachfunktionen (`Berechnungen/`), ohne Oberfläche und Datenbank.
- `src/AndyTours.Daten/`: SQLite-Datenbank (Schema, Lesen, Schreiben, `Datenordner/`, `Datensicherung/`). Verweist auf Core.
- `src/AndyTours.Import/`: FIT-Dateien einlesen (`Garmin/`) und BikeXperience-Altdaten lesen (`Altdaten/`). Verweist auf Core.
- `src/AndyTours.App/`: WPF-Oberfläche mit `Views/`, `ViewModels/`, `Funktionen/` und `Ressourcen/`. Verweist auf Core, Daten und Import.
- `src/AndyTours.App/Ressourcen/`: Programmicon als Vektorzeichnung `Programmicon.xaml` (einzige Quelle des Motivs, große Fassung
  `ProgrammiconGross` und kleine `ProgrammiconKlein` für 16–24 px) und die daraus erzeugte `AndyTours.ico` (im Repository), auch für
  Umwandlungs- und Installationsprogramm.
- `tools/AndyTours.Altdatenuebernahme/`: Kommandozeilenprogramm für die einmalige Umwandlung der BikeXperience-Daten.
- `tools/AndyTours.Installation/`: Inno-Setup-Skript `AndyTours.iss` und Bauskript `Erstelle-Installation.ps1`.
- `tools/AndyTours.Programmicon/`: Skript `Erstelle-Programmicon.ps1`, erzeugt `AndyTours.ico` aus `Programmicon.xaml`.
- `tests/AndyTours.Tests/`: Unit-Tests (xUnit).
- `Daten/`: BikeXperience-Altdaten, nicht versioniert.
- `AppDaten/`: Datenordner der Entwicklung (Debug-Build), nicht versioniert.
- `docs/`: Grundlagen, Konventionen, Analyseplan.
- `docs/adr/`: Architekturentscheidungen.
- `specs/tourenverwaltung/map.md`: Funktionsübersicht.
- `specs/tourenverwaltung/<NNN-funktion>/`: `spec.md`, `plan.md`, `tasks.md` je Funktion.
- Projektmappe: `AndyTours.slnx`.

### Datenordner
- Inhalt:
  - Datenbank `andytours.db`
  - Aufzeichnungen in Jahresordnern (`2016/` …)
  - Ordner `Sicherungen`
  - Berichte der Umwandlung
- Die Datenbank speichert nur relative Pfade zu den Aufzeichnungen.
- Einstellungen, die zum Datenbestand gehören, liegen in der Tabelle `Einstellung` (Schlüssel/Wert mit Bereichsvorsilbe, z. B. `Sicherung.Intervall`).
- Unter `%APPDATA%\AndyTours` liegt nur die Merkdatei `datenordner.txt`.
- Feste Schlüssel (`Einstellungsschluessel`): `Sicherung.Ordner`, `Sicherung.Intervall`, `Sicherung.Hoechstzahl`,
  `Sicherung.StartsSeitSicherung`, `Einheitentabelle.AusgeblendeteSpalten`, `Einheitentabelle.Spalten`, `Hauptfenster.Position`,
  `Hauptfenster.Karteikarte`, `Aktivitaeten.Reihenfolge`, `GpxExport.Ordner`, `Tourenplanung.Profil`, `Tourenplanung.Ausschnitt`,
  `Tourenplanung.Ordner`, `Routing.Dienst`, `Routing.BRouterAdresse`, `Routing.OpenrouteserviceSchluessel`, `Overlay.Farbe`,
  `Darstellung.Groesse` (Stufe der Oberfläche als Prozentzahl 100/110/125/150/175/200, jeder andere Wert ergibt 100); dazu die Auswahllisten.

### Regeln für den Code
Ausführlich in `docs/Konventionen.md`, wörtlich in Abschnitt 11 dieser Datei.

- **Eine Datei je Aufgabe.**
  - Eigenständige Fachfunktionen sind `static class` mit einer öffentlichen Methode `Ausfuehren(...)`. Der Klassenname ist ein Verb, z. B. `BerechneAuswahlsummen`.
  - Zusammengehöriges steht gemeinsam in einer `static class` mit einem Substantiv als Namen: `…Tabelle.Laden/Speichern`, `…Einstellung.Laden/Speichern`, `DatumSpalte.Lesen/AlsText`.
- Ausführliche deutsche XML-Doc-Kommentare. XAML-Dateien beginnen mit einem Kommentar zum Aufbau. Nachgebildete BikeXperience-Klassen werden genannt.
- Bezeichner sind deutsch ohne Umlaute; Oberfläche und Kommentare stehen mit Umlauten.
- Fachfunktionen bekommen Unit-Tests.
- Equipment-Namen werden immer über `EquipmentAnzeigename.Bilden` gebildet (Zusatz „(ausgemustert)“) und über `EquipmentAnzeigename.Passt` verglichen.
- Modelle, ViewModels und Fenster: je eine Klasse pro Datei, ohne eigene Fachlogik.
- Fenster nutzen Standardsteuerelemente ohne eigene Vorlagen; eigene Farben nur für fachliche Hervorhebungen (ADR 0006).
- Eigene Stile für Steuerelemente bauen auf dem Fluent-Stil auf (`BasedOn="{StaticResource {x:Type …}}"`), sonst fällt das
  Steuerelement auf das klassische Aussehen zurück; ausgenommen sind `Border` und `ContentPresenter` (kein Fluent-Stil).
- Eigene Vorlagen gibt es nur zwei, beide app-weit in `App.xaml`:
  - der Zellstil für Tabellen (baut auf `DefaultDataGridCellStyle` auf, stellt den Inhalt senkrecht mittig und übernimmt die
    Hintergrundfarbe der Zelle für fachliche Farben); eigene Zellstile der Ansichten bauen mit
    `BasedOn="{StaticResource {x:Type DataGridCell}}"` darauf auf;
  - der Reiterstil für Karteikarten (baut auf dem Fluent-Reiter auf; eigene Vorlage, weil Fluent den gewählten Reiter fest weiß animiert).
- Ausnahme bei den Farben, app-weit in `App.xaml` (ADR 0008): Karteikarten haben eine hellgraue Seite, mit der der gewählte
  Reiter (deutlicher Rahmen, halbfette Beschriftung) eine Fläche bildet; nicht gewählte Reiter und die Hauptmenüpunkte sind dunkler
  grau, beim Darüberfahren bzw. geöffnet noch etwas dunkler (auch die Einträge aufklappender Menüs); Eingabe-, Auswahl- und
  Datumsfelder sowie Listen sind deckend weiß, Listen mit feinem Rahmen. `Label` haben app-weit den Innenabstand `5,5,12,5`.
- Jedes Fenster meldet sich nach `InitializeComponent` bei `Oberflaechenskalierung` an; Dialoge haben unten eine feste
  Schaltflächenleiste und darüber rollbaren Inhalt (`DockPanel`, Leiste unten angedockt, `ScrollViewer` mit dem Inhalt);
  Kartensteuerelemente werden mit `Oberflaechenskalierung.Ausnehmen` ausgenommen (ADR 0007).
- Technische Stolpersteine des Fluent-Designs (gelernt bei 034/035):
  - App-Ressourcen mit den Fluent-Schlüsseln (z. B. `TextControlBackground`, `ComboBoxBackground`, `ListBoxBackground`,
    `MenuBarItemBackgroundSelected`) überschreiben Fluent-Farben, die per `DynamicResource` verwendet werden, nicht aber
    Zustandsanimationen (VisualStateManager) – deshalb die eigene Reitervorlage.
  - Ein `<StaticResource>`-Element in `Style.Resources` führt beim Start zu einer `XamlParseException`; stattdessen
    `Color`-Ressourcen mit eigenem `SolidColorBrush` verwenden.
  - `FontWeight` am `TabItem` vererbt sich auf den ganzen Inhalt der Karteikarte; die halbfette Beschriftung kommt deshalb über
    eine `HeaderTemplate` mit `DataTrigger` auf `TabItem.IsSelected`.

## 7. Funktionsauswahl: was umgesetzt und was gestrichen wurde

Grundlage war die Liste `specs/tourenverwaltung/funktionsauswahl.md`. Der Benutzer hat darin Nicht-Benötigtes
gelöscht. Spätere Änderungen sind unten vermerkt.

### Von Anfang an bestätigt
- Datenordner wählen
- Einmalige Altdaten-Umwandlung (separates Programm)
- Import vom Garmin Edge 1040
- „Aktuell“: Jahresfortschritt, Bestleistungen mit Rekordjahr, Mehrjahresdiagramm
- Wartung: Einträge erfassen, Alarme nach n km, Auskunft „wie lange hielt das Teil“

### Aus der Auswahlliste behalten und umgesetzt
1. Hauptfenster mit Jahres- und Aktivitätsauswahl → 003
2. Automatische Sicherung → 004
3. Persönliche Daten → 013
4. Saisonziele → 014
5. Aktivitäten und Kategorien verwalten → 010
6. Equipments verwalten inkl. „ausgemustert“ → 011
8. Tabelle mit Sortierung, Summenzeile, roten Bestwerten → 006
9. Filter → 007
10. Einheit neu, bearbeiten, kopieren, löschen → 008
11. Mehrfachauswahl → 012
13. Höhenprofil → 018
14. Strecke auf OSM-Karte → 019
20. Mehrtagestour → 020
26. Suche → 026
27. Einsatz je Equipment → 027
40. Track als GPX exportieren → 028
41. Courses auf den Edge 1040 übertragen → 032
42. Track bearbeiten (kürzen, umkehren, glätten) → 029
43. Tourenplanung per Mausklick → 030
44. Overlays → 031

### Beim Auswählen gestrichen
- 7. Schwellwerte mit Farben (Puls, Geschwindigkeit, Steigung, Leistung)
- 15. Puls- und Trainingszonen-Auswertung (Kuchendiagramme)
- 17. Rundenzeiten
- 18. Höhenprofile mehrerer Touren vergleichen
- 21. Karteikarte „Diagramme“
- 22. Karteikarte „Statistik“
- 23. Kalender
- 24. Jahresprognose
- 25. Top 50
- 28. Gewichtsverlauf
- 29. Ruhe- und Erholungspuls-Verlauf
- 30. Leistungs- und Trainingskontrolle
- 31. Ø-Geschwindigkeits- und Trittfrequenz-Diagramm
- 33. Temperatur-Auswertung
- 34. Virtuelle Weltumrundung
- 36. Ritzelrechner
- 37. Pulsrechner
- 38. Marathonzeitenplaner
- 46. Trainingsplan
- 47. Karteikarte „Planung“

### Zunächst behalten, später gestrichen
- 12. Anhänge einer Einheit öffnen
- 16. Leistungsberechnung (Steig- und Gesamtleistung, Kalorien). Gestrichen mit 009; damit entfallen auch die Equipment-Felder Gewicht, Radumfang, Roll- und Luftwiderstandswerte, Kettenblätter, Ritzel und Bild.
- 19. Werte aus der Aufzeichnung neu übernehmen; der Garmin-Import übernimmt sie bereits beim Anlegen.
- 32. Vergleich Aktivitäten / Kategorien
- 35. CSV-Import und -Export der Wartungen, ebenso das Wartungs-Kuchendiagramm
- 39. GPX/TCX-Import
- 45. Bildergalerie mit Geotagging, nach 032 aus der Map entfernt
- Die leeren Karteikarten „Diagramme“, „Statistik“ und „Kalender“ wurden nach 028 aus dem Hauptfenster entfernt.

### Ebenfalls ausgeschlossen
- Mehrbenutzer-Verwaltung
- Altdaten-Import in der App
- Synchronisierung mit Garmin Connect (kein offizieller Zugang für Einzelnutzer)
- Dropbox-Synchronisation
- Serielle CicloSport-Geräte
- Exotische Dateiformate
- Look & Feel
- Proxy

### Während des Projekts neu hinzugekommen (nicht in der Auswahlliste)
- `sicherung-wiederherstellen` (005)
- `auswahllisten-verwalten` (009): Trainingsbereiche als gepflegte Liste
- Aktivitäten, Kategorien und Equipments werden unter „Auswahllisten“ in den Einstellungen verwaltet (010, 011)
- `teile-haltbarkeit` (023), erweitert: die Wartungstabelle zeigt die km seit der letzten gleichartigen Wartung desselben Rads
- `wartung-equipment-auswahl` (024)
- `ausgemustert-kennzeichnen` (025)
- `installationsprogramm` (033) inkl. Übernahme vorhandener Daten und sauberer Deinstallation
- Mehrjahresdiagramm (`mehrjahresdiagramm`) ist in 015 Jahresfortschritt aufgegangen
- FIT-Dateien aus einem Ordner importieren (Menü „Datei → FIT-Dateien importieren…“) ergänzend zum Garmin-Import
- Kein eigener Map-Eintrag: Hauptfenster merkt sich Größe, Position und Karteikarte; Spaltenauswahl und -anordnung bleiben erhalten (006)
- `erscheinungsbild-und-icon` (034): modernes, helles Windows-11-Erscheinungsbild (Fluent) und Programmicon, „Über AndyTours“ als eigenes Fenster
- `oberflaeche-skalieren` (035): Größe der Oberfläche 100–200 %; dabei die Einstellungen in Karteikarten gegliedert und Kontrastfarben (ADR 0008) ergänzt
- Nach 034: Die Auswahl „Tätigkeit“ im Wartungs- und Alarmfenster ist nach der bisherigen Häufigkeit geordnet (021, 022)
- Stand nach 035: Die Map enthält 36 Einträge, alle `done`; offene Funktionen gibt es nicht.

## 8. Bedienoberfläche (Stand 1.0.1)

### Menüs
- **Datei:** Vom Garmin importieren… · FIT-Dateien importieren… · Beenden
- **Einheit:** Neue Einheit… (Strg+N) · Bearbeiten… · Kopieren… · Tourdetail… · Kategorien zuordnen… · Aktivität ändern… · Equipment ändern… · Löschen (Entf)
- **Tourenplanung:** Tour planen… · Overlays…
- **Extras:** Persönliche Daten… · Saisonziele… · Equipment-Einsatz… · Einstellungen… · Sicherung wiederherstellen…
- **Hilfe:** Über AndyTours (eigenes Fenster mit großem Icon, Name, Version und Autor; schließt mit OK, Enter oder Esc)

### Fenster
- **Karteikarten des Hauptfensters:** Aktuell · Trainingseinheiten · Wartung (Titel zeigt Alarme an)
- **Einstellungen** (seit 035 in Karteikarten, Fenster 640 × 600, jede Karteikarte rollt für sich, „Schließen“ unten fest):
  - Allgemein: Datenordner (mit Hinweis in der Entwicklungsversion) · Darstellung („Größe der Oberfläche“)
  - Datensicherung
  - Tourenplanung: Routing · Overlays
  - Auswahllisten: Trainingsbereich, Aktivität, Kategorie, Equipment, Tätigkeit (Liste fest 300 hoch)
- **Tourdetail:** Höhenprofil · Karte
- **Dialoge:** unten feste Schaltflächenleiste, darüber rollbarer Inhalt; höchstens so groß wie der Arbeitsbereich des Bildschirms.

### Erscheinungsbild
- Fluent-Design von Windows 11, immer hell, Akzentfarbe von Windows; Meldungsfenster bleiben Windows-Standard.
- Karteikarten: hellgraue Seite `#F0F2F5` mit Rahmen `#8F98A3`; gewählter Reiter in Seitenfarbe mit gleichem Rahmen und halbfetter
  Beschriftung; nicht gewählte Reiter `#DCE1E7`, beim Darüberfahren `#CDD4DC`.
- Hauptmenüpunkte wie nicht gewählte Reiter; hervorgehobene Menüpunkte (auch aufklappende Menüs, Kontextmenüs) `#CDD4DC`.
- Eingabe-, Auswahl- und Datumsfelder deckend weiß, Listen weiß mit feinem Rahmen.
- Programmicon: stilisiertes Fahrrad mit orangem Rahmen und dunklen Rädern vor grünen Bergen mit weißen Gipfeln und blauem Himmel
  auf abgerundetem Quadrat.
- Größe der Oberfläche in den Einstellungen wählbar (100/110/125/150/175/200 %), sofort wirksam; Karten bleiben in Originalgröße.

## 9. Altdaten (BikeXperience 6.0.4)

### Quellen in `Daten/`

| Datei | Inhalt | Umfang / Format |
|---|---|---|
| `workouts.csv` | Hauptdatenbank, 932 Einheiten 2016–2026 | Erste Zeile = Anzahl; `;`-getrennt, Texte in `'…'`, Dezimalpunkt. 27 Felder: date, name, tm, dst, hm, maxhm, maxSpeed, heartrate, equipment, trainevent, starthm, avcc, temp, maxheartrate, colKey, starttime, activity, category (mit `,` getrennt), weight, minheartrate, avPower, recoveryPulse, comment, changeid, profile, gpstrack, text |
| `2016/` … `2026/` | Aufzeichnungen | 930 `.fit`-Dateien |
| `bikexperience.ini` | Einstellungen | Activity (Name=Haupt;Einheit;Ziel km;Ziel hm;Ziel Anzahl), Category, Biker, Limits…, `Equipment.<Name>`, GPSViewer, WebAccess (nicht übernehmen) |
| `maintanence.dat` | Wartungen und Alarme | `Datum;Equipment;Tätigkeit;Kommentar;Distanz;aktiv;Typ` (Typ 0 = Wartung, 1 = Alarm; Equipment `-` = radunabhängig) |
| `planning.dat`, `train.ini` | Planung, Trainingsplan | leer |

### Ergebnis der Umwandlung
- 932 Einheiten, 86 Wartungen, 20 Equipments: 18 aus der INI plus „GENERIC“ und „MOUNTAIN“ aus `workouts.csv`.
- 5 Kategorien plus „Training“, 28 Schwellwerte.
- Die Jahressummen sind identisch mit `workouts.csv` (automatischer Test).

### Beschädigte Umlaute
- 19 Umlaute lagen als Latin-1-Bytes vor und wurden exakt wiederhergestellt.
- 121 Texte wurden über eine Korrekturliste repariert, ergänzt um „Bäckeralm“ und „(ÜPD)“.

### Fehlende Aufzeichnungen
Zwei FIT-Dateien fehlen in den Altdaten: Zeile 344 (2019-11-11) und Zeile 933 (2026-10-05).

## 10. Datenbankschema (Schemaversion 4, `PRAGMA user_version`)

Wörtlich aus `src/AndyTours.Daten/ErstelleSchema.cs`:

```sql
-- Sportler (Nutzerverwaltung)
CREATE TABLE IF NOT EXISTS Fahrer (
    Id                  INTEGER PRIMARY KEY,
    Vorname             TEXT    NOT NULL,
    Nachname            TEXT    NOT NULL,
    Geburtstag          TEXT,
    GroesseCm           INTEGER NOT NULL,
    GewichtKg           REAL    NOT NULL,
    Ruhepuls            INTEGER NOT NULL,
    Maxpuls             INTEGER NOT NULL,
    TrainingspulsVon    INTEGER NOT NULL,
    TrainingspulsBis    INTEGER NOT NULL,
    Heimatort           TEXT    NOT NULL,
    HeimatBreitengrad   REAL,
    HeimatLaengengrad   REAL
);

CREATE TABLE IF NOT EXISTS Aktivitaet (
    Id                       INTEGER PRIMARY KEY,
    Name                     TEXT    NOT NULL UNIQUE COLLATE NOCASE,
    IstHaupt                 INTEGER NOT NULL,
    Geschwindigkeitseinheit  TEXT    NOT NULL,
    ZielKm                   REAL    NOT NULL,
    ZielHm                   REAL    NOT NULL,
    ZielAnzahl               INTEGER NOT NULL
);

-- Eigene Saisonziele je Aktivität, gültig ab einem Jahr (seit Schemaversion 3); das Grundziel aus
-- BikeXperience steht weiter in Aktivitaet. Wird eine Aktivität gelöscht, verschwinden ihre Ziele mit.
CREATE TABLE IF NOT EXISTS Saisonziel (
    AktivitaetId  INTEGER NOT NULL REFERENCES Aktivitaet(Id) ON DELETE CASCADE,
    AbJahr        INTEGER NOT NULL,
    ZielKm        INTEGER NOT NULL,
    ZielHm        INTEGER NOT NULL,
    ZielAnzahl    INTEGER NOT NULL,
    PRIMARY KEY (AktivitaetId, AbJahr)
);

CREATE TABLE IF NOT EXISTS Kategorie (
    Id           INTEGER PRIMARY KEY,
    Name         TEXT    NOT NULL UNIQUE COLLATE NOCASE,
    IstStandard  INTEGER NOT NULL
);

-- Ausrüstung; Kettenblätter und Ritzel als kommagetrennte Zähnezahlen („39,53")
CREATE TABLE IF NOT EXISTS Equipment (
    Id                 INTEGER PRIMARY KEY,
    Name               TEXT    NOT NULL UNIQUE COLLATE NOCASE,
    AktivitaetId       INTEGER REFERENCES Aktivitaet(Id),
    GewichtKg          REAL    NOT NULL,
    RadumfangMm        INTEGER NOT NULL,
    CrCoeff            REAL    NOT NULL,
    CmCoeff            REAL    NOT NULL,
    CwCoeff            REAL    NOT NULL,
    CwaCoeff           REAL    NOT NULL,
    SinCoeff           REAL    NOT NULL,
    Kettenblaetter     TEXT    NOT NULL,
    Ritzel             TEXT    NOT NULL,
    Geraetezuordnung   TEXT    NOT NULL,
    StandardKategorie  TEXT    NOT NULL,
    IstStandard        INTEGER NOT NULL,
    Ausgemustert       INTEGER NOT NULL,
    Anschaffungsdatum  TEXT,
    Bildpfad           TEXT    NOT NULL,
    AnfangsdistanzKm   REAL    NOT NULL
);

CREATE TABLE IF NOT EXISTS Trainingseinheit (
    Id                  INTEGER PRIMARY KEY,
    FahrerId            INTEGER NOT NULL REFERENCES Fahrer(Id),
    Datum               TEXT    NOT NULL,
    Startzeit           TEXT,
    Name                TEXT    NOT NULL,
    FahrzeitSekunden    INTEGER NOT NULL,
    DistanzKm           REAL    NOT NULL,
    Hoehenmeter         INTEGER NOT NULL,
    MaxHoehe            INTEGER NOT NULL,
    StartHoehe          INTEGER NOT NULL,
    MaxGeschwindigkeit  REAL    NOT NULL,
    HfDurchschnitt      INTEGER NOT NULL,
    HfMax               INTEGER NOT NULL,
    HfMin               INTEGER NOT NULL,
    Trittfrequenz       INTEGER NOT NULL,
    Temperatur          INTEGER NOT NULL,
    LeistungWatt        REAL    NOT NULL,
    KoerpergewichtKg    REAL    NOT NULL,
    Erholungspuls       INTEGER NOT NULL,
    Trainingsbereich    TEXT    NOT NULL,
    Kommentar           TEXT    NOT NULL,
    AktivitaetId        INTEGER NOT NULL REFERENCES Aktivitaet(Id),
    EquipmentId         INTEGER REFERENCES Equipment(Id)
);

-- Die Tabelle wird fast immer nach Datum gefiltert und sortiert
CREATE INDEX IF NOT EXISTS IX_Trainingseinheit_Datum ON Trainingseinheit(Datum);

-- n:m-Zuordnung Einheit ↔ Kategorie; wird eine Einheit gelöscht, verschwinden ihre Zuordnungen mit
CREATE TABLE IF NOT EXISTS EinheitKategorie (
    EinheitId    INTEGER NOT NULL REFERENCES Trainingseinheit(Id) ON DELETE CASCADE,
    KategorieId  INTEGER NOT NULL REFERENCES Kategorie(Id),
    PRIMARY KEY (EinheitId, KategorieId)
);

CREATE TABLE IF NOT EXISTS Anhang (
    Id             INTEGER PRIMARY KEY,
    EinheitId      INTEGER NOT NULL REFERENCES Trainingseinheit(Id) ON DELETE CASCADE,
    Typ            TEXT    NOT NULL,
    RelativerPfad  TEXT    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_Anhang_EinheitId ON Anhang(EinheitId);

CREATE TABLE IF NOT EXISTS Wartung (
    Id           INTEGER PRIMARY KEY,
    Datum        TEXT    NOT NULL,
    EquipmentId  INTEGER REFERENCES Equipment(Id),
    Taetigkeit   TEXT    NOT NULL,
    Kommentar    TEXT    NOT NULL,
    Typ          TEXT    NOT NULL,
    AlarmKm      REAL    NOT NULL,
    Aktiv        INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Schwellwert (
    Id           INTEGER PRIMARY KEY,
    Art          TEXT    NOT NULL,
    Reihenfolge  INTEGER NOT NULL,
    Name         TEXT    NOT NULL,
    Von          REAL    NOT NULL,
    Bis          REAL    NOT NULL,
    Farbe        TEXT    NOT NULL
);

-- Einstellungen, die zum Datenbestand gehören (z. B. Datensicherung), als Schlüssel/Wert-Paare;
-- seit Schemaversion 2. Ältere Datenbanken erhalten die Tabelle beim nächsten Öffnen automatisch.
CREATE TABLE IF NOT EXISTS Einstellung (
    Schluessel  TEXT    PRIMARY KEY,
    Wert        TEXT    NOT NULL
);

-- Ordner der Overlay-Verwaltung (seit Schemaversion 4); ElternId NULL = oberste Ebene. Wird ein Ordner
-- gelöscht, verschwinden seine Unterordner mit (und über die Tabelle Overlay auch deren Overlays).
CREATE TABLE IF NOT EXISTS Overlayordner (
    Id        INTEGER PRIMARY KEY,
    ElternId  INTEGER REFERENCES Overlayordner(Id) ON DELETE CASCADE,
    Name      TEXT    NOT NULL
);

-- Overlays: gespeicherte Strecken, die zusätzlich auf den Karten eingeblendet werden (seit Schemaversion 4).
-- OrdnerId NULL = oberste Ebene; Farbe NULL = gemeinsame Overlay-Farbe, sonst der Name der Farbe;
-- Punkte als Binärwert (siehe OverlayPunkte).
CREATE TABLE IF NOT EXISTS Overlay (
    Id             INTEGER PRIMARY KEY,
    OrdnerId       INTEGER REFERENCES Overlayordner(Id) ON DELETE CASCADE,
    Name           TEXT    NOT NULL,
    Farbe          TEXT,
    Eingeschaltet  INTEGER NOT NULL,
    Punkte         BLOB    NOT NULL
);

PRAGMA user_version = {Schemaversion};
```

## 11. Konventionen (wörtlich aus `docs/Konventionen.md`)

Verbindliche Regeln für den Quellcode. Grundlage ist die Projektvorgabe in `CLAUDE.md`:
*„Jede Aufgabe bekommt eine eigene Datei: eigenständige Fachfunktionen einzeln, zusammengehörige Funktionen gemeinsam in einer Klasse. Alles wird ausführlich mit Kommentaren erläutert."*

### Projektstruktur

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

### „Eine Aufgabe pro Datei"

- **Eigenständige Fachfunktionen** (Berechnungen, Import-Schritte, Prüfungen) werden als `static class` mit genau **einer** öffentlichen Methode `Ausfuehren(...)` geschrieben. Der Klassenname beschreibt die Funktion als Verb:
  `Berechnungen/BerechneDurchschnittsgeschwindigkeit.cs` → `BerechneDurchschnittsgeschwindigkeit.Ausfuehren(distanzKm, fahrzeit)`
- **Zusammengehörige Funktionen**, die dasselbe Format oder dieselbe Tabelle teilen, stehen gemeinsam in einer `static class`, deren Name das Thema als Substantiv nennt. Die Methoden heißen nach ihrer Tätigkeit:
  - Tabellen der Datenbank: `…Tabelle` mit `Laden`/`Speichern`, z. B. `TrainingseinheitenTabelle.Laden(verbindung)`
  - Einstellungen (Tabelle `Einstellung`): `…Einstellung` mit `Laden`/`Speichern`, z. B. `FensterpositionEinstellung.Speichern(verbindung, position)`
  - Hin- und Rückumwandlung eines Formats, z. B. `DatumSpalte.Lesen`/`DatumSpalte.AlsText`
- Faustregel: Ändert sich das eine, muss fast immer auch das andere geändert werden → eine Datei. Sonst → getrennte Dateien.
- **Modelle, ViewModels, Fenster** sind je eine Klasse pro Datei; sie enthalten nur Daten bzw. das Weiterreichen an Fachfunktionen, keine eigene Fachlogik.
- Dateiname = Klassenname.

### Kommentare

- Jede Klasse, Methode und öffentliche Eigenschaft bekommt einen XML-Doc-Kommentar (`/// <summary>`), bei Bedarf mit `<param>`, `<returns>` und `<remarks>`.
- Im Methodenrumpf erklären Inline-Kommentare das **Warum** und die einzelnen Rechenschritte, insbesondere Formeln, Einheiten und Sonderfälle.
- XAML-Dateien beginnen mit einem Kommentar, der den Aufbau des Fensters beschreibt.
- Wo eine Funktion dem Original BikeXperience nachgebildet ist, wird die Java-Klasse genannt (z. B. „entspricht `datamodel.PowerCalc`").

### Benennung

- Bezeichner auf **Deutsch**, ohne Umlaute im Code (`Aktivitaet`, `Hoehenmeter`, `Ausfuehren`); in Texten, Kommentaren und der Oberfläche mit korrekten Umlauten.
- Klassen, Methoden, Eigenschaften: `PascalCase`; lokale Variablen und Parameter: `camelCase`; private Felder: `_camelCase`.
- Einheiten im Namen, wenn sie nicht offensichtlich sind: `DistanzKm`, `GewichtKg`, `FahrzeitSekunden`.

### Anzeige

- **Equipment-Namen** werden in Listen, Tabellen und Auswahlen immer über `EquipmentAnzeigename.Bilden(equipment)` gebildet, damit ausgemustertes Equipment überall den Zusatz „(ausgemustert)“ trägt. Wird ein angezeigter Name wieder in ein Equipment übersetzt (Filter, Zellbearbeitung), vergleicht `EquipmentAnzeigename.Passt`. Hinweis- und Fehlertexte nennen den gespeicherten Namen.

### Technik

- .NET 10 (`net10.0`, App: `net10.0-windows10.0.19041.0`), C# `latest`, Nullable aktiviert – gemeinsam festgelegt in `Directory.Build.props`; die SDK-Version (10.0.4xx, keine Vorabversionen) legt `global.json` fest.
- Formatierung nach `.editorconfig` (4 Leerzeichen, dateibezogene Namespaces, geschweifte Klammern immer).
- Oberfläche nach MVVM mit `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`).
- Fachfunktionen in Core bekommen Unit-Tests in `tests/AndyTours.Tests`.
- Einstellungen, die zum Datenbestand gehören (z. B. Datensicherung, später persönliche Daten), liegen in der Tabelle `Einstellung` (Schlüssel/Wert) der Datenbank im Datenordner – nicht unter `%APPDATA%`. Sie wandern so mit dem Datenordner mit. Die Schlüssel stehen in `Einstellungsschluessel` und tragen den Bereich als Vorsilbe (`Sicherung.Intervall`). Unter `%APPDATA%\AndyTours` liegt nur die Merkdatei des Datenordners.

## 12. Architekturentscheidungen (wörtlich aus `docs/adr/`)

### 0001 · Diagrammbibliothek

#### Entscheidung
Wir verwenden OxyPlot (`OxyPlot.Wpf`) für alle Diagramme der App, beginnend mit dem Jahresfortschritt auf der Karteikarte „Aktuell“ und später für das Höhenprofil.

#### Warum
Alles, was das Jahresdiagramm braucht, ist eingebaut: Linie mit ausgefüllter Fläche, Punkte, gestrichelte Linien, Markierungen und der Hinweis beim Überfahren eines Punkts. Das Diagramm wird als Datenmodell im ViewModel beschrieben und passt damit zu MVVM. Die Bibliothek ist seit vielen Jahren bewährt, kostenlos (MIT-Lizenz) und schafft auch Höhenprofile mit einigen tausend Punkten. Dass sie nur noch langsam weiterentwickelt wird und optisch eher schlicht ist, nehmen wir in Kauf.

#### Alternativen
- ScottPlot (`ScottPlot.WPF`): sehr aktiv gepflegt und schnell bei vielen Punkten, aber den Hinweis beim Überfahren eines Punkts müssten wir selbst programmieren, und Zoomen/Verschieben müsste abgeschaltet werden.
- LiveCharts2 (`LiveChartsCore.SkiaSharpView.WPF`): modern mit Animationen, aber lange nur als Vorabversion mit häufig geänderten Schnittstellen.

### 0002 · Zugriff auf Mediengeräte

#### Entscheidung
AndyTours greift über das NuGet-Paket `MediaDevices` (Version 2.0.0, MIT-Lizenz) auf den Garmin Edge 1040 zu, der sich per USB nur als Mediengerät (MTP) und nicht als Laufwerk meldet. Der Zugriff liegt im App-Projekt (`net10.0-windows`); Core, Import und Tests bleiben plattformneutral (`net10.0`).

#### Warum
Wenig und gut lesbarer Code für Geräte auflisten, Ordner lesen und Dateien kopieren; das Paket ist aktuell, unterstützt .NET 10 unter Windows und steht unter MIT-Lizenz.

#### Alternativen
- Windows-Schnittstelle für tragbare Geräte (WPD) selbst anbinden: keine fremde Abhängigkeit, aber viel fehleranfälliger Interop-Code, der selbst gepflegt und getestet werden müsste.
- Windows-Shell (Explorer-Funktionen): ohne Paket, aber Kopieren läuft im Hintergrund, kann Explorer-Dialoge zeigen, und die Ordnernamen sind übersetzt – für einen verlässlichen Import zu wackelig.

### 0003 · Kartenbibliothek

#### Entscheidung
AndyTours zeigt Karten mit dem NuGet-Paket `Mapsui.Wpf` (Version 5.1.0, MIT-Lizenz), einer nativen WPF-Kartenansicht. Kartenausschnitte von OpenStreetMap und OpenTopoMap werden mit der Kennung „AndyTours“ geladen und auf dem Rechner zwischengespeichert; Strecke sowie Start- und Zielzeichen liegen als Ebenen auf der Karte. Das Paket kommt nur ins App-Projekt (`net10.0-windows`); Core, Import und Tests bleiben ohne Kartenbibliothek.

#### Warum
Mapsui ist die einzige aktiv gepflegte native WPF-Karte, die unter .NET 10 läuft (gebaut für .NET 8/9, von .NET 10 nutzbar). Kartenausschnitte mit Zwischenspeicher, eigene Kennung, Linien und Zeichen sowie Mausrad und Ziehen sind eingebaut, ohne Umweg über einen eingebetteten Browser.

#### Alternativen
- `GMap.NET.WinPresentation` 2.1.7: bewährt und mit fertigem Zwischenspeicher, aber seit Juni 2022 nicht mehr aktualisiert und nur bis .NET 6 gebaut – ein Risiko unter .NET 10.
- Browser-Steuerelement (WebView2) mit Leaflet: viele Beispiele und moderne Darstellung, aber eingebetteter Browser, Kartenlogik in JavaScript, Datenaustausch über Skripte und ein mühsamer Zwischenspeicher für die Offline-Nutzung.

### 0004 · Routingdienst

#### Entscheidung
Die Tourenplanung ermittelt Strecken über einen Online-Routingdienst, den der Benutzer in den Einstellungen wählt.
Zur Wahl stehen zwei Dienste:

- **BRouter (voreingestellt):** ohne Zugangsschlüssel, Serveradresse änderbar, anfangs `https://brouter.de/brouter`.
  Die Fahrprofile gehen an die BRouter-Profile: Rennrad → `fastbike`, Trekking/Tourenrad → `trekking`,
  Mountainbike → `mtb`. Höhen kommen je Streckenpunkt, Abbiegehinweise über `timode`.
- **openrouteservice:** mit Zugangsschlüssel des Benutzers. Die Fahrprofile gehen an `cycling-road`,
  `cycling-regular` und `cycling-mountain`, Höhen und Fahranweisungen werden mit angefordert.

Fähren werden bei beiden Diensten gemieden (BRouter `profile:allow_ferries=0`, openrouteservice `avoid_features`),
sonst führt die Strecke z. B. mit der Fähre quer über den Chiemsee. Jede Anfrage trägt die Kennung „AndyTours“. Die Antwort jedes Dienstes wandelt eine eigene Fachfunktion im Core in
dasselbe Teilstück-Modell um, sodass die übrige Planung vom gewählten Dienst nichts weiß. Dienst, Serveradresse und
Zugangsschlüssel liegen in der Tabelle `Einstellung` des Datenordners. Der Schlüssel erscheint im
Einstellungsfenster nur maskiert und wird nirgends protokolliert. Erreicht die App den Dienst nicht, wird das
Teilstück eine Luftlinie (Spec 030).

#### Warum
BRouter ist kostenlos, braucht kein Konto, ist auf Fahrräder ausgelegt und liefert alles, was die Planung braucht:
Fahrprofile, Höhen und Abbiegehinweise. Eine Testabfrage gegen `brouter.de` hat das für alle drei Profile
bestätigt. Der öffentliche Server wird privat betrieben, eine Verfügbarkeit ist nicht zugesichert. openrouteservice
dient deshalb als wählbarer Ausweichdienst mit eigenem Schlüssel. Die änderbare BRouter-Adresse erlaubt später
einen anderen oder eigenen BRouter-Server.

#### Alternativen
- Nur BRouter: am einfachsten, aber ohne Ausweichmöglichkeit, wenn der öffentliche Server ausfällt.
- Zusätzlich GraphHopper: gute Fahranweisungen, aber ein weiterer Dienst mit Schlüssel und eigener Umwandlung. Ob das
  freie Kontingent Rennrad und Mountainbike enthält, ist ungeprüft.
- Nur openrouteservice: von einem Institut betrieben, aber jeder Benutzer bräuchte von Anfang an Konto und Schlüssel.

### 0005 · Installationsprogramm

#### Entscheidung
Wir bauen das Installations- und Deinstallationsprogramm von AndyTours mit Inno Setup.

#### Warum
- Kostenlos.
- Installiert nur für den Benutzer, ohne Administratorrechte.
- Erlaubt eigene Seiten mit Rückfragen, z. B. für den Datenordner und die Übernahme der bisherigen Daten.
- Bringt schon mit: den Eintrag unter „Apps“, ein Deinstallationsprogramm und das Erkennen einer laufenden App.

#### Alternativen
- WiX/MSI: lässt sich in `dotnet build` einbinden, aber eigene Seiten mit Rückfragen sind deutlich aufwendiger.
- Eigenes C#-Programm: volle Kontrolle, aber Eintrag unter „Apps“, Selbstlöschen des Deinstallationsprogramms und Erkennen einer laufenden App müssten selbst gebaut werden.

### 0006 · Grundlage der Oberfläche
Teilweise ersetzt durch [0008](docs/adr/0008-kontrastfarben-der-oberflaeche.md) (eigene Farben und Reitervorlage für mehr Kontrast)

#### Entscheidung
AndyTours verwendet das in WPF (.NET 10) eingebaute Fluent-Design. Es wird anwendungsweit an einer Stelle eingeschaltet (`ThemeMode="Light"` in `App.xaml`) und gilt für alle Fenster. Die App ist immer hell, einen dunklen Modus gibt es nicht. Künftige Fenster nutzen die Standardsteuerelemente ohne eigene Vorlagen; eigene Farben werden nur für fachliche Hervorhebungen gesetzt (Bestwerte, Alarmzustände, Fehler, Hinweise). Die Meldung `WPF0001` (experimentelle Schnittstelle) wird im App-Projekt gezielt abgeschaltet.

#### Warum
Das Design gehört zu .NET 10: kein neues Paket, keine Lizenzfrage. Es bringt das Windows-11-Aussehen mit abgerundeten Steuerelementen und der Akzentfarbe von Windows, ohne eigene Stile pflegen zu müssen, und wird mit einer Zeile für alle Fenster eingeschaltet – auch neue Fenster sehen ohne Zutun passend aus. Nur hell, weil Diagramme, Karten und die fachlichen Farben (rote Bestwerte, rote und orange Alarmzeilen) für hellen Hintergrund gemacht sind; ein dunkler Modus hätte sie alle verdoppelt. Dass die Schnittstelle noch als experimentell gekennzeichnet ist und sich in einer späteren .NET-Version ändern kann, nehmen wir in Kauf; betroffen wäre nur die eine Stelle in `App.xaml`.

#### Alternativen
- Eigene Stile auf dem klassischen WPF-Aussehen: volle Kontrolle und nichts Experimentelles, aber viel XAML, das für jedes Steuerelement selbst gepflegt werden muss.
- Fremde Bibliothek (z. B. WPF-UI): sehr modern und ausgereift, aber eine zusätzliche Abhängigkeit mit eigenen Steuerelementen und eigenem Versionswechsel.
- Das Fluent-Wörterbuch `Fluent.Light.xaml` direkt in `App.xaml` einbinden statt `ThemeMode`: ebenfalls hell und eingebaut, aber nicht der vorgesehene Weg; bleibt als Ausweg, falls sich `ThemeMode` ändert.

### 0007 · Skalierung der Oberfläche

#### Entscheidung
Die Oberfläche wird zentral über eine gemeinsame Vergrößerung des Fensterinhalts skaliert (eine `ScaleTransform` als `LayoutTransform`, gesteuert von `Oberflaechenskalierung`). Verbindlich für alle Fenster, auch künftige:
- Jedes Fenster meldet sich nach `InitializeComponent` bei `Oberflaechenskalierung` an; feste Fenstergrößen werden dabei mit der Stufe umgerechnet, das Hauptfenster behält seine gemerkte Größe und Position.
- Jeder Dialog hat unten eine feste Schaltflächenleiste und darüber einen rollbaren Inhalt und wird höchstens so groß wie der Arbeitsbereich des Bildschirms.
- Kartensteuerelemente (Mapsui) werden mit `Oberflaechenskalierung.Ausnehmen` ausgenommen; das Kartenbild bleibt in Originalgröße, alles um die Karte herum wächst mit.

Die Stufe gehört zum Datenbestand und steht in der Tabelle `Einstellung`. Die Entscheidung ergänzt [0006](docs/adr/0006-grundlage-der-oberflaeche.md).

#### Warum
- Eine einzige Stelle vergrößert alle Fenster gleichmäßig – Schrift, Felder, Abstände, Tabellen und feste Breiten – und wechselt die Stufe sofort, ohne Neustart.
- Gespeicherte Spaltenbreiten, Fensterpositionen und Kartenausschnitte bleiben in Einheiten von 100 % und stimmen deshalb in jeder Stufe.
- Die feste Schaltflächenleiste mit rollbarem Inhalt hält Dialoge auch bei 200 % auf kleineren Bildschirmen bedienbar.
- Karten werden ausgenommen, weil Mapsui ein Bitmap zeichnet, das vergrößert grob und unscharf würde und weniger Kartenausschnitt zeigte; die Kacheln bringen ihre Beschriftung ohnehin in fester Größe mit.

#### Alternativen
- Nur die Schriftgröße erhöhen: einfach, aber Felder, Abstände und feste Breiten wachsen nicht mit, Texte würden abgeschnitten.
- Die Windows-Skalierung nutzen: vorhanden, wirkt aber auf alle Programme statt nur auf AndyTours und lässt sich nicht aus der App heraus einstellen.
- Skalierung in jedem Fenster selbst per Bindung im XAML: kein Anmelden nötig, aber feste Fenstergrößen, aufklappende Elemente und die Kartenausnahme müssten in jedem Fenster einzeln gelöst werden.
- Karten mitvergrößern: einheitlicher, aber das Kartenbild wird unscharf und zeigt weniger Ausschnitt.

### 0008 · Kontrastfarben der Oberfläche

#### Entscheidung
Auf dem Fluent-Design (ADR 0006) setzt AndyTours app-weit in `App.xaml` eigene Farben für mehr Kontrast. Sie gelten für alle Fenster, auch künftige:
- Karteikarten: Die Seite ist hellgrau (`#F0F2F5`) und rundum grau gerahmt (`#8F98A3`). Der gewählte Reiter hat die Farbe der Seite, denselben Rahmen und eine halbfette Beschriftung und bildet mit der Seite eine Fläche. Nicht gewählte Reiter sind dunkler grau (`#DCE1E7`), beim Darüberfahren noch etwas dunkler (`#CDD4DC`).
- Hauptmenüpunkte („Datei“, „Einheit“ …) haben die Farben der nicht gewählten Reiter. Hervorgehobene Menüpunkte, auch in aufklappenden Menüs und Kontextmenüs, sind dunkler grau (`#CDD4DC`).
- Eingabe-, Auswahl- und Datumsfelder sind deckend weiß. Listen sind weiß mit feinem Rahmen.

Der Reiter bekommt dafür eine eigene, schlichte Vorlage in Fluent-Form (Rahmen oben und seitlich, oben abgerundet). Alles andere ändert nur Eigenschaften und Farbressourcen der Fluent-Stile.

#### Warum
Im Fluent-Design sind Reiter und Hauptmenüpunkte durchsichtig und ohne Rahmen, sodass sie vor dem Hintergrund kaum erkennbar sind. Eingabefelder sind halb durchsichtig weiß und Listen grau, auf grauem Grund wirken sie selbst grau und blass. Der Benutzer wollte eine graue Seite mit weißen Feldern und Listen darin. Den gewählten Reiter färbt die Fluent-Vorlage über eine Zustandsanimation fest weiß; das lässt sich weder über Eigenschaften noch über Ressourcen ändern, deshalb die eigene Vorlage.

#### Alternativen
- Fluent unverändert lassen: keine eigenen Farben, aber Reiter und Menüpunkte bleiben kaum erkennbar.
- Weiße Seite mit weißem Reiter: ausprobiert und verworfen, weil Seite, Reiter und Felder ineinander verschwimmen und die graue Liste blass wirkt.
- Gewählten Reiter in Fluent-Weiß auf grauer Seite lassen: ohne eigene Vorlage, aber Reiter und Seite bilden keine Fläche.

## 13. Funktionen im Einzelnen (Spezifikationen und Planentscheidungen)

In Umsetzungsreihenfolge. Je Funktion: Abhängigkeiten aus der Map, Spezifikation (wörtlich aus `spec.md`) und die mit dem Benutzer abgestimmten Entscheidungen aus `plan.md`.

### 001 – Datenordner wählen

Abhängig von: —

#### Was und warum
Alle Daten von AndyTours (Datenbestand, Aufzeichnungen, Anhänge) liegen in einem Datenordner. Der Benutzer wählt diesen Ordner frei und kann ihn in den Einstellungen ändern, damit die Daten z. B. auf dem NAS liegen können. Während der Entwicklung arbeitet die App immer mit einem eigenen Ordner im Projekt, damit die echten Daten unberührt bleiben.

#### Kriterien
- Wenn die fertige App zum ersten Mal startet, dann verwendet sie ohne Rückfrage den Ordner `Dokumente\AndyTours` als Datenordner.
- Wenn der Datenordner beim Start noch nicht existiert, dann legt die App ihn an.
- Wenn die App während der Entwicklung gestartet wird, dann verwendet sie immer den Ordner `AppDaten` im Projektordner, unabhängig von der Einstellung der fertigen App.
- Wenn der Benutzer im Menü „Extras → Einstellungen…“ wählt, dann öffnen sich die Einstellungen und zeigen den vollständigen Pfad des aktuellen Datenordners.
- Wenn die App während der Entwicklung läuft, dann lässt sich der Datenordner in den Einstellungen nicht ändern und ein Hinweis erklärt, warum.
- Wenn der Benutzer in der fertigen App einen anderen Ordner wählt und bestätigt, dann arbeitet die App sofort mit den Daten dieses Ordners und zeigt den Ordner in der Statusleiste an.
- Wenn der Benutzer die fertige App nach einem Ordnerwechsel neu startet, dann ist der zuletzt gewählte Ordner aktiv.
- Wenn der Benutzer die Ordnerauswahl abbricht, dann bleibt der bisherige Datenordner aktiv.
- Wenn der Benutzer einen Ordner wählt, in dem die App nicht schreiben darf, dann erscheint eine verständliche Meldung und der bisherige Ordner bleibt aktiv.
- Wenn der Benutzer einen Ordner wählt, in dem noch keine AndyTours-Daten liegen, dann fragt die App, ob die bisherigen Daten dorthin kopiert werden sollen.
- Wenn der Benutzer das Kopieren bestätigt, dann liegen danach alle bisherigen Daten im neuen Ordner und die App arbeitet mit ihnen.
- Wenn der Benutzer das Kopieren ablehnt, dann beginnt die App im neuen Ordner mit einem leeren Datenbestand.
- Wenn die App Daten kopiert, dann zeigt sie den Fortschritt (Anteil, Anzahl Dateien, Datenmenge) an und friert dabei nicht ein; andere Aktionen sind bis zum Ende des Kopierens gesperrt.
- Wenn das Kopieren fehlschlägt, dann erscheint eine verständliche Meldung und der bisherige Ordner bleibt aktiv.
- Wenn der Benutzer einen Ordner wählt, in dem bereits AndyTours-Daten liegen, dann erscheint ein Hinweis, dass die dort vorhandenen Daten verwendet werden, mit der Wahl „OK“ oder „Abbrechen“.
- Wenn der Benutzer diesen Hinweis mit „OK“ bestätigt, dann arbeitet die App mit den Daten des gewählten Ordners.
- Wenn der Benutzer diesen Hinweis mit „Abbrechen“ schließt, dann bleibt der bisherige Datenordner aktiv.
- Wenn der Benutzer den Datenordner wechselt, dann bleiben die Daten im bisherigen Ordner unverändert erhalten.
- Wenn der gespeicherte Datenordner beim Start nicht erreichbar ist (z. B. NAS ausgeschaltet), dann erscheint eine Meldung mit der Wahl „Erneut versuchen“, „Anderen Ordner wählen“ oder „Beenden“.

#### Entscheidungen aus dem Plan

- Menüpunkt „Altdaten aus BikeXperience übernehmen…“ wird entfernt (Benutzerwahl a); die Altdaten wurden am 22.10.2026 einmalig per Hilfsprogramm nach `AppDaten/` übernommen (932 Einheiten, 930 Dateien).
- Entwicklung = Debug-Build; Projektordner über die Projektmappe `AndyTours.slnx` gefunden.
- Fachlogik in `AndyTours.Daten`, weil die Tests das WPF-Projekt nicht einbinden können.
- Eigenes Fenster für „nicht erreichbar“, weil das Standard-Meldungsfenster keine eigenen Beschriftungen erlaubt; Rückfragen „kopieren?“ (Ja/Nein) und „vorhandene Daten verwenden?“ (OK/Abbrechen) als Standard-Meldungsfenster.
- Der Wechsel gilt mit Abschluss der Rückfragen, ohne zusätzliches „OK“ im Einstellungsfenster.
- „Anderen Ordner wählen“ beim Start: keine Kopier-Rückfrage (alter Ordner nicht erreichbar); bei leerem Ordner Start mit leerem Bestand.
- „Daten vorhanden“ = Datenbankdatei im Ordner vorhanden.
- Fortschrittsanzeige ohne „Abbrechen“ (nicht in der Spezifikation); Fortschritt je Datei genügt, da die Aufzeichnungen klein sind (Ø ca. 0,3 MB).
- Pufferzeiten für Netzlaufwerke: 1 s bis zum Zurücklesen, bis zu 3 Versuche mit 2 s Pause, Gesamtlimit 20 s.

### 002 – Altdaten umwandeln

Abhängig von: `datenordner-waehlen`

#### Was und warum
Ein eigenständiges Programm außerhalb von AndyTours wandelt die BikeXperience-Daten einmalig in einen AndyTours-Datenordner um: Trainingseinheiten, Räder, Wartungen, Aktivitäten, Kategorien, Schwellwerte, persönliche Daten und Aufzeichnungen. Ein Bericht zeigt, was übernommen wurde und was auffällig war. So braucht die App selbst keinen Altdaten-Import, und die Übernahme lässt sich jederzeit wiederholen, z. B. nach Korrekturen.

#### Kriterien
- Wenn der Benutzer das Programm mit dem BikeXperience-Ordner und einem Zielordner aufruft, dann schreibt es in diesen Zielordner.
- Wenn der Benutzer keinen Zielordner angibt, dann schreibt das Programm in den Ordner `AppDaten` im Projektordner.
- Wenn der Benutzer auch keinen BikeXperience-Ordner angibt, dann liest das Programm aus dem Ordner `Daten` im Projektordner.
- Wenn die Umwandlung fertig ist, dann enthält der Zielordner alle Trainingseinheiten, Räder, Wartungen, Aktivitäten, Kategorien, Schwellwerte und persönlichen Daten aus BikeXperience.
- Wenn die Umwandlung fertig ist, dann liegen alle Aufzeichnungen und Anhänge im Zielordner und lassen sich den Trainingseinheiten zuordnen.
- Wenn die Umwandlung fertig ist, dann liegt im Zielordner ein Bericht mit den übernommenen Mengen, den Jahressummen (Anzahl, km, hm, Fahrzeit), den Hinweisen, Warnungen und nicht reparierten Umlauten.
- Wenn die Umwandlung fertig ist, dann zeigt das Programm eine kurze Zusammenfassung (Mengen, Anzahl der Warnungen) und wo der Bericht liegt.
- Wenn die Jahressummen im Bericht mit der Statistik von BikeXperience verglichen werden, dann stimmen sie überein.
- Wenn Texte beschädigte Umlaute enthalten, dann werden sie repariert, soweit das eindeutig möglich ist, und die übrigen Stellen nennt der Bericht.
- Wenn ein Tourname „Stoisser B?ckeralm“ mit beschädigtem Umlaut lautet, dann heißt er nach der Umwandlung „Stoisser Bäckeralm“.
- Wenn ein Tourname die Abkürzung „(?PD)“ mit beschädigtem Umlaut enthält, dann lautet sie nach der Umwandlung „(ÜPD)“.
- Wenn zu einer Trainingseinheit die Aufzeichnung fehlt, dann wird die Einheit trotzdem übernommen und der Bericht nennt die fehlende Datei.
- Wenn eine Zeile der Altdaten fehlerhaft ist, dann wird der Rest trotzdem übernommen und der Bericht nennt die Zeile.
- Wenn die alte Konfiguration Zugangsdaten enthält (Dropbox, Web-Zugang), dann werden diese nicht übernommen.
- Wenn im angegebenen BikeXperience-Ordner keine BikeXperience-Daten liegen, dann meldet das Programm das verständlich und schreibt nichts.
- Wenn der Zielordner bereits AndyTours-Daten enthält, dann fragt das Programm, ob sie ersetzt werden sollen; ohne ausdrückliches Ja bleibt alles unverändert.
- Wenn der Benutzer das Ersetzen bestätigt, dann enthält der Zielordner danach nur die neu umgewandelten Daten.
- Wenn die Umwandlung mit einem Fehler abbricht, dann bleibt der Zielordner wie vorher.
- Wenn die Umwandlung läuft, dann bleibt der BikeXperience-Ordner unverändert.

#### Entscheidungen aus dem Plan

- BikeXperience-Ordner optional mit Vorgabe `Daten` im Projektordner (Benutzerwahl b).
- Kriterium „mehrere Sportler“ gestrichen: Ein BikeXperience-Datenordner enthält immer genau einen Sportler.
- Zwischenordner liegt direkt neben dem Ziel (gleiches Laufwerk), damit der Austausch ein schnelles Umbenennen ist – auch auf dem NAS.
- Zielordner mit fremden Dateien, aber ohne AndyTours-Datenbank: Abbruch mit Meldung, damit nichts Fremdes gelöscht wird.
- Rückfrage: nur „j“/„ja“ (ohne Groß-/Kleinschreibung) gilt als Ja; keine Eingabe (z. B. umgeleitete Eingabe) = Nein.
- Fachfunktionen liegen im Programm selbst (sie brauchen Import und Daten zugleich); das Testprojekt verweist auf das Programm.

### 003 – Jahres- und Aktivitätsauswahl im Hauptfenster

Abhängig von: `datenordner-waehlen`

#### Was und warum
Im Hauptfenster wählt der Benutzer ein Jahr und eine Aktivität. Neben der Auswahl sieht er sofort die Summen dieser Auswahl: Anzahl der Einheiten, Kilometer und Höhenmeter. Fast alle späteren Ansichten (Tabelle, „Aktuell“, Auskünfte) richten sich nach dieser Auswahl.

#### Kriterien
- Wenn das Hauptfenster geöffnet wird, dann kann der Benutzer ein Jahr oder „alle Jahre“ und eine Aktivität oder „alle Aktivitäten“ auswählen.
- Wenn der Benutzer die Jahresliste öffnet, dann enthält sie alle Jahre, in denen es Einheiten gibt, und immer auch das aktuelle Jahr, das neueste zuerst.
- Wenn der Benutzer die Aktivitätsliste öffnet, dann enthält sie alle Aktivitäten.
- Wenn das Programm startet, dann sind das aktuelle Jahr und die Hauptaktivität vorausgewählt.
- Wenn das aktuelle Jahr noch keine Einheiten hat, dann steht es trotzdem in der Liste und ist vorausgewählt.
- Wenn eine Auswahl getroffen ist, dann stehen oben neben der Auswahl die Anzahl der Einheiten, die Kilometer und die Höhenmeter dieser Auswahl.
- Wenn der Benutzer Jahr oder Aktivität ändert, dann zeigen die Summen sofort die Werte der neuen Auswahl.
- Wenn die Auswahl keine Einheiten enthält, dann zeigen die Summen 0.
- Wenn der Datenordner keine Einheiten enthält, dann erscheint ein Hinweis, dass noch keine Touren vorhanden sind.
- Wenn der Benutzer den Datenordner wechselt, dann werden Auswahllisten, Vorauswahl und Summen mit den Daten des neuen Ordners neu aufgebaut.
- Wenn ein Jahr und „alle Aktivitäten“ gewählt sind, dann stimmen Anzahl, Kilometer und Höhenmeter mit den Jahressummen im Umwandlungsbericht überein.

#### Entscheidungen aus dem Plan

- Der Hinweis „Noch keine Touren vorhanden.“ steht an der Stelle der Summen, damit er nicht übersehen wird.
- Gibt es keine Hauptaktivität, ist „alle Aktivitäten“ vorausgewählt.
- Die Oberfläche selbst wird nicht automatisch getestet (das Testprojekt verweist nicht auf die WPF-App); deshalb steckt die Logik in testbaren Fachfunktionen, und die Anzeige wird manuell geprüft.

### 004 – Automatische Datensicherung

Abhängig von: `datenordner-waehlen`

#### Was und warum
AndyTours sichert die Datenbank automatisch beim Programmstart, jeweils nach einer einstellbaren Zahl von Programmstarts, in einen frei wählbaren Sicherungsordner. Die Sicherung ist immer aktiv. Nur die eingestellte Höchstzahl an Sicherungen wird aufbewahrt, die ältesten werden gelöscht. So gehen bei einem Defekt oder Bedienfehler höchstens die Änderungen der letzten Programmstarts verloren, wie bei BikeXperience.

Standardwerte, solange in den Einstellungen nichts gewählt ist: alle 3 Programmstarts, höchstens 10 Sicherungen, Sicherungsordner `Sicherungen` im Datenordner. Das Zurückspielen einer Sicherung ist nicht Teil dieser Funktion (siehe `sicherung-wiederherstellen`).

#### Kriterien
- Wenn die App startet und seit der letzten Sicherung die eingestellte Zahl von Programmstarts erreicht ist, dann legt sie vor der Anzeige der Touren eine Kopie der Datenbank mit Datum und Uhrzeit im Namen im Sicherungsordner an.
- Wenn die eingestellte Zahl von Programmstarts noch nicht erreicht ist, dann legt die App keine Kopie an.
- Wenn eine Sicherung angelegt wurde, dann beginnt die Zählung der Programmstarts von vorn.
- Wenn eine Sicherung angelegt wurde, dann ist sie vollständig: zurückgespielt enthält sie alle Touren, Räder und Wartungen mit dem Stand zum Zeitpunkt der Sicherung.
- Wenn danach mehr Sicherungen vorhanden sind als die eingestellte Höchstzahl, dann löscht die App die ältesten; andere Dateien im Sicherungsordner bleiben unberührt.
- Wenn die Sicherung fehlschlägt (Ordner nicht erreichbar, nicht beschreibbar oder Datenträger voll), dann erscheint eine verständliche Meldung, die App startet normal weiter und versucht es beim nächsten Start erneut.
- Wenn die Datenbank noch keine Touren enthält, dann legt die App keine Sicherung an.
- Wenn der Benutzer in den Einstellungen nichts gewählt hat, dann wird alle 3 Programmstarts in den Ordner `Sicherungen` im Datenordner gesichert und es werden höchstens 10 Sicherungen aufbewahrt.
- Wenn der Benutzer in den Einstellungen einen Sicherungsordner wählt, dann sichert die App künftig dorthin, und die Wahl bleibt nach einem Neustart erhalten.
- Wenn der Benutzer in den Einstellungen das Intervall (Anzahl der Programmstarts) oder die Höchstzahl der Sicherungen ändert, dann gelten die neuen Werte und bleiben nach einem Neustart erhalten.
- Wenn der Benutzer für das Intervall oder die Höchstzahl einen Wert außerhalb von 1 bis 99 oder keine ganze Zahl eingibt, dann wird der Wert nicht übernommen und ein Hinweis nennt den erlaubten Bereich 1 bis 99.
- Wenn der Benutzer den Datenordner wechselt, dann sichert die App künftig die Datenbank des neuen Ordners.

#### Entscheidungen aus dem Plan

- Einstellungen, die zum Datenbestand gehören, liegen in der Datenbank des Datenordners (Tabelle `Einstellung`), nicht unter `%APPDATA%` (Benutzerwahl a). Sie wandern mit dem Datenordner mit, gelten auch im Debug-Build und dienen später auch für persönliche Daten und Saisonziele.
- Der Zähler zählt die Programmstarts, auch bei leerer Datenbank. Bei einem Fehler bleibt er stehen, sodass die Sicherung beim nächsten Start sofort wieder fällig ist.
- Eine kleinere Höchstzahl wirkt erst bei der nächsten Sicherung (Kriterium „danach“), nicht schon beim Speichern der Einstellung.
- Gültige Werte werden bei der Eingabe sofort gespeichert, ohne eigenes „OK“, wie beim Datenordner. Beim Verlassen des Feldes zu speichern hätte Eingaben verloren, die mit Enter oder Esc bestätigt werden (bei der Umsetzung geändert).
- Kein Fortschrittsfenster: Die Datenbank ist rund 0,3 MB groß, der Wartecursor genügt.
- Der gewählte Sicherungsordner wird beim Auswählen nicht geprüft. Ein unbrauchbarer Ordner fällt bei der nächsten Sicherung als Fehler auf (Kriterium „fehlschlägt“).

### 005 – Sicherung wiederherstellen

Abhängig von: `datensicherung`

#### Was und warum
Der Benutzer kann über „Extras → Sicherung wiederherstellen…“ eine der automatischen Sicherungen aus dem aktuellen Sicherungsordner auswählen und den Datenbestand nach einer Rückfrage auf deren Stand zurücksetzen. So lassen sich ein Defekt oder ein Bedienfehler ohne Handarbeit im Dateisystem beheben.

Vor dem Zurücksetzen sichert die App den aktuellen Stand als zusätzliche, ganz normale Sicherung; damit lässt sich auch das Wiederherstellen selbst rückgängig machen. Die Einstellungen der Datensicherung bleiben dabei unverändert.

#### Kriterien
- Wenn der Benutzer „Extras → Sicherung wiederherstellen…“ wählt, dann zeigt die App alle Sicherungen des aktuellen Sicherungsordners mit Datum und Uhrzeit, die neueste zuerst.
- Wenn der Sicherungsordner keine Sicherungen enthält oder nicht erreichbar ist, dann erscheint ein Hinweis und der Datenbestand bleibt unverändert.
- Wenn der Benutzer die Auswahl abbricht, dann bleibt der Datenbestand unverändert.
- Wenn der Benutzer eine Sicherung auswählt, dann fragt die App nach; die Rückfrage nennt Datum und Uhrzeit der Sicherung und weist darauf hin, dass alle späteren Änderungen verloren gehen.
- Wenn der Benutzer die Rückfrage ablehnt, dann bleibt der Datenbestand unverändert.
- Wenn der Benutzer bestätigt, dann entspricht der Datenbestand dem Stand der Sicherung, und das Hauptfenster zeigt diesen Stand sofort an (Jahre, Aktivitäten, Summen), ohne Neustart.
- Wenn der Benutzer bestätigt, dann legt die App vorher eine Sicherung des aktuellen Stands im Sicherungsordner an; sie zählt wie jede andere Sicherung zur Höchstzahl.
- Wenn die Sicherung des aktuellen Stands fehlschlägt, dann erscheint eine verständliche Meldung, und es wird nichts wiederhergestellt.
- Wenn die gewählte Sicherung beschädigt ist oder keine AndyTours-Daten enthält, dann erscheint eine verständliche Meldung und der Datenbestand bleibt unverändert.
- Wenn das Wiederherstellen fehlschlägt (z. B. Ordner nicht erreichbar, Datenträger voll), dann erscheint eine verständliche Meldung und der bisherige Datenbestand bleibt vollständig erhalten.
- Wenn die Sicherung von einer älteren AndyTours-Version stammt, dann wird sie übernommen und ist danach normal nutzbar.
- Wenn die Sicherung von einer neueren AndyTours-Version stammt, dann lehnt die App sie mit einer Meldung ab und der Datenbestand bleibt unverändert.
- Wenn wiederhergestellt wurde, dann bleiben die Einstellungen der Datensicherung (Sicherungsordner, Intervall, Höchstzahl) so, wie sie vor dem Wiederherstellen waren.

#### Entscheidungen aus dem Plan

- Eigenes Auswahlfenster statt des Windows-Dateidialogs. Nur so lassen sich ausschließlich Sicherungen anzeigen, verständlich mit Datum und Uhrzeit und in fester Reihenfolge.
- Die neue Datenbank wird vollständig in einer Zwischendatei im Datenordner vorbereitet und erst am Ende in einem Schritt ausgetauscht. Bis dahin bleibt der bisherige Bestand unberührt, auch bei einem Abbruch mittendrin.
- Der Startzähler steht nach dem Wiederherstellen auf 0, weil der vorherige Stand gerade gesichert wurde.
- Aufräumen erst nach dem Austausch. Fehler beim Aufräumen gelten nicht als Fehlschlag des Wiederherstellens.
- Das Zeitlimit von 20 s gilt nur für das Lesen des Sicherungsordners. Das Zurückspielen selbst läuft ohne Zeitlimit, denn ein abgebrochener Hintergrundvorgang könnte den Austausch sonst noch nach der Fehlermeldung ausführen.
- Erkannt wird die Sicherung einer neueren Version an der Schemaversion. Eine neuere Programmversion mit unverändertem Schema gilt als passend, was technisch auch zutrifft.

### 006 – Tabelle der Trainingseinheiten

Abhängig von: `hauptfenster-auswahl`

#### Was und warum
Die Karteikarte „Trainingseinheiten“ zeigt alle Einheiten der Jahres- und Aktivitätsauswahl in einer sortierbaren Tabelle, wie in BikeXperience. Eine Summenzeile fasst die angezeigten Einheiten zusammen, die Bestwerte der Auswahl sind rot hervorgehoben. So sieht der Benutzer seine Touren auf einen Blick und erkennt sofort die längste, höchste und schnellste.

Spalten: Datum, Startzeit, Name, Aktivität, Kategorien, Equipment, Distanz, Höhenmeter, Fahrzeit, Ø km/h, max km/h, Ø Puls, max Puls, Trittfrequenz, Temperatur, Maximalhöhe, Trainingsbereich. Auswahl, Reihenfolge und Breite der Spalten passt der Benutzer an, sie bleiben erhalten. Ebenso merkt sich die App Größe und Position des Hauptfensters und die zuletzt gewählte Karteikarte (nach der ersten und zweiten Prüfung ergänzt). Filter, Bearbeiten, Anhänge und Suche sind eigene Funktionen.

#### Kriterien
- Wenn das Hauptfenster geöffnet ist, dann zeigt die Karteikarte „Trainingseinheiten“ alle Einheiten der aktuellen Jahres- und Aktivitätsauswahl als Tabelle, eine Zeile je Einheit.
- Wenn der Benutzer Jahr oder Aktivität ändert oder der Datenbestand neu geladen wird (Datenordnerwechsel, Wiederherstellen einer Sicherung), dann zeigt die Tabelle sofort die passenden Einheiten.
- Wenn die Tabelle angezeigt wird, dann ist sie nach Datum sortiert, die neueste Einheit zuerst.
- Wenn der Benutzer auf eine Spaltenüberschrift klickt, dann wird nach dieser Spalte sortiert; ein erneuter Klick kehrt die Richtung um.
- Wenn der Benutzer mit der rechten Maustaste auf die Spaltenüberschriften klickt, dann kann er jede Spalte ein- oder ausblenden; anfangs sind alle Spalten sichtbar.
- Wenn der Benutzer Spalten ein- oder ausgeblendet hat, dann bleibt diese Wahl nach einem Neustart erhalten.
- Wenn nur noch eine Spalte sichtbar ist, dann lässt sie sich nicht ausblenden.
- Wenn der Benutzer im Spaltenmenü eine Spalte an- oder abwählt, dann bleibt das Menü offen, sodass er mehrere Spalten nacheinander umschalten kann; „Übernehmen“ am Ende des Menüs schließt es.
- Wenn der Benutzer eine Spaltenüberschrift an eine andere Stelle zieht, dann wird die Spalte dorthin verschoben, und die Summenzeile folgt.
- Wenn der Benutzer Reihenfolge oder Breite der Spalten geändert hat, dann bleiben sie nach einem Neustart erhalten.
- Wenn eine Spalte automatisch an ihren Inhalt angepasst wird (Doppelklick auf den Spaltenrand) oder der Benutzer sie verkleinert, dann bleibt sie mindestens so breit, dass ihr Wert in der Summenzeile vollständig sichtbar ist.
- Wenn der Benutzer die App beendet, dann werden Größe, Position und maximierter Zustand des Hauptfensters gemerkt und beim nächsten Start wiederhergestellt.
- Wenn die gemerkte Position außerhalb der vorhandenen Bildschirme liegt, dann erscheint das Hauptfenster auf dem Hauptbildschirm.
- Wenn der Benutzer die App beendet, dann wird die zuletzt gewählte Karteikarte gemerkt und beim nächsten Start wieder angezeigt.
- Wenn die Tabelle Einheiten enthält, dann zeigt eine Summenzeile die Anzahl der angezeigten Einheiten sowie die Summen von Distanz, Höhenmetern und Fahrzeit.
- Wenn der Benutzer in der Tabelle blättert oder sortiert, dann bleibt die Summenzeile fest am unteren Rand sichtbar und wird nicht mitsortiert.
- Wenn eine Einheit in einer der Spalten Distanz, Höhenmeter, Fahrzeit, Ø km/h oder max km/h den höchsten Wert der angezeigten Auswahl hat, dann ist dieser Wert rot dargestellt.
- Wenn mehrere Einheiten denselben Bestwert haben, dann sind alle diese Werte rot.
- Wenn die Auswahl keine Einheiten enthält, dann zeigt die Karteikarte den Hinweis „Keine Einheiten in dieser Auswahl.“ und keine Summenzeile.
- Wenn bei einer Einheit ein Wert fehlt (z. B. kein Puls aufgezeichnet), dann bleibt die Zelle leer statt „0“, und der Wert zählt nicht als Bestwert.
- Wenn Zahlen, Datum und Zeiten angezeigt werden, dann im deutschen Format (z. B. „1.234,5“ km, „Sa, 23.10.2026“, „3:45:12“).

#### Entscheidungen aus dem Plan

- Die Summenzeile ist eine echte Zeile unter den Spalten, gekoppelt an Breite, Sichtbarkeit und waagrechtes Blättern (Benutzerwahl G1).
- Temperatur 0 °C gilt als „nicht erfasst“ und bleibt leer (Benutzerwahl H1).
- Die Spaltenwahl liegt in der Tabelle `Einstellung` des Datenordners, wie es `docs/Konventionen.md` für Einstellungen vorsieht. Gespeichert werden die ausgeblendeten Spalten, damit später ergänzte Spalten sichtbar erscheinen.
- Zeilen, Bestwerte und Formatierung werden in Core berechnet und dort getestet. Das App-Projekt ist nicht testbar (WPF), deshalb enthält der Code-Behind nur Darstellung.
- Ø km/h und Bestwerte werden aus den Rohwerten berechnet, nicht aus den gerundeten Anzeigewerten.
- Eine per Klick gewählte Sortierung bleibt bis zum Programmende erhalten. Gespeichert wird sie nicht (Spec: Start mit Datum, neueste zuerst).
- Nach der ersten Prüfung (Benutzerwahl I1, J1, K1): Die Fenstergröße gehört mit zu 006. Spaltenreihenfolge und -breite sowie Größe, Position und „maximiert“ des Fensters bleiben erhalten. Sie liegen wie die Spaltenwahl in der Tabelle `Einstellung` des Datenordners.
- Fehler beim Speichern von Spaltenanordnung und Fensterposition werden ohne Meldung übergangen. Es sind reine Komfortwerte, und eine Meldung beim Beenden würde nur stören.

### 007 – Filter für die Trainingseinheiten

Abhängig von: `einheiten-tabelle`

#### Was und warum
Über der Tabelle der Trainingseinheiten stehen Filter für Kategorie, Equipment, Trainingsbereich, Distanz, Höhenmeter und Name, wie in BikeXperience. Damit grenzt der Benutzer die Einheiten der Jahres- und Aktivitätsauswahl weiter ein, etwa auf alle Touren mit einem bestimmten Rad oder alle über 100 km, und sieht Summen und Bestwerte genau dieser Einheiten. Die Filter wirken sofort und sind nach einem Neustart wieder leer.

#### Kriterien
- Wenn das Hauptfenster geöffnet ist, dann stehen über der Tabelle Filter für Kategorie, Equipment, Trainingsbereich, Distanz (von–bis), Höhenmeter (von–bis) und Name, und keiner davon ist gesetzt.
- Wenn der Benutzer die App neu startet, dann sind alle Filter wieder leer.
- Wenn der Benutzer eine der Auswahllisten Kategorie, Equipment oder Trainingsbereich öffnet, dann bietet sie nur die Werte an, die in der aktuellen Jahres- und Aktivitätsauswahl vorkommen, alphabetisch sortiert.
- Wenn der Benutzer eine Kategorie wählt, dann zeigt die Tabelle nur Einheiten, denen diese Kategorie zugeordnet ist, auch wenn sie weitere Kategorien haben.
- Wenn der Benutzer ein Equipment wählt, dann zeigt die Tabelle nur Einheiten mit diesem Equipment.
- Wenn der Benutzer einen Trainingsbereich wählt, dann zeigt die Tabelle nur Einheiten mit diesem Trainingsbereich.
- Wenn der Benutzer bei der Distanz einen Von- und/oder Bis-Wert einträgt, dann zeigt die Tabelle nur Einheiten in diesem Bereich, die Grenzwerte eingeschlossen.
- Wenn der Benutzer bei den Höhenmetern einen Von- und/oder Bis-Wert einträgt, dann zeigt die Tabelle nur Einheiten in diesem Bereich, die Grenzwerte eingeschlossen.
- Wenn bei aktivem Distanz- oder Höhenmeterfilter eine Einheit keinen solchen Wert hat, dann wird sie nicht angezeigt.
- Wenn der Benutzer Text in das Namensfeld schreibt, dann zeigt die Tabelle nur Einheiten, deren Name diesen Text enthält, ohne Rücksicht auf Groß- und Kleinschreibung.
- Wenn der Benutzer einen Filter ändert, dann wird die Tabelle sofort angepasst, ohne dass er eine Schaltfläche drücken muss.
- Wenn mehrere Filter gesetzt sind, dann zeigt die Tabelle nur Einheiten, die alle Filter gleichzeitig erfüllen.
- Wenn ein Filter gesetzt ist, dann beziehen sich die Summenzeile und die roten Bestwerte nur auf die angezeigten Einheiten.
- Wenn keine Einheit zu den Filtern passt, dann zeigt die Karteikarte den Hinweis „Keine Einheiten passen zum Filter.“ und keine Summenzeile.
- Wenn bei Distanz oder Höhenmetern keine gültige Zahl steht oder „von“ größer als „bis“ ist, dann wird das Feld rot markiert und dieser Filter nicht angewendet.
- Wenn der Benutzer „Filter zurücksetzen“ wählt, dann werden alle Filter geleert, und die Tabelle zeigt wieder die ganze Jahres- und Aktivitätsauswahl.
- Wenn mindestens ein Filter gesetzt ist, dann ist die Schaltfläche „Filter zurücksetzen“ hervorgehoben, sodass erkennbar ist, dass die Tabelle gefiltert ist.
- Wenn der Benutzer Jahr oder Aktivität wechselt oder der Datenbestand neu geladen wird, dann bleiben die gesetzten Filter erhalten und wirken auf die neue Auswahl.
- Wenn ein gewählter Wert einer Auswahlliste in der neuen Auswahl nicht mehr vorkommt, dann bleibt er gewählt, und die Tabelle zeigt den Hinweis „Keine Einheiten passen zum Filter.“

#### Entscheidungen aus dem Plan

- A1: Die Summe in der Werkzeugleiste zeigt weiterhin die ungefilterte Jahres- und Aktivitätsauswahl; nur Tabelle, Summenzeile und Bestwerte folgen dem Filter.
- B1: Der Eintrag für „kein Filter“ in den Auswahllisten heißt „(alle)“.
- Komma und Punkt gelten beide als Dezimalzeichen; ein ungültiges Feld wird einzeln rot, bei „von“ > „bis“ beide.
- Kategorie und Equipment werden über den Namen gefiltert, damit die Wahl ein Neuladen des Datenbestands übersteht.
- Der Filterzustand bekommt ein eigenes ViewModel, damit `HauptfensterViewModel` nicht weiter wächst.

### 008 – Trainingseinheit anlegen, bearbeiten, kopieren und löschen

Abhängig von: `einheiten-tabelle`

#### Was und warum
Der Benutzer kann Trainingseinheiten neu anlegen, ändern, kopieren und löschen, wie mit dem Dialog in BikeXperience. Das Bearbeitungsfenster zeigt alle Werte einer Einheit, auch die, die in der Tabelle nicht vorkommen. Einzelne Werte lassen sich zusätzlich direkt in der Tabelle ändern. So pflegt er seine Touren ohne Umweg, berichtigt Fehler aus Aufzeichnungen und erfasst Einheiten ohne Aufzeichnung von Hand. Diese Funktion ist auch die Grundlage für die späteren Importe.

Die Befehle stehen im Menü „Einheit“ und im Rechtsklickmenü der Tabelle. Mehrfachaktionen (Kategorie, Aktivität, Equipment), Anhänge öffnen und Werte aus der Aufzeichnung übernehmen sind eigene Funktionen.

#### Kriterien

##### Aufrufen
- Wenn der Benutzer „Neue Einheit“ wählt, dann öffnet sich das Bearbeitungsfenster mit dem heutigen Datum, der Aktivität aus der Werkzeugleiste (bei „alle Aktivitäten“ die Hauptaktivität) und den als Standard markierten Kategorien; alle anderen Felder sind leer.
- Wenn der Benutzer eine Zeile doppelt anklickt oder sie markiert und „Bearbeiten“ wählt, dann öffnet sich das Bearbeitungsfenster mit allen Werten dieser Einheit.
- Wenn der Benutzer eine Einheit markiert und „Kopieren“ wählt, dann öffnet sich das Bearbeitungsfenster mit einer neuen Einheit, die alle Werte der markierten übernimmt, außer dem Datum (heute) und den Anhängen; das Original bleibt unverändert.
- Wenn keine Zeile markiert ist, dann sind „Bearbeiten“, „Kopieren“ und „Löschen“ nicht wählbar; sind mehrere markiert, dann sind „Bearbeiten“ und „Kopieren“ nicht wählbar.
- Wenn der Benutzer die Befehle sucht, dann findet er „Neue Einheit“, „Bearbeiten“, „Kopieren“ und „Löschen“ im Menü „Einheit“ und im Rechtsklickmenü der Tabelle.

##### Bearbeitungsfenster
- Wenn das Bearbeitungsfenster offen ist, dann lassen sich darin Datum, Startzeit, Name, Aktivität, Kategorien (mehrere), Equipment, Distanz, Höhenmeter, Fahrzeit, max km/h, Ø, max und min Puls, Trittfrequenz, Temperatur, Leistung, Körpergewicht, Erholungspuls, Starthöhe, Maximalhöhe, Trainingsbereich und Kommentar bearbeiten.
- Wenn Distanz und Fahrzeit eingetragen sind, dann zeigt das Fenster die daraus berechneten Ø km/h an, nur zur Ansicht.
- Wenn Datum oder Aktivität fehlen oder ein Wert ungültig ist (keine Zahl, negativ außer bei der Temperatur, Fahrzeit nicht im Format „h:mm“ oder „h:mm:ss“; fehlen die Sekunden, gelten sie als 00), dann ist das Feld rot markiert und „Speichern“ nicht möglich; alle anderen Felder dürfen leer bleiben.
- Wenn der Benutzer den Trainingsbereich wählt, dann bietet die Auswahlliste die Kürzel an, die im Datenbestand vorkommen, und einen leeren Eintrag; ein neues Kürzel lässt sich nicht eintippen (das Pflegen der Liste ist eine eigene Funktion).
- Wenn der Benutzer das Equipment wählt, dann bietet die Auswahlliste „(kein)“ und alle nicht ausgemusterten Equipments an; ein ausgemustertes erscheint nur, wenn es der bearbeiteten Einheit bereits zugeordnet ist.
- Wenn der Benutzer „Speichern“ wählt, dann wird die Einheit dauerhaft gespeichert, das Fenster schließt sich, und Tabelle, Summen, Bestwerte, Jahresliste und Statusleiste zeigen sofort den neuen Stand; die gespeicherte Einheit ist in der Tabelle markiert.
- Wenn die gespeicherte Einheit nicht zur aktuellen Jahres-, Aktivitäts- oder Filterauswahl passt, dann bleibt die Auswahl unverändert, und die Statusleiste meldet „Gespeichert – liegt außerhalb der aktuellen Auswahl.“
- Wenn der Benutzer „Abbrechen“ wählt oder das Fenster schließt, ohne etwas geändert zu haben, dann schließt sich das Fenster, und der Datenbestand bleibt unverändert.
- Wenn der Benutzer „Abbrechen“ wählt oder das Fenster schließt, nachdem er etwas geändert hat, dann fragt die App „Änderungen verwerfen?“; bei „Ja“ schließt sich das Fenster ohne Speichern, bei „Nein“ bleibt es mit den Eingaben offen.
- Wenn eine Einheit mit Anhängen bearbeitet wird, dann bleiben ihre Anhänge erhalten.

##### Bearbeiten in der Tabelle
- Wenn der Benutzer in einer markierten Zelle F2 drückt, die bereits markierte Zelle anklickt oder lostippt, dann kann er den Wert direkt in der Tabelle ändern; ein Doppelklick öffnet weiterhin das Bearbeitungsfenster.
- Wenn der Benutzer eine Zelle bearbeitet, dann lassen sich alle Spalten außer Ø km/h und Kategorien ändern; Aktivität, Equipment und Trainingsbereich werden aus einer Auswahlliste gewählt.
- Wenn der Benutzer die Zelle verlässt oder Enter drückt, dann wird die Änderung sofort dauerhaft gespeichert.
- Wenn der Benutzer während der Bearbeitung einer Zelle Esc drückt, dann wird die Änderung verworfen und der alte Wert angezeigt.
- Wenn der Benutzer in einer Zelle einen ungültigen Wert einträgt (z. B. „abc“ bei Distanz, ein leeres Datum, eine negative Zahl außer bei der Temperatur), dann wird die Zelle rot markiert und bleibt in Bearbeitung, bis der Wert gültig ist oder der Benutzer mit Esc abbricht.
- Wenn ein Wert in der Tabelle geändert wurde, dann werden Ø km/h, Summenzeile und rote Bestwerte sofort neu berechnet; ist die Tabelle nach der geänderten Spalte sortiert, springt die Zeile an ihre neue Stelle.

##### Löschen
- Wenn der Benutzer eine Einheit markiert und „Löschen“ wählt oder die Entf-Taste drückt (nicht während der Bearbeitung einer Zelle), dann fragt die App „Einheit ‚Name‘ vom Datum wirklich löschen?“ und löscht erst nach „Ja“.
- Wenn eine Einheit mit Anhängen gelöscht wird, dann bleiben die Dateien der Anhänge im Datenordner erhalten; nur ihre Zuordnung zur Einheit entfällt.
- Wenn mehrere Einheiten markiert sind und der Benutzer löscht, dann fragt die App einmal mit der Anzahl („3 Einheiten wirklich löschen?“) und löscht nach „Ja“ alle.
- Wenn der Benutzer bei der Rückfrage „Nein“ wählt, dann bleibt alles unverändert.
- Wenn Einheiten gelöscht sind, dann verschwinden sie sofort aus der Tabelle, und Summen, Bestwerte, Jahresliste und Statusleiste zeigen den neuen Stand.

##### Fehler
- Wenn das Speichern oder Löschen fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und der angezeigte Datenbestand bleibt beim vorherigen Stand; ein offenes Bearbeitungsfenster bleibt mit den Eingaben offen, eine Zelle zeigt wieder ihren alten Wert.

#### Entscheidungen aus dem Plan

- A1: Negative Werte sind bei der Temperatur erlaubt (Spec angepasst).
- B2: Der Trainingsbereich kommt aus einer festen Liste der vorhandenen Kürzel; das Pflegen der Liste in den Einstellungen ist der neue Map-Eintrag `auswahllisten-verwalten`.
- C1: Beim Löschen bleiben die Dateien der Anhänge im Datenordner.
- D1: Ausgemusterte Equipments erscheinen nur, wenn sie der Einheit schon zugeordnet sind; „(kein)“ steht oben.
- Komma und Punkt gelten als Dezimalzeichen, Eingabetexte werden ohne Tausenderpunkt gezeigt; Datum „24.10.2026“ oder „24.10.26“, Startzeit „9:30“, Fahrzeit „h:mm“ oder „h:mm:ss“ (Sekunden freiwillig).
- Ein leeres Zahlenfeld wird als 0 („nicht erfasst“) gespeichert, wie die übernommenen Altdaten.
- Meldungen wie „Gespeichert – liegt außerhalb …“ stehen in einem eigenen Feld der Statusleiste; die Anzahl der Einheiten bleibt sichtbar.
- Gibt es noch keinen Fahrer (leere Datenbank), wird beim ersten Speichern einer angelegt.
- Nach dem Speichern wird der Datenbestand im Speicher angepasst statt neu geladen; das ist schneller und lässt die Tabelle ruhig.

### 009 – Auswahllisten verwalten

Abhängig von: `einheit-bearbeiten`

#### Was und warum
Der Benutzer pflegt in den Einstellungen unter „Auswahllisten“ die festen Werte, die in den Eingabefeldern angeboten werden. Diese Funktion legt den Bereich an und beginnt mit der Liste der Trainingsbereiche. Bisher kamen die angebotenen Kürzel nur aus den vorhandenen Einheiten, ein neues Kürzel ließ sich nicht erfassen und ein falsches nicht berichtigen. Aktivitäten, Kategorien und Equipments kommen später als weitere Listen in denselben Bereich; sie sind eigene Funktionen.

#### Kriterien

##### Pflegen
- Wenn der Benutzer die Einstellungen öffnet, dann findet er einen Bereich „Auswahllisten“ mit einer Auswahl der Liste (zunächst nur „Trainingsbereich“) und darunter deren Einträge in der festgelegten Reihenfolge.
- Wenn die App zum ersten Mal mit dieser Funktion gestartet wird, dann enthält die Liste „Trainingsbereich“ die Kürzel, die im Datenbestand vorkommen, alphabetisch geordnet.
- Wenn der Benutzer einen Eintrag hinzufügt, dann erscheint er am Ende der Liste und ist sofort dauerhaft gespeichert.
- Wenn ein neuer oder umbenannter Eintrag leer ist oder es ihn in der Liste schon gibt (ohne Unterschied zwischen Groß- und Kleinschreibung; Leerzeichen am Anfang und Ende werden entfernt), dann wird er abgewiesen, und ein Hinweis nennt den Grund.
- Wenn der Benutzer einen Eintrag nach oben oder unten verschiebt, dann gilt die neue Reihenfolge sofort und bleibt beim nächsten Start erhalten.
- Wenn der Benutzer einen Eintrag umbenennt, der in keiner Einheit vorkommt, dann ändert sich nur die Liste, ohne Rückfrage.
- Wenn der Benutzer einen Eintrag umbenennt, der in Einheiten vorkommt, dann fragt die App mit der Anzahl („In 714 Einheiten ‚GA2‘ durch ‚GA 2‘ ersetzen?“) und ändert nach „Ja“ die Liste und alle diese Einheiten; bei „Nein“ bleibt alles unverändert.
- Wenn der Benutzer einen Eintrag löschen will, der in Einheiten vorkommt, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl dieser Einheiten.
- Wenn der Benutzer einen Eintrag, der in keiner Einheit vorkommt, ausdrücklich mit „Löschen“ entfernt, dann verschwindet er ohne weitere Rückfrage aus der Liste; Einträge, die er nicht löscht, bleiben erhalten, auch wenn sie noch keiner Einheit zugeordnet sind.
- Wenn das Speichern einer Änderung fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Liste und Einheiten bleiben beim vorherigen Stand.

##### Verwenden
- Wenn der Benutzer im Bearbeitungsfenster oder in einer Tabellenzelle den Trainingsbereich wählt, dann bietet die Auswahlliste einen leeren Eintrag und die Einträge der Liste in deren Reihenfolge an.
- Wenn die Liste geändert wurde, dann bieten danach geöffnete Bearbeitungsfenster und Tabellenzellen den neuen Stand an.
- Wenn ein Eintrag umbenannt wurde, dann zeigen Tabelle und Filterleiste sofort den neuen Namen.
- Wenn der Benutzer in der Filterleiste nach Trainingsbereich filtert, dann bietet die Liste weiterhin die Werte an, die in den Einheiten vorkommen.

#### Entscheidungen aus dem Plan

- H1: Eingabefeld unter der Liste statt eigener Eingabefenster. Ein Klick auf einen Eintrag übernimmt ihn ins Feld; „Hinzufügen“ fügt den Feldinhalt an, „Umbenennen“ ersetzt den markierten Eintrag. Hinweise erscheinen rot unter dem Feld wie bei der Datensicherung.
- Die Liste steht in der Tabelle `Einstellung`, ohne Schemaänderung. Das folgt dem Muster der übrigen Einstellungen.
- Doppelte Einträge und verwendende Einheiten werden ohne Unterschied zwischen Groß- und Kleinschreibung ermittelt. Das Umbenennen ersetzt auch Schreibvarianten in den Einheiten.
- Ein Wert einer Einheit, der nicht in der Liste steht, wird in ihrer Auswahlliste zusätzlich angeboten, damit er beim Speichern nicht verloren geht. Bei gepflegter Liste kommt das nicht vor.

### 010 – Aktivitäten und Kategorien verwalten

Abhängig von: `hauptfenster-auswahl`

#### Was und warum
Der Benutzer pflegt Aktivitäten und Kategorien in den Einstellungen unter „Auswahllisten“, neben den Trainingsbereichen: anlegen, umbenennen, löschen, Hauptaktivität und Standardkategorien festlegen und zwei Einträge zusammenführen. Bisher gab es nur die aus BikeXperience übernommenen Einträge; neue Sportarten oder Kategorien ließen sich nicht anlegen, doppelte wie „Trainieren“ und „Training“ nicht bereinigen. Geschwindigkeitseinheit und Saisonziele der Aktivitäten gehören nicht dazu.

#### Kriterien

##### Anzeigen
- Wenn der Benutzer in den Einstellungen unter „Auswahllisten“ die Liste wählt, dann findet er neben „Trainingsbereich“ auch „Aktivität“ und „Kategorie“.
- Wenn die Liste „Aktivität“ gewählt ist, dann zeigt sie die Aktivitäten in der festgelegten Reihenfolge, die Hauptaktivität mit dem Zusatz „(Hauptaktivität)“.
- Wenn die Liste „Kategorie“ gewählt ist, dann zeigt sie die Kategorien alphabetisch, Standardkategorien mit dem Zusatz „(Standard)“; „Nach oben“ und „Nach unten“ sind dort nicht wählbar.

##### Anlegen und umbenennen
- Wenn der Benutzer eine Aktivität oder Kategorie hinzufügt, dann ist sie sofort gespeichert und überall wählbar: in der Werkzeugleiste (Aktivität), im Bearbeitungsfenster, in der Tabellenzelle und, sobald Einheiten sie verwenden, im Filter.
- Wenn ein neuer oder umbenannter Name leer ist oder es ihn in der Liste schon gibt (ohne Unterschied zwischen Groß- und Kleinschreibung; Leerzeichen am Anfang und Ende werden entfernt), dann wird er abgewiesen, und ein Hinweis nennt den Grund.
- Wenn der Benutzer eine Aktivität oder Kategorie umbenennt, dann zeigen Tabelle, Werkzeugleiste, Filter und Auswahllisten sofort den neuen Namen; eine Rückfrage gibt es nicht, weil die Zuordnung der Einheiten erhalten bleibt.
- Wenn eine umbenannte Kategorie bei einem Equipment als Standardkategorie eingetragen ist, dann wird sie dort mit umbenannt.

##### Hauptaktivität, Standardkategorien, Reihenfolge
- Wenn der Benutzer eine Aktivität markiert und „Als Hauptaktivität“ wählt, dann ist genau diese die Hauptaktivität: Sie ist beim nächsten Start in der Werkzeugleiste vorgewählt und wird bei „Neue Einheit“ unter „alle Aktivitäten“ vorbelegt.
- Wenn der Benutzer eine Kategorie markiert und „Standard“ umschaltet, dann ist sie Standardkategorie bzw. nicht mehr; Standardkategorien sind bei „Neue Einheit“ angehakt, beliebig viele sind möglich.
- Wenn der Benutzer eine Aktivität nach oben oder unten verschiebt, dann folgen Werkzeugleiste und Auswahllisten sofort der neuen Reihenfolge, und sie bleibt beim nächsten Start erhalten.

##### Löschen
- Wenn der Benutzer eine Aktivität löschen will, die Einheiten oder Equipments verwenden, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl der Einheiten bzw. Equipments.
- Wenn der Benutzer die Hauptaktivität oder die letzte verbleibende Aktivität löschen will, dann ist das nicht möglich, und ein Hinweis nennt den Grund.
- Wenn der Benutzer eine Kategorie löschen will, die Einheiten verwenden oder die bei einem Equipment als Standardkategorie eingetragen ist, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl der Einheiten bzw. das Equipment.
- Wenn eine Aktivität oder Kategorie nicht verwendet wird (und bei einer Aktivität weder Hauptaktivität noch die letzte ist), dann verschwindet sie mit „Löschen“ ohne Rückfrage.

##### Zusammenführen
- Wenn der Benutzer eine Aktivität oder Kategorie markiert und „Zusammenführen mit …“ wählt, dann wählt er in einem kleinen Fenster das Ziel aus den übrigen Einträgen derselben Liste; gibt es keinen anderen Eintrag, ist „Zusammenführen mit …“ nicht wählbar.
- Wenn der Benutzer das Ziel gewählt hat, dann fragt die App mit der Anzahl („42 Einheiten von ‚Trainieren‘ nach ‚Training‘ übernehmen und ‚Trainieren‘ entfernen?“); bei „Nein“ bleibt alles unverändert.
- Wenn der Benutzer zwei Kategorien zusammenführt und „Ja“ wählt, dann haben alle Einheiten der Ursprungskategorie die Zielkategorie – eine Einheit, die beide hatte, hat sie danach nur einmal –, und die Ursprungskategorie ist aus der Liste entfernt.
- Wenn die zusammengeführte Kategorie Standardkategorie war oder bei einem Equipment als Standardkategorie eingetragen ist, dann gilt das danach für die Zielkategorie.
- Wenn der Benutzer zwei Aktivitäten zusammenführt und „Ja“ wählt, dann gehören alle Einheiten und Equipments der Ursprungsaktivität zur Zielaktivität, und die Ursprungsaktivität ist entfernt; war sie die Hauptaktivität, dann ist danach die Zielaktivität die Hauptaktivität.
- Wenn zusammengeführt wurde, dann zeigen Tabelle, Summen, Werkzeugleiste und Filter sofort den neuen Stand.

##### Fehler
- Wenn das Speichern einer Änderung oder das Zusammenführen fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Listen, Einheiten und Equipments bleiben beim vorherigen Stand.

#### Entscheidungen aus dem Plan

- Die Reihenfolge der Aktivitäten wird als Id-Liste in der Tabelle `Einstellung` gespeichert, nicht als neue Spalte: Das Schema hat keinen Migrationsmechanismus, und die Id-Liste übersteht Umbenennen. Fehlt die Einstellung (oder fehlen Ids), gilt die Id-Reihenfolge.
- Das `AuswahllistenViewModel` arbeitet weiter mit Namen (sie sind je Liste eindeutig); die Ids kennt nur das Hauptfenster. Die vielen Operationen werden über die Schnittstelle `IAuswahllistenSpeicher` statt über einzelne `Func`-Parameter übergeben.
- Das Ziel des Zusammenführens wird in einem eigenen kleinen Fenster mit Liste und „OK“/„Abbrechen“ gewählt; danach folgt die Rückfrage als MessageBox.
- „Als Hauptaktivität“ und „Standard“ erscheinen nur bei der jeweiligen Liste, „Zusammenführen mit …“ bei Aktivität und Kategorie, nicht bei Trainingsbereich (dort wirkt weiterhin das Umbenennen mit Rückfrage aus 009).
- Kategorien in Bearbeitungsfenster und Filter bleiben alphabetisch; nur die Aktivitäten folgen der festgelegten Reihenfolge.

### 011 – Equipments verwalten

Abhängig von: `hauptfenster-auswahl`

#### Was und warum
Der Benutzer pflegt seine Räder und seine Ausrüstung (Equipments) in den Einstellungen unter „Auswahllisten“: anlegen, bearbeiten, ausmustern und löschen. Je Aktivität kann ein Equipment Standard sein; es wird bei neuen Einheiten vorbelegt, und seine Standardkategorie wird mit angehakt. Im Bearbeitungsfenster einer Einheit stehen nur die Equipments der passenden Aktivität. Bisher gab es nur die aus BikeXperience übernommenen Equipments, und neue Räder ließen sich nicht anlegen. Gewicht, Radumfang, Widerstandswerte, Kettenblätter, Ritzel und Bild gehören nicht dazu, die Gerätezuordnung gehört zum Garmin-Import.

#### Kriterien

##### Anzeigen
- Wenn der Benutzer in den Einstellungen unter „Auswahllisten“ die Liste wählt, dann findet er auch „Equipment“.
- Wenn die Liste „Equipment“ gewählt ist, dann zeigt sie alle Equipments alphabetisch mit ihrer Aktivität, die ausgemusterten am Ende der Liste, ausgemusterte mit dem Zusatz „(ausgemustert)“ und Standard-Equipments mit dem Zusatz „(Standard)“.

##### Anlegen und bearbeiten
- Wenn der Benutzer bei „Equipment“ „Hinzufügen“ oder für ein markiertes Equipment „Bearbeiten…“ wählt, dann öffnet sich ein Fenster mit Name, Aktivität, Standardkategorie, Anschaffungsdatum, Anfangskilometer, Standard und ausgemustert.
- Wenn der Benutzer im Fenster „OK“ wählt, dann ist das Equipment sofort gespeichert und im Bearbeitungsfenster der Einheiten, in der Tabellenzelle und, sobald Einheiten es verwenden, im Filter wählbar.
- Wenn der Name leer ist oder es ihn schon gibt (ohne Unterschied zwischen Groß- und Kleinschreibung; Leerzeichen am Anfang und Ende werden entfernt), dann wird nicht gespeichert, und ein Hinweis nennt den Grund.
- Wenn das Anschaffungsdatum kein gültiges Datum ist oder die Anfangskilometer keine Zahl von 0 an sind, dann wird nicht gespeichert, und ein Hinweis nennt den Grund.
- Wenn der Benutzer ein Equipment umbenennt, dann zeigen Tabelle, Filter und Auswahllisten sofort den neuen Namen, und die Einheiten behalten ihre Zuordnung.
- Wenn der Benutzer im Fenster „Abbrechen“ wählt, dann bleibt alles unverändert.

##### Standard
- Wenn der Benutzer ein Equipment als Standard kennzeichnet, dann ist es das einzige Standard-Equipment seiner Aktivität; ein bisheriges Standard-Equipment derselben Aktivität verliert die Kennzeichnung.
- Wenn ein Equipment ausgemustert ist, dann ist es nicht Standard: Beim Ausmustern fällt die Kennzeichnung weg, und ein ausgemustertes Equipment lässt sich nicht als Standard kennzeichnen.
- Wenn ein ausgemustertes Equipment aus dem bisherigen Bestand noch als Standard gekennzeichnet ist, dann wird es weder als Standard angezeigt noch vorbelegt.
- Wenn der Benutzer „Neue Einheit“ wählt, dann ist das Standard-Equipment der Aktivität vorbelegt; gibt es keines, dann ist kein Equipment gewählt.

##### Equipment und Einheit
- Wenn der Benutzer im Bearbeitungsfenster einer Einheit das Equipment aufklappt, dann stehen dort nur die nicht ausgemusterten Equipments der gewählten Aktivität, Equipments ohne Aktivität und das Equipment, das die Einheit bereits hat.
- Wenn der Benutzer bei einer neuen Einheit die Aktivität wechselt, dann ist das Standard-Equipment der neuen Aktivität gewählt, bzw. kein Equipment, wenn es keines gibt.
- Wenn der Benutzer bei einer Einheit ein Equipment mit Standardkategorie wählt, dann wird diese Kategorie zusätzlich angehakt; schon angehakte Kategorien bleiben angehakt.
- Wenn ein Equipment ausgemustert ist, dann behalten die Einheiten, die es verwenden, die Zuordnung, und Tabelle und Filter zeigen es weiter an.
- Wenn der Benutzer die Kennzeichnung „ausgemustert“ wieder entfernt, dann wird das Equipment wieder angeboten.

##### Löschen
- Wenn der Benutzer ein Equipment löschen will, das Einheiten verwenden oder zu dem Wartungen erfasst sind, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl der Einheiten bzw. Wartungen und empfiehlt, es stattdessen auszumustern.
- Wenn ein Equipment von keiner Einheit verwendet wird und keine Wartungen hat, dann verschwindet es mit „Löschen“ ohne Rückfrage.

##### Fehler
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Equipments und Einheiten bleiben beim vorherigen Stand.

#### Entscheidungen aus dem Plan

- **Equipment-Fenster:** Ein eigenes Fenster statt Eingabefeld, weil ein Equipment mehrere Felder hat. Bei der Liste „Equipment“ sind Eingabefeld und „Umbenennen“ ausgeblendet; „Bearbeiten…“ und ein Doppelklick auf eine Zeile öffnen das Fenster.
- **Liste in der Tabellenzelle:** Die Auswahl je Aktivität gilt auch für die Equipment-Zelle, weil sie dieselbe Liste wie das Bearbeitungsfenster nutzt.
- **Standard nur mit Aktivität:** „Standard“ ist nur bei einem Equipment mit Aktivität wählbar, weil der Standard je Aktivität gilt.
- **Altbestand bleibt unverändert:** Der Standard von „Namur“ wird nicht bereinigt. Wirksam ist der Standard nur bei nicht ausgemusterten Equipments. Beim nächsten Speichern eines Equipments derselben Aktivität entfernt `UebernimmEquipment` die alte Kennzeichnung.
- **Standardkategorie:** Auswahl aus den vorhandenen Kategorien plus „(keine)“. Gespeichert wird weiter der Name, wie bisher und wie es die Umbenennung aus 010 erwartet.
- **Aktivität:** Auswahl aus den Aktivitäten plus „(keine)“ für Equipments ohne Aktivität.
- **Sortierung:** Zuerst die nicht ausgemusterten, dann die ausgemusterten Equipments, jeweils alphabetisch (Wunsch nach der manuellen Prüfung).

### 012 – Mehrfachauswahl zuordnen

Abhängig von: `einheiten-tabelle`, `aktivitaeten-kategorien-verwalten`, `equipments-verwalten`

#### Was und warum
Der Benutzer markiert eine oder mehrere Einheiten in der Tabelle und ordnet ihnen auf einmal Kategorien zu, wechselt ihre Aktivität oder ändert ihr Equipment. Bisher ging das nur Einheit für Einheit im Bearbeitungsfenster oder in der Tabellenzelle. Viele alte Einheiten lassen sich so in einem Schritt bereinigen, etwa dem richtigen Rad zuordnen.

#### Kriterien

##### Aufruf
- Wenn der Benutzer eine oder mehrere Einheiten in der Tabelle markiert hat, dann kann er im Menü „Einheit“ und im Kontextmenü der Tabelle „Kategorien zuordnen…“, „Aktivität ändern…“ und „Equipment ändern…“ wählen.
- Wenn keine Einheit markiert ist, dann sind diese Befehle nicht wählbar.

##### Kategorien
- Wenn der Benutzer „Kategorien zuordnen…“ wählt, dann zeigt ein Fenster alle Kategorien: angehakt, wenn alle markierten Einheiten sie haben, leer, wenn keine sie hat, und teilweise angehakt, wenn nur manche sie haben.
- Wenn der Benutzer im Fenster eine Kategorie anhakt bzw. leert und „OK“ wählt, dann haben alle markierten Einheiten diese Kategorie bzw. keine von ihnen hat sie mehr; eine Kategorie, die auf „teilweise“ stehen bleibt, ändert sich bei keiner Einheit.

##### Aktivität
- Wenn der Benutzer „Aktivität ändern…“ wählt, eine Aktivität auswählt und „OK“ wählt, dann gehören alle markierten Einheiten zu dieser Aktivität.
- Wenn die Aktivität geändert wird, dann behalten die Einheiten ihr Equipment, auch wenn es nicht zur neuen Aktivität passt.

##### Equipment
- Wenn der Benutzer „Equipment ändern…“ wählt, dann bietet das Fenster „(kein)“ und alle nicht ausgemusterten Equipments an, die zu einer der Aktivitäten der markierten Einheiten passen oder keine Aktivität haben.
- Wenn der Benutzer ein Equipment auswählt und „OK“ wählt, dann haben alle markierten Einheiten dieses Equipment bzw. keines; ihre Kategorien bleiben unverändert.

##### Gemeinsam
- Wenn mehr als eine Einheit markiert ist und der Benutzer „OK“ wählt, dann fragt die App mit der Anzahl nach, z. B. „42 Einheiten ändern?“; bei „Nein“ bleibt alles unverändert.
- Wenn der Benutzer im Fenster „Abbrechen“ wählt, dann bleibt alles unverändert.
- Wenn zugeordnet wurde, dann zeigen Tabelle, Summen und Filter sofort den neuen Stand, und die Einheiten bleiben markiert, soweit sie noch angezeigt werden.
- Wenn geänderte Einheiten danach nicht mehr zur Auswahl der Werkzeugleiste oder zum Filter passen, dann nennt die Statuszeile ihre Anzahl, z. B. „3 geänderte Einheiten liegen außerhalb der aktuellen Auswahl.“

##### Fehler
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und keine der markierten Einheiten ist geändert.

#### Entscheidungen aus dem Plan

- **Fenster für Aktivität und Equipment:** Ein gemeinsames Fenster „Eintrag wählen“ mit Liste statt zweier eigener Fenster; es ähnelt dem Fenster „Zusammenführen“, liefert aber eine Nummer statt eines Namens, weil „(kein)“ keine Nummer hat.
- **Vorauswahl:** Haben alle markierten Einheiten dieselbe Aktivität bzw. dasselbe Equipment, ist es vorgewählt, sonst nichts.
- **„Teilweise“ nur am Anfang:** Ein Häkchen lässt sich nur dann wieder auf „teilweise“ stellen, wenn es anfangs so stand; sonst wechselt es nur zwischen an und aus.
- **Keine Änderung, keine Rückfrage:** Ändert „OK“ im Kategorienfenster nichts, wird ohne Rückfrage und ohne Speichern geschlossen.
- **Gezählt wird, was markiert ist:** Die Rückfrage nennt die Zahl der markierten Einheiten, auch wenn einzelne den neuen Wert schon haben. Geschrieben werden alle markierten.

### 013 – Persönliche Daten

Abhängig von: `hauptfenster-auswahl`

#### Was und warum
Der Benutzer pflegt seine persönlichen Daten in einem eigenen Fenster: Vorname, Nachname, Geburtstag, Größe, Gewicht, Ruhe- und Maximalpuls sowie den Heimatort. Die Werte wurden aus BikeXperience übernommen und lassen sich bisher nicht ansehen oder ändern. Spätere Funktionen greifen darauf zurück, etwa die Streckenkarte auf den Heimatort.

#### Kriterien

##### Anzeigen
- Wenn der Benutzer „Extras → Persönliche Daten…“ wählt, dann zeigt ein Fenster die gespeicherten Werte: Vorname, Nachname, Geburtstag, Größe in cm, Gewicht in kg, Ruhepuls, Maximalpuls und Heimatort (nur der Name des Orts).
- Wenn das Fenster zum ersten Mal geöffnet wird, dann zeigt es die aus BikeXperience übernommenen Werte.
- Wenn noch keine persönlichen Daten vorhanden sind (neuer, leerer Datenordner), dann sind die Felder leer.

##### Ändern
- Wenn der Benutzer Werte ändert und „OK“ wählt, dann sind sie gespeichert und erscheinen auch nach einem Neustart.
- Wenn der Benutzer „Abbrechen“ wählt, dann bleibt alles unverändert.
- Wenn ein Feld leer gelassen wird, dann gilt der Wert als unbekannt und wird so gespeichert.

##### Prüfen
- Wenn der Geburtstag kein gültiges Datum im Format TT.MM.JJJJ ist oder in der Zukunft liegt, dann erscheint ein Hinweis, und „OK“ ist nicht wählbar.
- Wenn die Größe keine ganze Zahl ist oder außerhalb von 100 bis 250 cm liegt, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
- Wenn das Gewicht keine Zahl mit höchstens einer Nachkommastelle ist (z. B. „78,5“) oder außerhalb von 30 bis 250 kg liegt, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
- Wenn der Ruhepuls außerhalb von 30 bis 120 oder der Maximalpuls außerhalb von 100 bis 230 liegt oder keine ganze Zahl ist, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
- Wenn der Ruhepuls nicht kleiner als der Maximalpuls ist, dann erscheint der Hinweis „Der Ruhepuls muss kleiner als der Maximalpuls sein.“, und „OK“ ist nicht wählbar.

##### Fehler
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, das Fenster bleibt offen, und die gespeicherten Werte bleiben unverändert.

#### Entscheidungen aus dem Plan

- **Ein Hinweis für alle Felder:** Wie im Fenster „Equipment“ nennt eine rote Zeile unter den Feldern den ersten Fehler. Das ist einheitlich und reicht bei acht Feldern.
- **Unbekannt bleibt 0:** Die Datenbank kennt bei Zahlen schon „0 = unbekannt“ (übernommene Altdaten, leerer Standardsportler). Es gibt daher keine Schemaänderung.
- **Geburtstag großzügig lesen:** Wie in allen Datumsfeldern wird auch „28.5.63“ angenommen; angezeigt wird immer TT.MM.JJJJ.
- **Koordinaten bleiben:** Ändert sich der Name des Heimatorts, bleiben die gespeicherten Koordinaten unverändert. Ihre Pflege gehört zu `streckenkarte`.

### 014 – Saisonziele

Abhängig von: `aktivitaeten-kategorien-verwalten`

#### Was und warum
Der Benutzer legt je Aktivität und Jahr Saisonziele für Kilometer, Höhenmeter und Anzahl der Einheiten fest. Die Ansicht „Aktuell“ vergleicht später den Jahresfortschritt mit diesen Zielen. Ein Ziel gilt so lange weiter, bis es für ein späteres Jahr geändert wird; so behalten vergangene Jahre ihre damaligen Ziele. Gepflegt werden nur das laufende und das folgende Jahr.

#### Kriterien

##### Anzeigen
- Wenn der Benutzer „Extras → Saisonziele…“ wählt, dann zeigt ein Fenster für das gewählte Jahr alle Aktivitäten mit ihren Zielen für Kilometer, Höhenmeter und Anzahl; vorgewählt ist das laufende Jahr.
- Wenn der Benutzer im Fenster das Jahr wählt, dann stehen nur das laufende und das folgende Jahr zur Wahl, und das Fenster zeigt die Ziele des gewählten Jahres.
- Wenn für ein Jahr kein eigenes Ziel eingetragen ist, dann gilt das Ziel des nächstfrüheren Jahres, und das Fenster kennzeichnet es als „übernommen aus <Jahr>“ bzw. bei einem Ziel aus BikeXperience als „übernommen aus BikeXperience“.
- Wenn das Fenster zum ersten Mal geöffnet wird, dann zeigt es die aus BikeXperience übernommenen Ziele; sie gelten für alle Jahre bis zur ersten Änderung.
- Wenn eine Aktivität kein Ziel hat, dann sind ihre Felder leer.

##### Ändern
- Wenn der Benutzer Ziele ändert und „OK“ wählt, dann gelten sie ab dem gewählten Jahr; frühere Jahre bleiben unverändert, ein späteres Jahr ohne eigenes Ziel übernimmt sie, und nach einem Neustart sind sie noch da.
- Wenn der Benutzer ein Feld leert oder 0 einträgt, dann hat die Aktivität ab diesem Jahr dafür kein Ziel.
- Wenn der Benutzer „Abbrechen“ wählt, dann bleibt alles unverändert.
- Wenn eine Aktivität neu angelegt wird, dann hat sie zunächst keine Ziele; wird sie gelöscht, verschwinden ihre Ziele mit ihr.

##### Prüfen
- Wenn ein Wert keine ganze Zahl ist oder außerhalb der Grenzen liegt (Kilometer 0 bis 100.000, Höhenmeter 0 bis 2.000.000, Anzahl 0 bis 1.000), dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.

##### Fehler
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, das Fenster bleibt offen, und die gespeicherten Ziele bleiben unverändert.

#### Entscheidungen aus dem Plan

- **Jahreswechsel (E1):** Änderungen bleiben beim Wechsel zwischen den beiden Jahren erhalten, und „OK“ speichert beide gemeinsam. Gewechselt werden kann nur, solange alle Eingaben gültig sind.
- **Kennzeichnung (F1):** Ein Ziel direkt aus BikeXperience heißt „übernommen aus BikeXperience“.
- **Eigenes Ziel erst bei Änderung:** Eine Zeile wird erst dann zum eigenen Ziel des Jahres, wenn eines ihrer Felder geändert wurde. Ein schon vorhandenes eigenes Ziel bleibt eigenes Ziel. „OK“ ohne Änderung legt nichts an.
- **Grundziel bleibt in `Aktivitaet`:** Die Ziele werden nicht in die neue Tabelle umgezogen. Das spart eine Datenumwandlung, und ältere Sicherungen bleiben brauchbar.
- **Zusammenführen:** Die Ziele der Ursprungsaktivität verschwinden; die Ziele der verbleibenden Aktivität gelten.

### 015 – Jahresfortschritt

Abhängig von: `saisonziele`

#### Was und warum
Die Karteikarte „Aktuell“ zeigt in einem Liniendiagramm die Jahressummen aller erfassten Jahre für Kilometer, Anzahl Einheiten, Höhenmeter und Fahrzeit, dazu die Saisonziele. Über dem Diagramm stehen die Kennzahlen des laufenden Jahres: Stand gegenüber Ziel und Soll-Stand sowie die nötige Wochenleistung bis Saisonende. So sieht der Benutzer auf einen Blick, wie das laufende Jahr im Vergleich zu früheren Jahren und zum Ziel steht. Das Diagramm deckt auch den Map-Eintrag `mehrjahresdiagramm` ab.

#### Kriterien

##### Diagramm
- Wenn der Benutzer die Karteikarte „Aktuell“ öffnet, dann zeigt sie ein Liniendiagramm mit ausgefüllter Fläche unter der Linie: waagrecht alle Jahre mit erfassten Einheiten (das laufende Jahr ganz rechts), senkrecht die Jahressumme des gezeigten Werts, mit einem Punkt je Jahr.
- Wenn der Benutzer in das Diagramm klickt, dann wechselt es von Kilometern (Summe Entfernung) zu Anzahl Einheiten, dann zu Höhenmetern (Summe Höhenmeter), dann zu Fahrzeit und wieder zu Kilometern; die Überschrift nennt den gezeigten Wert.
- Wenn Jahre ein Saisonziel für den gezeigten Wert haben, dann zeigt das Diagramm es als gestrichelte Ziellinie über diesen Jahren, und im laufenden Jahr ist zusätzlich der Soll-Stand zum heutigen Tag markiert.

##### Auswahl
- Wenn in der Werkzeugleiste eine Aktivität gewählt ist, dann enthält das Diagramm nur deren Einheiten und Ziele; bei „Alle Aktivitäten“ werden Einheiten und Ziele aller Aktivitäten zusammengezählt.
- Wenn in der Filterleiste eine Kategorie gewählt ist, dann enthält das Diagramm nur Einheiten dieser Kategorie; die übrigen Filter und die Jahresauswahl der Werkzeugleiste ändern das Diagramm nicht.

##### Kennzahlen des laufenden Jahres
- Wenn für den gezeigten Wert ein Ziel im laufenden Jahr besteht, dann stehen über dem Diagramm Stand, Ziel, Anteil in Prozent, Soll-Stand zum heutigen Tag (Ziel anteilig zu den verstrichenen Tagen des Jahres) und Vorsprung bzw. Rückstand.
- Wenn das Ziel noch nicht erreicht ist, dann nennt die Karteikarte die nötige Wochenleistung bis zum 31. Dezember, z. B. „noch 2.345 km – 112,4 km pro Woche“; die verbleibenden Wochen sind die restlichen Tage einschließlich heute geteilt durch 7.
- Wenn das Ziel erreicht ist, dann steht dort „Ziel erreicht“ mit dem Überschuss.
- Wenn für Kilometer, Anzahl oder Höhenmeter kein Ziel besteht, dann stehen nur der Stand und der Hinweis „kein Saisonziel – unter Extras → Saisonziele… festlegen“; für die Fahrzeit gibt es kein Saisonziel, dort steht nur der Stand.

##### Aktualität und Leerzustand
- Wenn Einheiten angelegt, geändert oder gelöscht werden oder sich die Saisonziele, die Aktivität der Werkzeugleiste oder die Kategorie der Filterleiste ändern, dann zeigt die Karteikarte sofort den neuen Stand.
- Wenn die Auswahl keine Einheiten enthält, dann erscheint statt des Diagramms der Hinweis „Keine Einheiten in dieser Auswahl.“

#### Entscheidungen aus dem Plan

- **Laufendes Jahr immer im Diagramm:** Gibt es überhaupt Einheiten in der Auswahl, steht das laufende Jahr ganz rechts, auch mit Summe 0 (z. B. am 1. Januar). Sonst fehlten die Kennzahlen und der Soll-Stand.
- **Gezeigter Wert bleibt bis zum Programmende:** Nach einer Änderung der Auswahl bleibt der gewählte Wert (z. B. hm) stehen. Er wird nicht gespeichert; beim Start ist es Kilometer.
- **Kein Hinweis am Punkt:** Auf Wunsch des Benutzers entfallen; die Werte des laufenden Jahres stehen in den Kennzahlen.
- **Rundung:** Kilometer und Wochenleistung werden mit einer Nachkommastelle angezeigt, Höhenmeter und Anzahl ganzzahlig, Prozent ganzzahlig, Fahrzeit als Stunden:Minuten.

### 016 – Bestleistungen

Abhängig von: `hauptfenster-auswahl`

#### Was und warum
Die Karteikarte „Aktuell“ zeigt über dem Diagramm die Bestleistungen, wie die Übersicht „Aktuell“ in BikeXperience: die Jahresrekorde mit ihrem Rekordjahr und die Einzelrekorde mit der Einheit, in der sie aufgestellt wurden. So sieht der Benutzer auf einen Blick, was seine besten Jahre und Touren waren und ob das laufende Jahr einen Rekord hält.

#### Kriterien

##### Anzeige
- Wenn der Benutzer die Karteikarte „Aktuell“ öffnet, dann steht über dem Diagramm eine Liste der Bestleistungen.
- Wenn es Jahresrekorde gibt, dann nennt die Liste für Kilometer, Höhenmeter, Anzahl Einheiten und Fahrzeit jeweils den höchsten Jahreswert mit seinem Jahr, z. B. „Kilometer: 8.123,4 km (2019)“.
- Wenn es Einzelrekorde gibt, dann nennt die Liste die längste Tour, die meisten Höhenmeter, die längste Fahrzeit, die höchste Ø-Geschwindigkeit, die höchste Höchstgeschwindigkeit und den höchsten Punkt, jeweils mit Wert, Datum und Name der Einheit.
- Wenn das laufende Jahr einen Rekord hält, dann ist dieser hervorgehoben.
- Wenn der Benutzer auf einen Einzelrekord doppelklickt, dann öffnet sich die zugehörige Einheit zum Bearbeiten.

##### Auswahl
- Wenn in der Werkzeugleiste eine Aktivität oder in der Filterleiste eine Kategorie gewählt ist, dann gelten die Bestleistungen nur für diese Einheiten, wie beim Diagramm; die Jahresauswahl und die übrigen Filter ändern die Bestleistungen nicht.

##### Grenzfälle
- Wenn mehrere Jahre oder Einheiten denselben Rekordwert haben, dann hält der frühere den Rekord.
- Wenn bei einer Einheit ein Wert fehlt (z. B. keine Höchstgeschwindigkeit erfasst), dann zählt er nicht als Rekord; hat keine Einheit diesen Wert, entfällt die Zeile.
- Wenn die Auswahl keine Einheiten enthält, dann entfällt die Liste.
- Wenn Einheiten angelegt, geändert oder gelöscht werden oder sich die Aktivität der Werkzeugleiste oder die Kategorie der Filterleiste ändert, dann zeigt die Liste sofort den neuen Stand.

#### Entscheidungen aus dem Plan

- **Anordnung:** Die Bestleistungen stehen ganz oben; Überschrift und Kennzahlen bleiben direkt beim Diagramm, weil sie sich mit dem Klick ins Diagramm ändern.
- **Hervorhebung:** Zeilen mit einem Rekord des laufenden Jahres werden fett in der Linienfarbe des Diagramms gezeigt. Rot ist in der Tabelle schon für die Bestwerte der Auswahl vergeben.
- **Einzelrekorde:** Die Formate entsprechen der Tabelle, damit die Werte wiedererkennbar sind.
- **Gemeinsame Auswahl:** Die Auswahl steht in `FiltereUebersicht`, damit Diagramm und Bestleistungen sicher dieselben Einheiten verwenden.

### 017 – Garmin-Import

Abhängig von: `einheit-bearbeiten`

#### Was und warum
Neue Touren werden direkt vom angeschlossenen Garmin-Radcomputer übernommen: vom Edge 1040, der sich nicht wie ein USB-Laufwerk auslesen lässt, ebenso wie von den älteren Edge 1000 und Edge 800, die als Laufwerk erscheinen. AndyTours zeigt die Touren, die noch fehlen, legt für die gewählten Touren Einheiten mit allen Werten der Aufzeichnung an und legt die Aufzeichnung dazu ab. Das Rad erkennt AndyTours an der Rad-Kennung, die der Edge nach der Tour abfragt und in der Aufzeichnung speichert, ersatzweise an den Sensoren am Rad; die Zuordnung lernt AndyTours beim Import selbst. Als Ausweg ohne Gerät lassen sich Aufzeichnungsdateien auch von der Festplatte wählen. Damit entfällt das bisherige Übertragen über Umwege, und BikeXperience wird für den Import nicht mehr gebraucht.

#### Kriterien

##### Import vom Gerät
- Wenn der Benutzer „Vom Garmin importieren…“ wählt und ein Edge 1040, Edge 1000 oder Edge 800 per USB angeschlossen ist, dann zeigt ein Fenster alle Touren des Geräts, die noch nicht in AndyTours sind, mit Datum, Startzeit, Distanz, Fahrzeit, Profil und Equipment; alle sind angehakt und lassen sich abwählen.
- Wenn mehrere Garmin-Geräte gleichzeitig angeschlossen sind, dann zeigt die Liste die neuen Touren aller Geräte, jeweils mit dem Namen des Geräts.
- Wenn der Benutzer auf „Importieren“ klickt, dann wird für jede angehakte Tour eine Einheit angelegt; Datum, Startzeit, Fahrzeit, Distanz, Höhenmeter, Höhen, Geschwindigkeiten, Puls, Trittfrequenz, Temperatur und Leistung kommen aus der Aufzeichnung, der Name bleibt leer.
- Wenn eine Einheit importiert wird, dann ist ihre Fahrzeit die gestoppte Zeit ohne Pausen, wie sie der Edge als „Zeit“ anzeigt.
- Wenn eine Tour importiert wird, dann wird ihre Aufzeichnung bei der Einheit abgelegt, so wie bei den aus BikeXperience übernommenen Touren.
- Wenn der Import fertig ist, dann sind die neuen Einheiten in der Tabelle markiert, und die Statusleiste meldet die Anzahl, z. B. „3 Touren importiert.“

##### Import aus Dateien
- Wenn der Benutzer „FIT-Dateien importieren…“ wählt und eine oder mehrere Aufzeichnungsdateien aussucht, dann erscheint dieselbe Liste wie beim Gerät, mit den Touren aus diesen Dateien, die noch nicht in AndyTours sind.

##### Gerätezuordnung
- Wenn die Aufzeichnung eine Rad-Kennung enthält, die bei einem Equipment gemerkt ist, dann steht in der Liste dieses Equipment.
- Wenn die Rad-Kennung fehlt oder bei keinem Equipment gemerkt ist, aber ein Geschwindigkeits- oder Trittfrequenzsensor der Aufzeichnung bei einem Equipment gemerkt ist, dann steht in der Liste dieses Equipment.
- Wenn weder Rad-Kennung noch Sensor passen, dann steht in der Liste das Standard-Equipment der Hauptaktivität.
- Wenn der Benutzer in der Liste für eine Tour ein anderes Equipment wählt, dann erhält die Einheit dieses Equipment.
- Wenn eine Einheit importiert wird, dann erhält sie die Aktivität ihres Equipments (ohne Aktivität: die Hauptaktivität), die Standardkategorien und die Standardkategorie des Equipments.
- Wenn eine Tour importiert wird, dann merkt sich AndyTours ihre Rad-Kennung und ihre Geschwindigkeits- und Trittfrequenzsensoren bei dem Equipment der Einheit; waren sie bei einem anderen Equipment gemerkt, werden sie dort entfernt.
- Wenn der Benutzer ein Equipment bearbeitet, dann sieht er dort die gemerkten Rad-Kennungen und Sensoren und kann einzelne entfernen.

##### Grenz- und Fehlerfälle
- Wenn eine Tour mit gleichem Datum und gleicher Startzeit schon in AndyTours ist, dann erscheint sie nicht in der Liste.
- Wenn kein Garmin-Gerät angeschlossen ist, dann erscheint der Hinweis „Kein Garmin-Gerät gefunden. Bitte den Edge per USB anschließen.“
- Wenn alle Touren schon in AndyTours sind, dann erscheint der Hinweis „Keine neuen Touren gefunden.“
- Wenn eine Aufzeichnung nicht gelesen werden kann, dann wird sie übersprungen und in der Meldung am Ende genannt; die übrigen Touren werden trotzdem importiert.

#### Entscheidungen aus dem Plan

- **Gerätezuordnung:** Rad-Kennung vor Sensor vor Standard-Equipment; gelernt wird bei jedem Import (Wahl B3 des Benutzers). Als Sensoren zählen nur Geschwindigkeits- und Trittfrequenzsensoren; Brustgurt und Leistungsmesser zählen nicht.
- **Speicherort der Kennungen:** Sie stehen im vorhandenen Textfeld `Equipment.Geraetezuordnung` (z. B. `Rad:7af22db6-…; Sensor:3452708077`), daher keine Schemaänderung. Altwerte wie „Bike1“ bleiben als Altwert sichtbar und lassen sich entfernen.
- **Gerätename:** Er stammt aus der Aufzeichnung (`FileId.Product`, z. B. „Edge 1040“). Damit gilt er gleichermaßen für Gerät und Datei.
- **Startzeit:** Sie wird sekundengenau gespeichert. Für die Duplikatprüfung wird auf die Minute verglichen, weil die übernommenen Altdaten nur „hh:mm“ haben.
- **Menü:** Beide Befehle stehen unter „Datei“.

### 018 – Höhenprofil

Abhängig von: `einheiten-tabelle`

#### Was und warum
Zu einer Trainingseinheit lässt sich der Verlauf der Tour ansehen: das Höhenprofil aus der Aufzeichnung, auf Wunsch zusammen mit Puls, Geschwindigkeit, Trittfrequenz und Temperatur. Es erscheint in einem eigenen Fenster „Tourdetail“, in das später auch die Strecke auf der Karte kommt. So lässt sich eine Tour wie in BikeXperience nachvollziehen, ohne ein anderes Programm zu öffnen.

#### Kriterien

##### Anzeigen
- Wenn genau eine Einheit mit Aufzeichnung markiert ist und der Benutzer „Einheit → Tourdetail…“ oder im Kontextmenü der Tabelle „Tourdetail…“ wählt, dann öffnet sich das Fenster „Tourdetail“ mit der Karteikarte „Höhenprofil“.
- Wenn das Fenster offen ist, dann stehen oben Datum, Name, Distanz, Höhenmeter und höchster Punkt der Einheit.
- Wenn das Höhenprofil angezeigt wird, dann zeigt die waagrechte Achse die Distanz in km und die senkrechte Achse die Höhe in m.
- Wenn der Benutzer die Achse auf „Fahrzeit“ umschaltet, dann zeigt die waagrechte Achse die Fahrzeit; Pausen werden dabei herausgelassen.
- Wenn der Benutzer bei Puls, Geschwindigkeit, Trittfrequenz oder Temperatur ein Häkchen setzt, dann erscheint dieser Wert als zusätzliche Linie mit eigener Skala; ohne Häkchen verschwindet sie wieder.
- Wenn die Aufzeichnung einen dieser Werte nicht enthält, dann ist sein Häkchen nicht wählbar.
- Wenn der Benutzer mit der Maus über das Profil fährt, dann erscheinen an dieser Stelle Distanz, Fahrzeit, Höhe und die Werte der eingeblendeten Linien.
- Wenn der Benutzer mit der Maus einen Bereich aufzieht, dann wird dieser Bereich vergrößert; ein Doppelklick zeigt wieder die ganze Tour.
- Wenn der Benutzer doppelt auf eine Zeile der Tabelle klickt, dann öffnet sich wie bisher das Bearbeitungsfenster.

##### Grenz- und Fehlerfälle
- Wenn keine oder mehrere Einheiten markiert sind, dann ist „Tourdetail…“ nicht wählbar.
- Wenn die markierte Einheit keine Aufzeichnung hat, dann ist „Tourdetail…“ nicht wählbar.
- Wenn die Datei der Aufzeichnung fehlt, dann erscheint der Hinweis „Die Aufzeichnung ‚2019/2019-11-11-06-25-39.fit‘ wurde im Datenordner nicht gefunden.“ mit dem Namen der fehlenden Datei.
- Wenn die Aufzeichnung keine Höhendaten enthält (z. B. eine Fahrt auf der Rolle), dann erscheint in der Karteikarte „Die Aufzeichnung enthält keine Höhendaten.“
- Wenn die Aufzeichnung nicht gelesen werden kann, dann erscheint der Hinweis „Die Aufzeichnung konnte nicht gelesen werden.“ mit dem Grund.

#### Entscheidungen aus dem Plan

- Das Fenster ist nicht modal (Wahl G1 A1 des Benutzers): Das Hauptfenster bleibt bedienbar, mehrere Tourdetails können offen sein. Die Anzeige ist der Stand beim Öffnen.
- Pausen kommen aus den Start- und Stopp-Ereignissen der Zeitmessung, wie die gestoppte Zeit im Garmin-Import (017). So endet die Fahrzeit-Achse bei der Fahrzeit der Einheit.
- Messpunkte ohne Distanz übernehmen die letzte bekannte Distanz; Messpunkte ohne Zeitstempel werden übergangen.
- Der Verlauf wird beim Öffnen gelesen und nicht gespeichert; die FIT-Datei bleibt die einzige Quelle.

### 019 – Streckenkarte

Abhängig von: `einheiten-tabelle`

#### Was und warum
Im Tourdetail einer Einheit lässt sich die gefahrene Strecke auf einer Landkarte ansehen. Das Tourdetail bekommt dafür neben „Höhenprofil“ die Karteikarte „Karte“. Wahlweise erscheint die OpenStreetMap-Standardkarte oder OpenTopoMap mit Höhenlinien und Wegen. So lässt sich wie in BikeXperience nachvollziehen, wo eine Tour entlangführte. Bereits angesehene Gegenden bleiben auch ohne Internet sichtbar.

#### Kriterien

##### Anzeigen
- Wenn das Tourdetail einer Einheit geöffnet ist, dann gibt es neben „Höhenprofil“ die Karteikarte „Karte“.
- Wenn der Benutzer die Karteikarte „Karte“ wählt, dann zeigt sie die Strecke der Aufzeichnung als einfarbige Linie auf einer Karte, so vergrößert, dass die ganze Strecke zu sehen ist.
- Wenn die Strecke angezeigt wird, dann sind Start und Ziel jeweils mit einem eigenen Zeichen markiert.
- Wenn der Benutzer zwischen „OpenStreetMap“ und „OpenTopoMap“ umschaltet, dann erscheint die gewählte Karte mit derselben Strecke und demselben Ausschnitt.
- Wenn der Benutzer das Mausrad dreht, dann wird die Karte an der Mausposition vergrößert oder verkleinert; Ziehen mit gedrückter linker Maustaste verschiebt sie.
- Wenn der Benutzer auf „Ganze Strecke“ klickt, dann zeigt die Karte wieder die ganze Strecke.
- Wenn die Karte angezeigt wird, dann steht in einer Ecke der Hinweis „© OpenStreetMap-Mitwirkende“, bei OpenTopoMap ergänzt um „Kartendarstellung: © OpenTopoMap (CC-BY-SA)“.

##### Ohne Internet
- Wenn ein Kartenausschnitt einmal geladen wurde, dann erscheint er später auch ohne Internetverbindung.
- Wenn keine Internetverbindung besteht und ein Ausschnitt noch nie geladen wurde, dann wird die Strecke trotzdem gezeichnet, und es erscheint der Hinweis „Die Karte konnte nicht geladen werden. Besteht eine Internetverbindung?“

##### Grenz- und Fehlerfälle
- Wenn die Aufzeichnung keine GPS-Daten enthält (z. B. eine Fahrt auf der Rolle), dann erscheint in der Karteikarte „Die Aufzeichnung enthält keine GPS-Daten.“
- Wenn einzelne Messpunkte keine Position haben (z. B. im Tunnel), dann verbindet die Linie die Punkte davor und danach.

#### Entscheidungen aus dem Plan

- Die Positionen kommen aus dem Verlauf aus 018. Die Aufzeichnung wird beim Öffnen des Tourdetails nur einmal gelesen und für beide Karteikarten verwendet.
- Der Kartenspeicher liegt unter `%LOCALAPPDATA%\AndyTours\Karten` und nicht im Datenordner, damit Kartenausschnitte weder in die Sicherung noch auf ein NAS kommen.
- Der Lizenzhinweis steht im ViewModel je Kartenart, nicht in der Bibliothek. So ist er unabhängig davon, ob die Bibliothek selbst einen anzeigt.

### 020 – Mehrtagestour

Abhängig von: `hoehenprofil`, `streckenkarte`

#### Was und warum
Mehrere Einheiten, z. B. die Etappen einer Radreise, lassen sich gemeinsam im Tourdetail ansehen: oben die Summen, ein durchgehendes Höhenprofil über alle Etappen und eine Karte mit der ganzen Reise. Die Mehrtagestour entsteht jedes Mal aus den markierten Einheiten und wird nicht gespeichert. Die Summen stammen aus den Werten der Einheiten; sie stehen deshalb auch für Etappen ohne Aufzeichnung zur Verfügung, etwa wenn unterwegs der Radcomputer ausfällt. So lässt sich eine Reise wie mit der Mehrtagestour in BikeXperience als Ganzes nachvollziehen.

#### Kriterien

##### Anzeigen
- Wenn mehrere Einheiten markiert sind und der Benutzer „Einheit → Tourdetail…“ oder im Kontextmenü der Tabelle „Tourdetail…“ wählt, dann öffnet sich das Tourdetail für alle markierten Einheiten gemeinsam.
- Wenn das Tourdetail mehrerer Einheiten geöffnet wird, dann sind die Etappen nach Datum und Startzeit geordnet, unabhängig von der Reihenfolge, in der sie markiert wurden.
- Wenn das Tourdetail mehrerer Einheiten offen ist, dann stehen oben der Zeitraum vom ersten bis zum letzten Datum, die Anzahl der Etappen, die Summen von Distanz, Höhenmetern und Fahrzeit, die Durchschnittsgeschwindigkeit über alle Etappen und der höchste Punkt.
- Wenn das Höhenprofil angezeigt wird, dann folgen die Etappen hintereinander, die Distanz zählt über die Etappen weiter, und jeder Wechsel zur nächsten Etappe ist mit einer senkrechten Linie markiert.
- Wenn der Benutzer die Achse auf „Fahrzeit“ umschaltet, dann zählt die Fahrzeit über die Etappen weiter; Nächte und Pausen zwischen den Etappen werden herausgelassen.
- Wenn der Benutzer mit der Maus über das Profil fährt, dann erscheinen zusätzlich zu den bisherigen Werten Datum und Name der Etappe an dieser Stelle.
- Wenn der Benutzer die Karteikarte „Karte“ wählt, dann zeigt sie alle Etappen, so vergrößert, dass die ganze Reise zu sehen ist.
- Wenn die Karte mehrere Etappen zeigt, dann hat jede Etappe eine eigene Farbe, wobei benachbarte Etappen sich in der Farbe unterscheiden.
- Wenn die Karte mehrere Etappen zeigt, dann sind der Start der ersten Etappe mit dem Startzeichen, das Ziel der letzten Etappe mit dem Zielzeichen und jeder Etappenwechsel mit einem kleinen Punkt markiert.
- Wenn der Benutzer im Tourdetail mehrerer Einheiten Häkchen setzt, vergrößert, die Kartenart wechselt, „Ganze Strecke“ wählt oder ohne Internet arbeitet, dann verhält es sich wie das Tourdetail einer einzelnen Einheit.

##### Grenz- und Fehlerfälle
- Wenn genau eine Einheit markiert ist, dann öffnet „Tourdetail…“ wie bisher das Tourdetail dieser Einheit.
- Wenn mehrere Einheiten markiert sind, dann ist „Tourdetail…“ wählbar, auch wenn keine davon eine Aufzeichnung hat.
- Wenn keine der markierten Einheiten eine Aufzeichnung hat, dann zeigt das Tourdetail oben die Summen, und in den Karteikarten „Höhenprofil“ und „Karte“ erscheint der Hinweis „Keine der Einheiten hat eine Aufzeichnung.“
- Wenn unter den markierten Einheiten welche ohne Aufzeichnung sind, dann zählen sie zu den Summen oben, fehlen aber in Profil und Karte, und ein Hinweis nennt sie mit Datum und Name.
- Wenn die Aufzeichnung einer Etappe im Datenordner fehlt oder nicht gelesen werden kann, dann erscheint derselbe Hinweis wie beim Tourdetail einer einzelnen Einheit mit dem Namen der Datei.
- Wenn nur einige Etappen Höhendaten bzw. GPS-Daten enthalten, dann zeigen Profil bzw. Karte die übrigen Etappen, und ein Hinweis nennt die Etappen ohne diese Daten.
- Wenn keine der Etappen Höhendaten bzw. GPS-Daten enthält, dann erscheint in der Karteikarte der Hinweis „Die Aufzeichnung enthält keine Höhendaten.“ bzw. „Die Aufzeichnung enthält keine GPS-Daten.“

#### Entscheidungen aus dem Plan

- Eine fehlende oder unlesbare Aufzeichnung bricht die Mehrtagestour nicht ab: Nach der Meldung öffnet sich das Tourdetail, und die Etappe zählt wie eine ohne Aufzeichnung (Summen ja, Profil und Karte nein, im Hinweis genannt). Mehrere betroffene Dateien stehen in einer gemeinsamen Meldung.
- Der Verlauf wird aus den Aufzeichnungen verbunden, nicht aus den Werten der Einheiten: Die Distanz der nächsten Etappe beginnt bei der letzten Distanz der vorigen Aufzeichnung. Die Kopfzeile dagegen summiert die Werte der Einheiten, wie es die Spec verlangt.
- Eine Etappe mit Aufzeichnung, aber ohne Höhendaten, nimmt im Profil trotzdem ihren Platz auf der Achse ein. So passen Distanz und Fahrzeit zur Karte und zur Summe der übrigen Etappen.
- Ohne Startzeit steht eine Einheit am Anfang ihres Tages; bei gleichem Datum und gleicher Startzeit entscheidet die Nummer der Einheit (Reihenfolge der Anlage), weil die Markierung keine verlässliche Reihenfolge liefert.
- Die Kopfzeile der Mehrtagestour hat ein eigenes Format; die einer einzelnen Einheit bleibt unverändert.
- Das Fenster heißt weiter „Tourdetail“. Das Kriterium aus 018 „Wenn keine oder mehrere Einheiten markiert sind, dann ist ‚Tourdetail…‘ nicht wählbar.“ gilt nur noch für keine markierte Einheit; 020 ersetzt es für mehrere.

- Nachträglich: Die Testklassen, die FIT-Dateien schreiben und lesen, laufen in einer gemeinsamen xUnit-Testgruppe nacheinander. Laufen sie gleichzeitig, ist gelegentlich eine Testdatei unlesbar (schon vor 020 in etwa jedem dritten Lauf). Die App liest Aufzeichnungen nacheinander und ist nicht betroffen.

### 021 – Wartung erfassen

Abhängig von: `equipments-verwalten`

#### Was und warum
Die Karteikarte „Wartung“ zeigt alle Wartungen der Räder. Der Benutzer erfasst, ändert und löscht dort Wartungen mit Datum, Rad, Tätigkeit und Kommentar. Zu jeder Wartung steht der Kilometerstand des Rads an diesem Tag. Die Tätigkeiten pflegt er in den Einstellungen als eigene Auswahlliste, wie die Trainingsbereiche. So bleibt wie in BikeXperience nachvollziehbar, wann an welchem Rad was gemacht wurde. Alarme, die Haltbarkeit der Teile und der CSV-Austausch sind eigene Funktionen.

#### Kriterien

##### Anzeigen
- Wenn der Benutzer die Karteikarte „Wartung“ wählt, dann zeigt sie eine Tabelle aller Wartungen mit Datum, Rad, km-Stand, Tätigkeit und Kommentar, die neueste oben.
- Wenn eine Wartung angezeigt wird, dann steht unter „km-Stand“ die Summe der Distanzen aller Einheiten mit diesem Rad bis einschließlich zum Tag der Wartung, zuzüglich der Anfangskilometer des Rads.
- Wenn der Benutzer oben ein Rad wählt, dann zeigt die Tabelle nur dessen Wartungen; mit „Alle Räder“ erscheinen wieder alle.
- Wenn der Benutzer auf einen Spaltenkopf klickt, dann wird nach dieser Spalte sortiert; ein zweiter Klick kehrt die Reihenfolge um.
- Wenn es noch keine Wartung gibt, dann steht statt der Tabelle „Noch keine Wartungen erfasst.“

##### Erfassen, ändern, löschen
- Wenn der Benutzer über der Tabelle oder im Kontextmenü der Tabelle „Neue Wartung…“ wählt, dann öffnet sich ein Fenster mit Datum (vorbelegt mit heute), Rad, Tätigkeit und Kommentar.
- Wenn oben ein Rad gewählt ist und der Benutzer eine neue Wartung anlegt, dann ist dieses Rad vorgewählt.
- Wenn der Benutzer im Wartungsfenster die Tätigkeit wählt, dann bietet die Auswahl die Einträge der Liste „Tätigkeit“ nach ihrer bisherigen Häufigkeit in den Wartungen aller Räder an, die häufigste zuerst, bei Gleichstand in der Reihenfolge der Liste; andere Tätigkeiten lassen sich dort nicht eingeben.
- Wenn der Benutzer mit „OK“ bestätigt, dann erscheint die Wartung sofort in der Tabelle und bleibt nach einem Neustart erhalten.
- Wenn der Benutzer doppelt auf eine Wartung klickt oder „Bearbeiten…“ wählt, dann öffnet sich dasselbe Fenster mit ihren Werten; Änderungen erscheinen nach „OK“ in der Tabelle, „Abbrechen“ verwirft sie.
- Wenn der Benutzer eine oder mehrere Wartungen markiert und „Löschen“ wählt oder Entf drückt, dann erscheint eine Rückfrage mit der Anzahl, und erst nach „Ja“ werden sie gelöscht.

##### Liste der Tätigkeiten
- Wenn der Benutzer in den Einstellungen unter „Auswahllisten“ die Liste „Tätigkeit“ wählt, dann sieht er deren Einträge in der festgelegten Reihenfolge.
- Wenn die App zum ersten Mal mit dieser Funktion gestartet wird, dann enthält die Liste „Tätigkeit“ alle Tätigkeiten der vorhandenen Wartungen, alphabetisch geordnet.
- Wenn der Benutzer in der Liste „Tätigkeit“ einen Eintrag hinzufügt, verschiebt oder einen leeren oder doppelten Eintrag erfassen will, dann verhält sie sich wie die Liste „Trainingsbereich“.
- Wenn der Benutzer eine Tätigkeit umbenennt, die in Wartungen vorkommt, dann fragt die App mit der Anzahl („In 32 Wartungen ‚Kettenwechsel‘ durch ‚Kette gewechselt‘ ersetzen?“) und ändert nach „Ja“ die Liste und alle diese Wartungen; bei „Nein“ bleibt alles unverändert.
- Wenn der Benutzer eine Tätigkeit löschen will, die in Wartungen vorkommt, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl dieser Wartungen.
- Wenn die Liste „Tätigkeit“ geändert wurde, dann bieten danach geöffnete Wartungsfenster den neuen Stand an, und umbenannte Tätigkeiten erscheinen sofort in der Tabelle.

##### Grenz- und Fehlerfälle
- Wenn im Wartungsfenster kein Rad oder keine Tätigkeit gewählt ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
- Wenn das Datum ungültig ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
- Wenn das Datum in der Zukunft liegt, dann ist „OK“ nicht möglich, und es erscheint „Das Datum liegt in der Zukunft.“
- Wenn die Liste „Tätigkeit“ leer ist, dann weist das Wartungsfenster darauf hin, dass zuerst in den Einstellungen Tätigkeiten angelegt werden müssen.
- Wenn ein Rad ausgemustert ist, dann wird es für neue Wartungen nicht angeboten; seine Wartungen bleiben sichtbar und lassen sich weiter bearbeiten.
- Wenn keine oder mehrere Wartungen markiert sind, dann ist „Bearbeiten…“ nicht wählbar; ohne Markierung ist auch „Löschen“ nicht wählbar.
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Tabelle und Liste bleiben beim vorherigen Stand.

#### Entscheidungen aus dem Plan

- Die Tabelle zeigt nur Einträge vom Typ „Wartung“; Alarme bleiben unberührt, bis `wartungsalarme` sie anzeigt.
- Die Zeilen werden nach jeder Änderung an Einheiten, Rädern, Wartungen oder der Liste „Tätigkeit“ neu aufgebaut. Bei rund 100 Wartungen und 1.000 Einheiten ist das unmerklich und hält den km-Stand immer aktuell.
- „Neueste oben“: nach Datum absteigend, bei gleichem Datum die zuletzt erfasste oben.
- Das Datum wird wie bei den Einheiten als Text eingegeben (z. B. „06.09.2026“).
- Die Schaltflächen stehen über der Tabelle; das Menü „Einheit“ bleibt unverändert (W1a).
- Bei der Umsetzung ergänzt: Hat das oben gewählte Rad keine Wartungen, es gibt aber andere, steht „Keine Wartungen für dieses Rad.“ statt der Tabelle.
- Bei der Umsetzung ergänzt: Wird eine Wartung für ein anderes Rad gespeichert als das oben gewählte, wechselt die Auswahl auf „Alle Räder“, damit die Wartung sofort sichtbar ist; sie wird danach markiert.
- Bei der Umsetzung ergänzt: Steht in einer vorhandenen Wartung eine Tätigkeit nur in anderer Groß-/Kleinschreibung in der Liste, wird der Listeneintrag gewählt; fehlt sie ganz, bleibt sie im Wartungsfenster als erster Eintrag wählbar (wie ein ausgemustertes Rad).
- Auf Wunsch des Benutzers ergänzt: Solange die Karteikarte „Wartung“ gewählt ist, nennt die Statusleiste die Anzahl der Wartungen in der Datenbank (z. B. „86 Wartungen in der Datenbank“) statt der Einheiten.
- Auf Wunsch des Benutzers ergänzt: In der Radauswahl über der Tabelle und im Wartungsfenster stehen ausgemusterte Räder am Ende der Liste, mit dem Zusatz „(ausgemustert)“; die Spalte „Rad“ der Tabelle zeigt weiter nur den Namen.
- Nach 034 auf Wunsch des Benutzers geändert: Die Auswahl „Tätigkeit“ bietet die Einträge der Liste „Tätigkeit“ nach ihrer bisherigen Häufigkeit in den durchgeführten Wartungen aller Räder an, die häufigste zuerst, bei Gleichstand (auch bei nie verwendeten) in der Reihenfolge der Liste; freie Eingabe gibt es nicht. Umgesetzt in `Core/Berechnungen/OrdneTaetigkeitenNachHaeufigkeit.cs` (Tests `OrdneTaetigkeitenNachHaeufigkeitTests`), gezählt wie `ZaehleTaetigkeit` (ohne Groß-/Kleinschreibung, ohne Randleerzeichen). Antworten: A1 „Beide nach Häufigkeit“ (Wartungs- und Alarmfenster gleich), B1 „Über alle Räder“ (nicht je Rad).

### 022 – Wartungsalarme

Abhängig von: `wartung-erfassen`

#### Was und warum
Für ein Rad lässt sich ein Alarm einrichten, der nach einer bestimmten Kilometerzahl an eine Wartung erinnert, z. B. „Kettenwechsel alle 3.000 km“. Gezählt wird ab der letzten Wartung mit derselben Tätigkeit an diesem Rad; gibt es noch keine, ab dem Tag, an dem der Alarm angelegt wurde. Wird die Wartung erfasst, beginnt der Alarm von selbst neu. Fällige und bald fällige Alarme fallen auf der Karteikarte „Wartung“ und beim Programmstart auf, damit keine Wartung vergessen wird – wie die Alarme in BikeXperience, aber ohne sie nach jeder Wartung neu setzen zu müssen.

#### Kriterien

##### Anzeigen
- Wenn der Benutzer die Karteikarte „Wartung“ öffnet, dann sieht er über der Wartungstabelle eine eigene Liste der Alarme mit Rad, Tätigkeit, Intervall (km), gefahrenen km seit dem Bezugspunkt und verbleibenden km.
- Wenn ein Alarm angezeigt wird, dann sind die gefahrenen km die Distanzen aller Einheiten mit diesem Rad nach dem Bezugspunkt; Bezugspunkt ist der Tag der letzten Wartung mit derselben Tätigkeit an diesem Rad, ohne eine solche der Tag, an dem der Alarm angelegt wurde.
- Wenn ein Rad seit dem Bezugspunkt mindestens die Intervall-km gefahren ist, dann ist der Alarm fällig: Die Zeile ist rot, und die verbleibenden km erscheinen als Überschreitung (z. B. „−120“).
- Wenn bei einem Alarm nur noch höchstens 10 % des Intervalls übrig sind, er aber noch nicht fällig ist, dann ist er bald fällig, und die Zeile ist orange.
- Wenn mindestens ein Alarm fällig ist, dann trägt die Karteikarte die Anzahl der fälligen Alarme im Titel, z. B. „Wartung (2)“; ohne fällige Alarme heißt sie „Wartung“.
- Wenn beim Programmstart Alarme fällig sind, dann erscheint ein Hinweis, der jeden fälligen Alarm mit Rad, Tätigkeit und Überschreitung nennt.
- Wenn oben ein Rad gewählt ist, dann zeigt die Alarmliste nur dessen Alarme; mit „Alle Räder“ wieder alle.
- Wenn es keine Alarme gibt, dann steht statt der Alarmliste „Keine Alarme eingerichtet.“

##### Anlegen, ändern, löschen
- Wenn der Benutzer „Neuer Alarm…“ wählt, dann öffnet sich ein Fenster mit Rad, Tätigkeit (aus der Liste „Tätigkeit“, ohne freie Eingabe, geordnet wie im Wartungsfenster nach bisheriger Häufigkeit) und Intervall in km; ist oben ein Rad gewählt, ist es vorgewählt.
- Wenn der Benutzer mit „OK“ bestätigt, dann erscheint der Alarm sofort in der Liste und bleibt nach einem Neustart erhalten.
- Wenn der Benutzer einen Alarm doppelt anklickt oder „Bearbeiten…“ wählt, dann öffnet sich dasselbe Fenster mit seinen Werten; Änderungen erscheinen nach „OK“, „Abbrechen“ verwirft sie.
- Wenn der Benutzer einen oder mehrere Alarme markiert und „Löschen“ wählt oder Entf drückt, dann erscheint eine Rückfrage mit der Anzahl, und erst nach „Ja“ werden sie gelöscht.
- Wenn der Benutzer einen Alarm ausschaltet, dann wird er grau dargestellt, nicht fällig oder bald fällig und nicht beim Start gemeldet, bis er ihn wieder einschaltet.

##### Zurücksetzen
- Wenn der Benutzer für ein Rad eine Wartung mit der Tätigkeit eines Alarms erfasst, ändert oder löscht, dann zählt der Alarm sofort ab dem Tag der nun letzten solchen Wartung.
- Wenn eine Tätigkeit in der Liste „Tätigkeit“ umbenannt wird, dann tragen auch die Alarme mit dieser Tätigkeit den neuen Namen, und eine verwendete Tätigkeit lässt sich nicht löschen, solange ein Alarm sie nutzt.

##### Grenz- und Fehlerfälle
- Wenn das Intervall leer, keine ganze Zahl oder nicht größer als 0 ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
- Wenn kein Rad oder keine Tätigkeit gewählt ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
- Wenn für dasselbe Rad und dieselbe Tätigkeit schon ein Alarm besteht, dann ist „OK“ nicht möglich, und ein Hinweis nennt den vorhandenen Alarm.
- Wenn ein Rad ausgemustert ist, dann wird es für neue Alarme nicht angeboten, und seine Alarme werden weder fällig noch bald fällig noch beim Start gemeldet.
- Wenn keine oder mehrere Alarme markiert sind, dann ist „Bearbeiten…“ nicht wählbar; ohne Markierung ist auch „Löschen“ nicht wählbar.
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und die Liste bleibt beim vorherigen Stand.

#### Entscheidungen aus dem Plan

- Ein-/Ausschalten (B1a): Kontrollkästchen „Alarm eingeschaltet“ im Alarmfenster und „Ein-/Ausschalten“ im Kontextmenü für alle markierten Alarme.
- Alarme liegen als Typ „Alarm“ in der vorhandenen Tabelle `Wartung`; ihr Datum ist der Tag des Anlegens und bleibt beim Bearbeiten erhalten.
- Gezählt werden die Einheiten nach dem Bezugstag; eine Fahrt am Tag der Wartung zählt zum alten Teil (passend zum km-Stand „einschließlich des Tages“).
- Reihenfolge der Alarmliste: fällig, bald fällig, dann nach verbleibenden km aufsteigend; ausgeschaltete und die ausgemusterter Räder am Ende. Die Spalten lassen sich wie die Wartungstabelle sortieren.
- Der Hinweis beim Start erscheint nur beim Programmstart, nicht nach einem Datenordnerwechsel oder dem Wiederherstellen einer Sicherung.
- Rückfrage beim Umbenennen und Löschsperre einer Tätigkeit nennen Wartungen und Alarme getrennt (z. B. „In 32 Wartungen und 1 Alarm ‚Kettenwechsel‘ durch ‚Kette gewechselt‘ ersetzen?“); bisher zählten Alarme als Wartungen.
- Die Löschsperre eines Equipments zählt Alarme weiter als Wartungseinträge mit („hat 1 Wartung“); eine eigene Formulierung lohnt bei dieser Grenzlage nicht.
- Bei der Umsetzung ergänzt: Die Karteikarte ist in die Bereiche „Alarme“ und „Wartungen“ gegliedert, jeder mit eigener Leiste; die Radauswahl steht darüber und gilt für beide. Die Schaltflächen der Wartungen aus 021 stehen dadurch in ihrem Bereich statt neben der Radauswahl.
- Bei der Umsetzung ergänzt: „Ein-/Ausschalten“ schaltet alle markierten Alarme aus, sobald einer davon eingeschaltet ist; sind alle aus, werden alle eingeschaltet. Die Markierung bleibt danach erhalten.
- Bei der Umsetzung ergänzt: Hat das gewählte Rad keine Alarme, es gibt aber andere, steht „Keine Alarme für dieses Rad.“; der Titel der Karteikarte zählt die fälligen Alarme aller Räder, unabhängig von der Radauswahl.
- Nach Prüfung 2 ergänzt: Markierte fällige und bald fällige Alarme zeigen eine kräftigere Fassung ihrer Farbe statt der Markierungsfarbe des Systems, damit die Farbe auch bei einem einzigen (nach dem Speichern markierten) Alarm sichtbar bleibt.
- Bei der Umsetzung ergänzt: Das Intervall wird ohne Tausenderpunkt eingegeben („3000“), wie die übrigen ganzen Zahlen der App; angezeigt wird es mit Tausenderpunkt.
- Bei der Umsetzung ergänzt: Der Text „32 Wartungen und 1 Alarm“ für Rückfrage und Löschsperre entsteht an einer Stelle (`AuswahllistenViewModel.BeschreibeTaetigkeitsverwendung`).
- Nach 034 auf Wunsch des Benutzers geändert: Die Auswahl „Tätigkeit“ im Alarmfenster ist wie im Wartungsfenster nach der bisherigen Häufigkeit in den Wartungen aller Räder geordnet (`OrdneTaetigkeitenNachHaeufigkeit`, siehe Plan 021); Alarme zählen dabei nicht mit. Antworten A1 „Beide nach Häufigkeit“, B1 „Über alle Räder“.

### 023 – Haltbarkeit der Teile

Abhängig von: `wartung-erfassen`

#### Was und warum
Die Wartungstabelle zeigt zu jeder Wartung, wie viele Kilometer und Tage seit der vorherigen Wartung mit derselben Tätigkeit an diesem Rad vergangen sind – also wie lange z. B. die gewechselte Kette gehalten hat. Bei der ersten Wartung einer Art zählt der ganze bisherige Weg des Rads. So lässt sich wie mit der Auskunft in BikeXperience ablesen, wie lange ein Teil hält, und das nächste Wechselintervall abschätzen.

#### Kriterien

##### Spalten in der Wartungstabelle
- Wenn eine Wartung angezeigt wird, dann stehen neben dem km-Stand die Spalten „km seit letzter“ und „Tage seit letzter“ mit den Kilometern und Tagen seit der vorherigen Wartung mit derselben Tätigkeit an diesem Rad.
- Wenn die km seit der letzten gleichartigen Wartung berechnet werden, dann zählen die Distanzen der Einheiten mit diesem Rad nach dem Tag der vorherigen Wartung bis einschließlich zum Tag dieser Wartung (die Differenz der beiden km-Stände).
- Wenn die Tage berechnet werden, dann zählen die Kalendertage zwischen den beiden Wartungen (vom 01.05. bis zum 31.05. sind es 30 Tage).
- Wenn geprüft wird, ob zwei Wartungen gleichartig sind, dann zählt nur dieselbe Tätigkeit ohne Unterschied der Groß- und Kleinschreibung am selben Rad; andere Räder und andere Tätigkeiten zählen nicht.
- Wenn nach einer dieser Spalten sortiert wird, dann wird nach dem Zahlenwert sortiert; ein zweiter Klick kehrt die Reihenfolge um.

##### Erste Wartung einer Art
- Wenn es für eine Wartung keine vorherige gleichartige gibt, dann steht unter „km seit letzter“ der km-Stand des Rads an diesem Tag (einschließlich seiner Anfangskilometer).
- Wenn es für eine Wartung keine vorherige gleichartige gibt, dann stehen unter „Tage seit letzter“ die Tage seit dem Anschaffungsdatum des Rads; ist keines eingetragen, die Tage seit der ersten Einheit mit diesem Rad.
- Wenn es weder ein Anschaffungsdatum noch eine Einheit mit diesem Rad bis zum Tag der Wartung gibt, dann bleibt „Tage seit letzter“ leer.

##### Grenz- und Fehlerfälle
- Wenn beim Speichern einer Wartung für dasselbe Rad am selben Tag schon eine Wartung mit derselben Tätigkeit (ohne Unterschied der Groß- und Kleinschreibung) existiert, dann wird nicht gespeichert und es erscheint der Hinweis „Für ‚Genius‘ gibt es am 01.05.2026 schon die Wartung ‚Kettenwechsel‘.“; beim Bearbeiten zählt die Wartung selbst nicht.
- Wenn eine Wartung keinem Rad zugeordnet ist, dann bleiben beide Spalten leer.
- Wenn eine Wartung erfasst, geändert oder gelöscht wird oder sich Einheiten oder Räder ändern, dann zeigen die Spalten sofort den neuen Stand.

#### Entscheidungen aus dem Plan

- Vorherige gleichartige Wartung ist die mit dem spätesten Datum vor dieser. Doppelte am selben Tag kann es nach der neuen Prüfung nicht mehr geben; die Berechnung bleibt trotzdem eindeutig (bei gleichem Datum gilt die kleinere Nummer als vorherige, Ergebnis 0 km und 0 Tage).
- Zusammenführen (H5b): Tätigkeiten und Räder lassen sich nicht zusammenführen (nur Aktivitäten und Kategorien), und ein Umbenennen auf einen schon vorhandenen Namen wird ohne Unterschied der Schreibweise abgewiesen (`PruefeAuswahleintrag`). Auf diesem Weg kann also keine doppelte Wartung entstehen; es ist nichts zu sperren.
- Ein Anschaffungsdatum nach dem Tag der Wartung wird nicht verwendet; dann gilt die erste Einheit mit diesem Rad bis zum Tag der Wartung.
- Die Spalten zeigen km ohne Nachkommastellen und Tage als ganze Zahl; leere Werte bleiben leer.

### 024 – Equipment-Auswahl bei Wartung und Alarmen

Abhängig von: `wartungsalarme`

#### Was und warum
Auf der Karteikarte „Wartung“ und in den Fenstern für Wartungen und Alarme erscheinen nur die Equipments, die zur oben gewählten Aktivität gehören – bei „Bike“ die Räder, bei „Laufen“ die Schuhe. Ausgemusterte Equipments werden nicht mehr zur Auswahl angeboten. So bleibt die Auswahl kurz und enthält nur, was für die Wartung tatsächlich in Frage kommt; die Wartungen ausgemusterter Equipments bleiben trotzdem sichtbar.

#### Kriterien

##### Auswahl
- Wenn oben „alle Aktivitäten“ gewählt ist, dann gilt für die Karteikarte „Wartung“ und die Fenster für Wartungen und Alarme die Hauptaktivität (z. B. „Bike“).
- Wenn auf der Karteikarte „Wartung“ ein Equipment gewählt wird, dann bietet die Auswahl nach „Alle Equipments“ nur die Equipments im Einsatz an, die zur oben gewählten Aktivität gehören, alphabetisch; ausgemusterte erscheinen nicht.
- Wenn die Auswahl auf der Karteikarte „Wartung“ angezeigt wird, dann heißt ihr erster Eintrag „Alle Equipments“ (bisher „Alle Räder“).
- Wenn eine Wartung oder ein Alarm neu angelegt wird, dann bietet das Fenster nur die Equipments im Einsatz an, die zur oben gewählten Aktivität gehören.
- Wenn eine vorhandene Wartung oder ein vorhandener Alarm bearbeitet wird, dessen Equipment inzwischen ausgemustert ist, dann bleibt dieses Equipment im Fenster mit dem Zusatz „(ausgemustert)“ wählbar, damit sich der Eintrag ohne Umhängen speichern lässt.

##### Listen
- Wenn „Alle Equipments“ gewählt ist, dann zeigen die Listen „Wartungen“ und „Alarme“ nur die Einträge der Equipments, die zur oben gewählten Aktivität gehören.
- Wenn ein Equipment ausgemustert ist, dann bleiben seine Wartungen und Alarme unter „Alle Equipments“ sichtbar und lassen sich bearbeiten und löschen.
- Wenn ein Equipment ausgemustert ist, dann steht in den Listen „Wartungen“ und „Alarme“ hinter seinem Namen der Zusatz „(ausgemustert)“.
- Wenn Alarme fällig sind, dann zählen der Titel der Karteikarte „Wartung“ und der Hinweis beim Programmstart alle fälligen Alarme, unabhängig von der gewählten Aktivität.

##### Aktualität
- Wenn oben eine andere Aktivität gewählt wird, dann passen sich Auswahl und Listen sofort an; gehört das gewählte Equipment nicht zur neuen Aktivität, steht die Auswahl danach auf „Alle Equipments“.
- Wenn ein Equipment ausgemustert, wieder in Einsatz genommen oder einer anderen Aktivität zugeordnet wird, dann zeigen Auswahl und Listen sofort den neuen Stand.

#### Entscheidungen aus dem Plan

- Bei „alle Aktivitäten“ gilt die Hauptaktivität (B1b); gibt es keine Hauptaktivität, wird nicht nach Aktivität gefiltert.
- Ein Equipment ohne zugeordnete Aktivität gilt als passend zu jeder Aktivität, damit es erreichbar bleibt; eine Wartung ohne Equipment erscheint ebenfalls immer.
- Beim Bearbeiten bleibt das bisherige Equipment auch dann wählbar, wenn es zu einer anderen Aktivität gehört (nachträglich umgehängtes Equipment).
- Die Beschriftung vor der Auswahl heißt „Equipment:“ statt „Rad:“, passend zu „Alle Equipments“.
- Titel der Karteikarte und Hinweis beim Programmstart rufen die Fachfunktionen ohne Aktivität auf und zählen so alle fälligen Alarme (A4a).

### 025 – Ausgemustertes Equipment kennzeichnen

Abhängig von: `wartung-equipment-auswahl`

#### Was und warum
Überall, wo die App Equipment in einer Liste, Tabelle oder Auswahl zeigt, steht hinter ausgemustertem Equipment der Zusatz „(ausgemustert)“ – nicht nur in den Einstellungen und auf der Karteikarte „Wartung“. So ist bei alten Touren sofort erkennbar, dass das Rad oder der Schuh nicht mehr im Einsatz ist. Der gespeicherte Name des Equipments bleibt dabei unverändert; der Zusatz gehört nur zur Anzeige.

#### Kriterien

##### Einheitentabelle
- Wenn eine Einheit mit ausgemustertem Equipment in der Einheitentabelle steht, dann zeigt die Spalte „Equipment“ den Namen mit dem Zusatz „(ausgemustert)“.
- Wenn nach der Spalte „Equipment“ sortiert wird, dann gilt die Reihenfolge der angezeigten Namen.
- Wenn die Equipment-Zelle einer solchen Einheit direkt in der Tabelle bearbeitet wird, dann steht das bisherige Equipment mit dem Zusatz „(ausgemustert)“ am Ende der Auswahl, nach den Equipments im Einsatz; wird es gewählt oder unverändert gelassen, bleibt die Zuordnung erhalten.

##### Bearbeitungsfenster
- Wenn eine Einheit mit ausgemustertem Equipment im Bearbeitungsfenster geöffnet wird, dann steht das bisherige Equipment mit dem Zusatz „(ausgemustert)“ am Ende der Auswahl und ist gewählt; beim Speichern bleibt die Zuordnung erhalten.

##### Filterleiste
- Wenn die Auswahl „Equipment“ der Filterleiste ein ausgemustertes Equipment enthält (weil Einheiten der Auswahl es haben), dann steht es mit dem Zusatz „(ausgemustert)“ am Ende, nach den Equipments im Einsatz.
- Wenn in der Filterleiste ein ausgemustertes Equipment gewählt wird, dann zeigt die Tabelle genau die Einheiten mit diesem Equipment.

##### Allgemein
- Wenn ein Equipment ausgemustert oder wieder in Einsatz genommen wird, dann erscheint bzw. verschwindet der Zusatz überall sofort; sein gespeicherter Name bleibt unverändert.
- Wenn eine weitere Liste, Tabelle oder Auswahl Equipment anzeigt, dann trägt ausgemustertes Equipment dort ebenso den Zusatz „(ausgemustert)“ – auch in späteren Funktionen wie der Auskunft „Equipment-Einsatz“.

#### Entscheidungen aus dem Plan

- `Passt` akzeptiert den gespeicherten Namen und den Namen mit Zusatz, unabhängig vom aktuellen Zustand. So funktionieren Zellbearbeitung und Filter auch mit einem Filterwert, der vor dem Ausmustern bzw. Wiedereinsetzen gewählt wurde.
- „Equipment ändern…“ (Mehrfachauswahl) und der Garmin-Import bieten ausgemusterte Equipments weiterhin nicht an. „Equipment ändern…“ läuft über `ErmittleEquipmentauswahl` und erhält `Bilden` ohnehin mit; der Garmin-Import bleibt unverändert.
- Hinweis- und Fehlertexte (z. B. „Für ‚Namur‘ gibt es schon …“, Löschsperre) nennen weiterhin den gespeicherten Namen; sie sind keine Listen oder Auswahlen.

### 026 – Suche in Name und Kommentar

Abhängig von: `einheiten-tabelle`

#### Was und warum
Das Feld „Name:“ der Filterleiste wird zum Feld „Suche:“ und findet Einheiten über einen Suchbegriff in Name und Kommentar – z. B. alle Touren „mit Sabine“. Damit erkennbar ist, warum eine Einheit gefunden wurde, zeigt die Einheitentabelle den Kommentar in einer eigenen Spalte. Anhänge werden nicht durchsucht, weil es nur Aufzeichnungsdateien mit Zeitstempel als Namen sind.

#### Kriterien

##### Suchen
- Wenn in der Filterleiste ein Suchbegriff eingegeben wird, dann zeigt die Tabelle nur die Einheiten, deren Name oder Kommentar den Begriff enthält, ohne Unterschied der Groß- und Kleinschreibung.
- Wenn der Suchbegriff aus mehreren Wörtern besteht, dann muss jedes Wort in Name oder Kommentar vorkommen, in beliebiger Reihenfolge (z. B. findet „Sabine Alm“ eine Einheit „Bründlingalm“ mit dem Kommentar „mit Sabine“).
- Wenn der Suchbegriff geändert wird, dann wird die Tabelle sofort beim Tippen neu gefiltert.
- Wenn nur Leerzeichen eingegeben sind, dann wird nicht gefiltert.
- Wenn die Suche zusammen mit den übrigen Filtern und mit der Jahres- und Aktivitätsauswahl wirkt, dann zeigt die Tabelle nur Einheiten, die alles erfüllen; für eine Suche über alle Jahre wird oben „alle Jahre“ gewählt.
- Wenn keine Einheit passt, dann erscheint der Hinweis „Keine Einheiten passen zum Filter.“.
- Wenn „Filter zurücksetzen“ gewählt wird, dann ist das Suchfeld leer.
- Wenn die Filterleiste angezeigt wird, dann heißt das Feld „Suche:“ (bisher „Name:“).

##### Spalte „Kommentar“
- Wenn die Einheitentabelle angezeigt wird, dann zeigt die Spalte „Kommentar“ den Kommentar jeder Einheit; ohne Kommentar bleibt sie leer.
- Wenn nach der Spalte „Kommentar“ sortiert wird, dann gilt die alphabetische Reihenfolge; ein zweiter Klick kehrt sie um.
- Wenn ein Kommentar im Bearbeitungsfenster geändert wird, dann zeigt die Spalte sofort den neuen Text, und die Suche berücksichtigt ihn.

#### Entscheidungen aus dem Plan

- Die Spalte „Kommentar“ ist schreibgeschützt; der Kommentar wird wie bisher im Bearbeitungsfenster geändert.
- Die neue Spalte steht am Ende der Tabelle; über die vorhandene Spaltenanordnung lässt sie sich verschieben und ausblenden.
- Das Filterfeld heißt im Code künftig `Suchbegriff` statt `Name`, weil es nicht mehr nur den Namen durchsucht.

### 027 – Einsatz je Equipment

Abhängig von: `equipments-verwalten`

#### Was und warum
Eine Auskunft zeigt für jedes Equipment die insgesamt gefahrenen Kilometer und Höhenmeter, wie die Auskunft „Einsatz je Equipment“ in BikeXperience. So lässt sich ablesen, wie viel jedes Rad gefahren wurde. Die Kilometer stimmen mit dem km-Stand der Wartung überein.

#### Kriterien

##### Aufruf und Aufbau
- Wenn im Menü „Extras“ der Eintrag „Equipment-Einsatz…“ gewählt wird, dann öffnet sich ein Fenster mit einer Tabelle: eine Zeile je Equipment mit den Spalten „Equipment“, „vorher (km)“, „Gesamt (km)“ und „Gesamt (hm)“.
- Wenn die Auskunft berechnet wird, dann zählen alle Einheiten, unabhängig von der Jahres- und Aktivitätsauswahl im Hauptfenster.
- Wenn „Gesamt (km)“ berechnet wird, dann ist es die Summe der Distanzen aller Einheiten mit diesem Equipment plus seine Anfangskilometer; es entspricht damit dem km-Stand des Rads.
- Wenn „Gesamt (hm)“ berechnet wird, dann ist es die Summe der Höhenmeter aller Einheiten mit diesem Equipment.
- Wenn für ein Equipment Anfangskilometer eingetragen sind, dann stehen sie in der Spalte „vorher (km)“.
- Wenn Werte angezeigt werden, dann sind sie auf ganze Zahlen gerundet; ist ein Wert 0, bleibt die Zelle leer.
- Wenn die Tabelle angezeigt wird, dann bildet die letzte Zeile „Summe“ die Summe jeder Spalte.

##### Welche Equipments
- Wenn das Fenster geöffnet wird, dann stehen zuerst die Equipments im Einsatz, absteigend nach „Gesamt (km)“, danach die ausgemusterten mit dem Zusatz „(ausgemustert)“, ebenfalls absteigend nach „Gesamt (km)“, danach die Zeile „ohne Equipment“ und zuletzt „Summe“.
- Wenn ein Equipment weder eine Einheit noch Anfangskilometer hat, dann erscheint es nicht.
- Wenn Einheiten kein Equipment haben, dann erscheinen sie in der Zeile „ohne Equipment“; gibt es keine solchen Einheiten, fehlt die Zeile.

##### Grenzfälle
- Wenn es keine Einheit und keine Anfangskilometer gibt, dann zeigt das Fenster statt der Tabelle den Hinweis „Keine Einheiten vorhanden.“.
- Wenn nach einer Spalte sortiert wird, dann gilt bei den Zahlenspalten der Zahlenwert, bei „Equipment“ das Alphabet; ein zweiter Klick kehrt die Reihenfolge um; die Zeile „Summe“ bleibt dabei unten.
- Wenn das Fenster geöffnet wird, dann zeigt es den aktuellen Stand der Einheiten und Equipments; geändert wird darin nichts.

#### Entscheidungen aus dem Plan

- „Summe“ ist eine Zeile der Tabelle. Die Sortierung übernimmt `SortiereEinsatzzeilen` statt der eingebauten Sortierung, damit „Summe“ unten bleibt.
- Beim Öffnen trägt „Gesamt (km)“ den Sortierpfeil „absteigend“. Ein Klick darauf sortiert danach rein nach dem Zahlenwert, ohne Trennung nach Einsatz und Ausmusterung.
- Einheiten, deren Equipment im Bestand nicht (mehr) vorhanden ist, zählen zu „ohne Equipment“.
- Das Fenster wird modal geöffnet und zeigt den Stand beim Öffnen; es braucht deshalb keine Aktualisierung bei Änderungen.

### 028 – Track als GPX exportieren

Abhängig von: `streckenkarte`

#### Was und warum
Die Strecke einer Einheit lässt sich aus dem Tourdetail als GPX-Datei speichern, wie in BikeXperience. So kann eine gefahrene Tour in anderen Programmen angesehen, weitergegeben oder als Vorlage für eine neue Tour genutzt werden. Bei einer Mehrtagestour landen alle Etappen gemeinsam in einer Datei.

#### Kriterien

##### Exportieren
- Wenn das Tourdetail geöffnet ist, dann gibt es auf der Karteikarte „Karte“ neben „Ganze Strecke“ die Schaltfläche „Als GPX exportieren…“.
- Wenn „Als GPX exportieren…“ gewählt wird, dann öffnet sich ein Speichern-Dialog mit dem zuletzt dafür verwendeten Ordner und dem Dateinamen aus Datum und Name der Einheit, z. B. „2026-05-01 Bründlingalm.gpx“.
- Wenn gespeichert wird, dann enthält die Datei die Strecke der Aufzeichnung mit Position und Höhe jedes Messpunkts, ohne Uhrzeiten; der Track trägt den Namen der Einheit.
- Wenn die Datei in einem anderen Programm geöffnet wird (z. B. Garmin BaseCamp, Komoot, ein GPS-Viewer), dann zeigt es dieselbe Strecke wie die Karte im Tourdetail.
- Wenn das Speichern gelungen ist, dann erscheint neben der Schaltfläche der Hinweis „Track exportiert: 2026-05-01 Bründlingalm.gpx“.

##### Mehrtagestour
- Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann speichert „Als GPX exportieren…“ alle Etappen in eine Datei, jede Etappe als eigener Track mit dem Namen ihrer Einheit, nach Datum und Startzeit geordnet.
- Wenn eine Mehrtagestour exportiert wird, dann lautet der vorgeschlagene Dateiname „2026-05-01 bis 2026-05-04 Alpencross.gpx“ mit dem ersten und letzten Datum und dem Namen der ersten Etappe.
- Wenn einzelne Etappen keine GPS-Daten haben, dann fehlen sie in der Datei; die übrigen Etappen werden exportiert.

##### Grenz- und Fehlerfälle
- Wenn keine der angezeigten Aufzeichnungen GPS-Daten enthält (z. B. eine Fahrt auf der Rolle), dann ist „Als GPX exportieren…“ nicht wählbar.
- Wenn einzelne Messpunkte keine Position haben (z. B. im Tunnel), dann fehlen sie in der Datei; die übrigen Punkte bleiben erhalten.
- Wenn ein Zeichen im Namen der Einheit in Dateinamen nicht erlaubt ist (z. B. „/“), dann wird es im vorgeschlagenen Dateinamen durch „-“ ersetzt.
- Wenn die gewählte Datei schon existiert, dann fragt Windows wie üblich, ob sie ersetzt werden soll.
- Wenn der Dialog abgebrochen wird, dann wird nichts geschrieben und kein Hinweis angezeigt.
- Wenn die Datei nicht geschrieben werden kann (z. B. Ordner schreibgeschützt), dann erscheint der Hinweis „Der Track konnte nicht exportiert werden: …“ mit dem Grund.

#### Entscheidungen aus dem Plan

- Der GPX-Text entsteht im Core als reine Funktion und ist so testbar; nur Dialog und Schreiben liegen in der App. Eine zusätzliche Bibliothek ist nicht nötig.
- Ohne Uhrzeiten (Wunsch des Benutzers): GPX 1.1 verlangt je Punkt nur Breite und Länge, die Datei bleibt normkonform. Damit bleiben `Verlaufspunkt` und `LeseFitVerlauf` unverändert.
- Der zuletzt verwendete Ordner wird im Datenordner gespeichert und gilt damit auch nach einem Neustart. Ist noch keiner gespeichert oder gibt es ihn nicht mehr, schlägt der Dialog den Ordner „Dokumente“ vor.
- Liegen bei einer Mehrtagestour erstes und letztes Datum auf demselben Tag, enthält der Dateiname nur dieses eine Datum.
- Das Datum im Dateinamen stammt von allen Etappen des Tourdetails, auch von solchen ohne GPS-Daten, damit er zur Kopfzeile passt.

### 029 – Track bearbeiten

Abhängig von: `streckenkarte`

#### Was und warum
Im Tourdetail einer einzelnen Einheit lässt sich die Strecke auf der Karte kürzen, umkehren und glätten, wie im GPS-Viewer von BikeXperience. Das Ergebnis wird über „Als GPX exportieren…“ gespeichert, z. B. als Vorlage zum Weitergeben oder für eine neue Tour. Die Einheit und ihre Aufzeichnung bleiben dabei unverändert, damit die Originaldaten sicher sind.

#### Kriterien

##### Bearbeiten im Tourdetail
- Wenn das Tourdetail einer einzelnen Einheit mit GPS-Daten geöffnet ist, dann gibt es auf der Karteikarte „Karte“ unter der Karte die Leiste „Track bearbeiten“ mit den Schiebereglern „Start“ und „Ziel“, „Umkehren“, „Glätten“ und „Zurücksetzen“.
- Wenn ein Schieberegler „Start“ oder „Ziel“ verschoben wird, dann steht daneben seine Stelle in km ab dem Start der Aufzeichnung, die Karte zeigt nur noch das Stück dazwischen, und die Zeichen für Start und Ziel wandern mit.
- Wenn „Umkehren“ eingeschaltet wird, dann tauschen Start und Ziel ihre Rollen; die Strecke läuft rückwärts.
- Wenn bei „Glätten“ „schwach“ oder „stark“ gewählt wird, dann zeigt die Karte die entsprechend geglättete Linie – das GPS-Zittern verschwindet, „stark“ glättet mehr als „schwach“ – und die Strecke hat weniger Punkte; „aus“ zeigt die ungeglättete Linie.
- Wenn die Strecke angezeigt wird, dann steht in der Leiste die Länge der bearbeiteten Strecke, z. B. „Länge: 42,3 km“.
- Wenn „Zurücksetzen“ gewählt wird, dann stehen Start und Ziel wieder an Anfang und Ende, „Umkehren“ ist aus, „Glätten“ steht auf „aus“, und die Karte zeigt die ganze, unbearbeitete Strecke.

##### Speichern
- Wenn nach dem Bearbeiten „Als GPX exportieren…“ gewählt wird, dann enthält die Datei die bearbeitete Strecke – gekürzt, ggf. umgekehrt und geglättet –, weiter mit der Höhe je Punkt.
- Wenn die Strecke bearbeitet wurde, dann schlägt der Speichern-Dialog den Dateinamen mit dem Zusatz „(bearbeitet)“ vor, z. B. „2026-05-01 Bründlingalm (bearbeitet).gpx“; unbearbeitet bleibt es beim bisherigen Namen.
- Wenn die Strecke bearbeitet wurde, dann bleiben Einheit, Aufzeichnung, Distanz, Höhenmeter und Höhenprofil unverändert.
- Wenn das Tourdetail geschlossen und wieder geöffnet wird, dann ist die Bearbeitung verworfen.

##### Grenzfälle
- Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann fehlt die Leiste „Track bearbeiten“.
- Wenn die Aufzeichnung keine GPS-Daten enthält, dann fehlt die Leiste.
- Wenn „Start“ und „Ziel“ zusammengeschoben werden, dann bleibt immer ein Stück von mindestens zwei Punkten übrig; „Start“ kann nicht hinter „Ziel“ gezogen werden und umgekehrt.
- Wenn „Ganze Strecke“ gewählt wird, dann zeigt die Karte die ganze bearbeitete Strecke.

#### Entscheidungen aus dem Plan

- Die Länge ist die Distanz laut Aufzeichnung zwischen Start- und Zielpunkt, nicht die Länge der geglätteten Linie. So passt sie zu den km-Angaben der Regler und ungekürzt zur Distanz der Einheit; Glätten ändert sie nicht.
- Die Regler arbeiten mit den Punkten der Aufzeichnung, die eine Position haben. Ein Regelschritt ist ein Punkt, bei einer Aufzeichnung je Sekunde also wenige Meter.
- Beim Ziehen bleibt der Kartenausschnitt stehen. Erst „Ganze Strecke“ passt ihn an.
- Geglättet wird das bereits gekürzte Stück, umgekehrt wird zuletzt. Mittelwert und Vereinfachung rechnen in Metern auf einer örtlichen Ebene. Für Radtouren ist das genau genug.

### 030 – Tourenplanung auf der Karte

Abhängig von: `streckenkarte`

#### Was und warum
Der Benutzer plant eine neue Radtour, indem er Start und Ziel in eine Karte klickt und die Streckenführung mit
Zwischenpunkten anpasst. Die Strecke folgt dabei automatisch den Wegen, die zum gewählten Fahrprofil passen.
Länge und Höhenmeter erscheinen sofort. An jeder Abzweigung, an der abgebogen werden muss, entsteht
automatisch ein Abbiegepunkt mit der Richtung. Die fertige Planung wird als
GPX-Datei gespeichert und lässt sich später wieder öffnen und weiterbearbeiten. Damit ersetzt AndyTours
die Tourenplanung von BikeXperience und schafft die Grundlage, um geplante Strecken später auf den Garmin
Edge 1040 zu übertragen.

#### Kriterien

##### Planen
- Wenn der Benutzer im Menü „Tourenplanung → Tour planen…“ wählt, dann öffnet sich ein Fenster mit einer Karte.
- Wenn das Planungsfenster öffnet, dann zeigt die Karte den zuletzt in der Planung gezeigten Ausschnitt; beim ersten Mal zeigt sie das Gebiet der zuletzt gefahrenen Einheit.
- Wenn der Benutzer in die Karte neben die Strecke klickt, dann wird dort ein Wegpunkt gesetzt und als neuer letzter Punkt an die Planung angehängt.
- Wenn die Planung mindestens zwei Wegpunkte hat, dann verbindet eine Linie die Wegpunkte in ihrer Reihenfolge und folgt dabei den Wegen, die das gewählte Fahrprofil vorsieht.

##### Fahrprofil
- Wenn der Benutzer das Fahrprofil wählt, dann stehen „Rennrad (nur Straße)“, „Trekking/Tourenrad“ und „Mountainbike (bevorzugt offroad)“ zur Auswahl.
- Wenn das Profil „Rennrad (nur Straße)“ gewählt ist, dann führt die Strecke nur über asphaltierte Straßen und Radwege, soweit möglich.
- Wenn das Profil „Trekking/Tourenrad“ gewählt ist, dann führt die Strecke über Straßen, Radwege und gut befahrbare Schotterwege.
- Wenn das Profil „Mountainbike (bevorzugt offroad)“ gewählt ist, dann bevorzugt die Strecke Feld-, Wald- und Schotterwege.
- Wenn der Benutzer das Fahrprofil während der Planung wechselt, dann wird die ganze Strecke mit dem neuen Profil neu ermittelt; die Wegpunkte bleiben an ihrer Stelle.
- Wenn das Planungsfenster öffnet, dann ist das zuletzt gewählte Fahrprofil eingestellt, beim ersten Mal „Trekking/Tourenrad“.
- Wenn der Benutzer eine gespeicherte Planung öffnet, dann ist das Fahrprofil eingestellt, mit dem sie gespeichert wurde.

##### Strecke
- Wenn der Benutzer die Karte mit gedrückter Maustaste verschiebt, dann wird kein Wegpunkt gesetzt.
- Wenn die Verbindung über Straßen für ein Teilstück nicht ermittelt werden kann (z. B. ohne Internet), dann wird dieses Teilstück als gerade Linie gezeichnet und ein Hinweis erscheint.
- Wenn sich die Strecke ändert, dann zeigt das Fenster sofort die Länge in km sowie die Höhenmeter bergauf und bergab an.

##### Wegpunkte ändern
- Wenn der Benutzer die Linie zwischen zwei Wegpunkten an einer Stelle mit der Maus wegzieht, dann entsteht dort ein neuer Wegpunkt zwischen diesen beiden, und die Strecke wird über ihn neu ermittelt.
- Wenn der Benutzer einen Wegpunkt mit der Maus zieht, dann wandert der Punkt mit und die angrenzenden Teilstücke werden neu ermittelt.
- Wenn der Benutzer mit der rechten Maustaste auf einen Wegpunkt klickt, dann wird dieser Punkt entfernt und seine Nachbarn werden mit dem gewählten Fahrprofil neu verbunden.
- Wenn der Benutzer „Letzten Punkt entfernen“ wählt, dann verschwindet der zuletzt gesetzte Wegpunkt samt seinem Teilstück.
- Wenn der Benutzer „Alles löschen“ wählt und die Rückfrage bestätigt, dann ist die Planung leer.

##### Abbiegepunkte
- Wenn die Strecke an einer Abzweigung abbiegt, dann entsteht dort automatisch ein Abbiegepunkt mit der Richtung (z. B. „links“, „rechts“, „leicht rechts“, „Kreisverkehr 2. Ausfahrt“); Straßennamen werden nicht angegeben.
- Wenn die Strecke an einer Abzweigung geradeaus weitergeht, dann entsteht dort kein Abbiegepunkt.
- Wenn sich die Strecke ändert, dann werden die Abbiegepunkte sofort neu ermittelt und als kleine Markierungen auf der Karte angezeigt.
- Wenn ein Teilstück als gerade Linie gezeichnet ist, dann enthält es keine Abbiegepunkte.
- Wenn der Benutzer die Planung speichert, dann enthält die Datei auch alle Abbiegepunkte mit ihrer Richtung.

##### Speichern und öffnen
- Wenn der Benutzer „Speichern…“ wählt, dann kann er Ordner und Dateinamen wählen; vorgeschlagen werden der zuletzt verwendete Ordner und der Name „Planung JJJJ-MM-TT“.
- Wenn der Benutzer die Datei speichert, dann ist sie eine GPX-Datei mit der geplanten Strecke, die andere Programme öffnen können.
- Wenn die Planung weniger als zwei Wegpunkte hat, dann lässt sie sich nicht speichern.
- Wenn die gewählte Datei schon existiert, dann fragt die App, ob sie überschrieben werden soll.
- Wenn die Datei nicht geschrieben werden kann, dann erscheint eine Fehlermeldung und die Planung bleibt erhalten.
- Wenn der Benutzer „Öffnen…“ wählt und eine von AndyTours gespeicherte Planung auswählt, dann erscheinen ihre Wegpunkte und ihre Strecke wieder und lassen sich weiterbearbeiten.
- Wenn die gewählte Datei keine von AndyTours gespeicherte Planung ist oder nicht gelesen werden kann, dann erscheint ein Hinweis und die aktuelle Planung bleibt unverändert.
- Wenn der Benutzer eine geöffnete Planung erneut speichert, dann werden Ordner und Dateiname der geöffneten Datei vorgeschlagen.
- Wenn der Benutzer das Fenster schließt oder eine andere Planung öffnet und es ungespeicherte Änderungen gibt, dann fragt die App, ob die Änderungen verworfen werden sollen.

#### Entscheidungen aus dem Plan

- Beim Ziehen eines Wegpunkts oder der Linie folgen die angrenzenden Teilstücke als gestrichelte Luftlinie der Maus. Ermittelt werden sie erst beim Loslassen, damit der Routingdienst nicht bei jeder Mausbewegung gefragt wird.
- Eine Mausbewegung bis 4 Pixel gilt als Klick. Ein Wegpunkt wird bis 10 Pixel, die Linie bis 6 Pixel Abstand getroffen.
- Teilstücke werden einzeln ermittelt: Eine Änderung fragt nur die betroffenen Teilstücke neu ab, ein Profilwechsel alle. Laufende Abfragen einer überholten Planung werden abgebrochen.
- Das Fahrprofil wird beim Wechsel gemerkt, der Kartenausschnitt beim Schließen des Fensters. Den Ordner merkt sich die App nach erfolgreichem Speichern oder Öffnen.
- Gibt es keine Einheit mit Aufzeichnung, zeigt die Karte beim ersten Öffnen Deutschland.
- Fähren werden gemieden (BRouter `profile:allow_ferries=0`, openrouteservice `avoid_features: ["ferries"]`): Bei der ersten Prüfung führten alle BRouter-Profile mit der Fähre quer über den Chiemsee.
- Kartenart ist OpenStreetMap. Die Umschaltung der Streckenkarte gehört nicht zur Spec.

### 031 – Overlays

Abhängig von: `streckenkarte`

#### Was und warum
Overlays sind gespeicherte Strecken, die zusätzlich auf einer Karte eingeblendet werden – etwa geplante Touren oder fremde GPX-Strecken. So sieht der Benutzer beim Ansehen einer Tour, einer Mehrtagestour oder beim Planen, wo er schon gefahren ist oder was er sich vorgemerkt hat. Die Overlays liegen in Ordnern mit Unterordnern, damit sie auch bei vielen Strecken übersichtlich bleiben. Verwaltet werden sie über den Menüpunkt „Tourenplanung → Overlays…“.

Nicht enthalten: Teilstücke eines Overlays in die Tourenplanung übernehmen („Zauberstab“), eigene Einheiten direkt als Overlay übernehmen (dafür zuerst den GPX-Export nutzen) und das Übertragen auf den Garmin Edge (gehört zu `courses-uebertragen`).

#### Kriterien

##### Verwaltung öffnen
- Wenn der Benutzer „Tourenplanung → Overlays…“ wählt, dann öffnet sich die Overlay-Verwaltung mit dem Ordnerbaum und den Overlays, jeweils mit einem Kontrollkästchen zum Einblenden.
- Wenn noch keine Overlays vorhanden sind, dann zeigt die Verwaltung einen Hinweis, wie man Overlays hinzufügt.

##### Overlays aufnehmen
- Wenn der Benutzer GPX-Dateien hinzufügt, dann erscheinen sie als Overlays im gewählten Ordner, benannt nach dem Dateinamen ohne Endung.
- Wenn ein Overlay hinzugefügt wurde, dann bleibt es erhalten, auch wenn die ursprüngliche Datei danach verschoben oder gelöscht wird.
- Wenn eine Planung aus der Tourenplanung gespeichert wurde, dann lässt sich diese Datei wie jede andere GPX-Datei als Overlay hinzufügen.
- Wenn eine Datei keine Strecke enthält oder nicht lesbar ist, dann wird sie nicht aufgenommen und ein Hinweis nennt die Datei.
- Wenn mehrere Dateien auf einmal hinzugefügt werden und einzelne davon fehlerhaft sind, dann werden die übrigen trotzdem aufgenommen.

##### Ordner verwalten
- Wenn der Benutzer einen Ordner anlegt, dann erscheint er im gewählten Ordner als Unterordner oder, wenn keiner gewählt ist, auf oberster Ebene.
- Wenn der Benutzer einen Ordner umbenennt, dann erscheint er unter dem neuen Namen.
- Wenn ein Ordnername leer ist oder im selben übergeordneten Ordner schon vorkommt, dann wird er nicht übernommen und ein Hinweis erscheint.
- Wenn der Benutzer einen Ordner löscht, der Overlays oder Unterordner enthält, dann kommt eine Rückfrage; nach dem Bestätigen sind der Ordner und sein gesamter Inhalt gelöscht.
- Wenn der Benutzer ein Overlay oder einen Ordner in einen anderen Ordner verschiebt, dann steht es danach dort.
- Wenn der Benutzer einen Ordner in sich selbst oder in einen seiner Unterordner verschieben will, dann geschieht nichts.

##### Overlays bearbeiten
- Wenn der Benutzer ein Overlay umbenennt, dann erscheint es unter dem neuen Namen.
- Wenn der Benutzer ein Overlay löscht, dann kommt eine Rückfrage; nach dem Bestätigen ist es entfernt und verschwindet von allen Karten.

##### Einblenden
- Wenn der Benutzer ein Overlay einschaltet, dann erscheint seine Strecke als Linie auf den Karten im Tourdetail, in der Mehrtagestour und in der Tourenplanung.
- Wenn der Benutzer einen Ordner einschaltet, dann werden alle Overlays darin einschließlich der Unterordner eingeschaltet; ausschalten wirkt ebenso auf den ganzen Inhalt.
- Wenn der Benutzer ein Overlay ausschaltet, dann verschwindet es von den Karten.
- Wenn eine Karte bereits geöffnet ist, während Overlays ein- oder ausgeschaltet werden, dann zeigt sie die Änderung sofort.
- Wenn Overlays eingeblendet sind, dann liegen sie unter der eigenen Strecke, den Wegpunkten und Abbiegepunkten und verdecken diese nicht.
- Wenn der Benutzer in der Verwaltung ein Overlay doppelt anklickt, dann zeigen die geöffneten Karten dessen ganze Strecke.
- Wenn die App neu gestartet wird, dann sind dieselben Overlays wieder eingeschaltet.

##### Farbe
- Wenn ein Overlay keine eigene Farbe hat, dann erscheint es in der gemeinsamen Overlay-Farbe, voreingestellt Violett.
- Wenn der Benutzer in den Einstellungen die gemeinsame Overlay-Farbe ändert, dann erscheinen alle Overlays ohne eigene Farbe in der neuen Farbe.
- Wenn der Benutzer einem Overlay eine eigene Farbe gibt, dann erscheint es in dieser Farbe, unabhängig von der gemeinsamen Farbe.
- Wenn der Benutzer die eigene Farbe eines Overlays zurücksetzt, dann erscheint es wieder in der gemeinsamen Farbe.

#### Entscheidungen aus dem Plan

- Overlays stehen in der Datenbank statt als GPX-Kopien im Datenordner (P1a). So sind sie in der automatischen Sicherung, die nur die Datenbank sichert.
- Farben stammen aus einer festen Liste von zwölf Farben (P2a). Der erste Eintrag „Gemeinsame Farbe“ setzt die eigene Farbe zurück. Kein Windows-Forms-Farbdialog.
- Verschoben wird über das Kontextmenü bzw. die Schaltfläche „Verschieben nach…“ und das vorhandene Fenster „Eintrag wählen“ (P3a). Ziehen und Ablegen gibt es nicht.
- Die Overlay-Linie ist 3 px breit und leicht durchscheinend (P4a). Die eigene Strecke hat 4 px und ist deckend.
- Overlays verweisen über die Kennung auf ihren Ordner, so bleibt das Umbenennen eines Ordners folgenlos. Overlay-Namen dürfen doppelt vorkommen, nur Ordnernamen nicht.
- Die Verwaltung ist nicht modal und nur einmal offen. Sie öffnet sich über „Tourenplanung → Overlays…“ und über die Schaltfläche „O_verlays…“ (Alt+V) in jeder Karte (K1a); ein erneuter Aufruf holt das offene Fenster nach vorn. Sie gehört zum Hauptfenster und bleibt offen, wenn eine Karte geschlossen wird.

### 032 – Courses übertragen

Abhängig von: `garmin-import`, `tourenplanung-karte`

#### Was und warum
Geplante und gefahrene Strecken lassen sich als Course direkt auf den angeschlossenen Garmin Edge übertragen, um sie unterwegs nachzufahren. Der Edge zeigt dann die Linie mit Höhenprofil und kündigt die Abbiegungen an: bei einer Planung die beim Planen ermittelten Abbiegepunkte, bei einer gefahrenen Strecke nachträglich ermittelte. Übertragen wird aus dem Planungsfenster und aus dem Tourdetail, auf den Edge 1040 ebenso wie auf die älteren Edge 1000 und Edge 800. Damit entfällt der Umweg über andere Programme.

Nicht enthalten: Courses aus der Overlay-Verwaltung übertragen und Courses als Datei speichern (eine Planung lässt sich schon als GPX speichern, eine Einheit als GPX exportieren).

#### Kriterien

##### Übertragen
- Wenn der Benutzer im Planungsfenster „Auf Garmin übertragen…“ wählt, dann wird die geplante Strecke als Course auf den angeschlossenen Edge übertragen.
- Wenn der Benutzer im Tourdetail „Auf Garmin übertragen…“ wählt, dann wird die gefahrene Strecke der Einheit als Course übertragen.
- Wenn der Benutzer überträgt, dann kann er vorher den Namen des Course eingeben; vorgeschlagen wird bei einer gespeicherten Planung der Dateiname ohne Endung, bei einer ungespeicherten „Planung JJJJ-MM-TT“, bei einer Einheit Datum und Name wie beim GPX-Export.
- Wenn der Benutzer die Namenseingabe abbricht, dann wird nichts übertragen.
- Wenn die Übertragung gelungen ist, dann erscheint der Hinweis „Course „X“ übertragen. Er erscheint auf dem Edge nach dem Trennen vom Computer unter „Courses“.“

##### Inhalt des Course
- Wenn ein Course auf dem Edge geöffnet wird, dann zeigt er dieselbe Strecke wie die Karte in AndyTours, mit Höhenprofil.
- Wenn eine Planung mit Abbiegepunkten übertragen wird, dann kündigt der Edge beim Nachfahren die Abbiegungen mit ihrer Richtung an.
- Wenn eine gefahrene Strecke übertragen wird, dann werden ihre Abbiegungen mit dem gewählten Routingdienst ermittelt und der Edge kündigt sie beim Nachfahren an; die Linie des Course bleibt die gefahrene Strecke.
- Wenn die Abbiegungen einer gefahrenen Strecke nicht ermittelt werden können (z. B. ohne Internet), dann wird der Course ohne Abbiegehinweise übertragen und ein Hinweis nennt den Grund.
- Wenn eine gefahrene Strecke abseits von Wegen verläuft, dann enthält der Course dort keine Abbiegehinweise.

##### Mehrtagestour
- Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann wird jede Etappe mit GPS-Daten als eigener Course übertragen, benannt nach Datum und Name ihrer Einheit; der Name lässt sich je Etappe ändern.

##### Geräte
- Wenn ein Edge 1040, Edge 1000 oder Edge 800 per USB angeschlossen ist, dann lässt sich auf dieses Gerät übertragen.
- Wenn mehrere Garmin-Geräte angeschlossen sind, dann wählt der Benutzer, auf welches übertragen wird.
- Wenn kein Garmin-Gerät angeschlossen ist, dann erscheint der Hinweis „Kein Garmin-Gerät gefunden. Bitte den Edge per USB anschließen.“

##### Grenz- und Fehlerfälle
- Wenn die Planung weniger als zwei Wegpunkte hat oder keine der angezeigten Einheiten GPS-Daten enthält, dann ist „Auf Garmin übertragen…“ nicht wählbar.
- Wenn auf dem Gerät schon ein noch nicht übernommener Course mit demselben Namen wartet, dann fragt die App, ob er ersetzt werden soll; bei „Nein“ wird dieser Course nicht übertragen.
- Wenn die Übertragung scheitert (z. B. weil das Gerät getrennt wurde), dann erscheint ein Hinweis mit dem Grund, und Planung bzw. Einheit bleiben unverändert.

#### Entscheidungen aus dem Plan

- P1a: Das Fahrprofil für die Abbiegungen einer gefahrenen Strecke ist im Übertragungsfenster wählbar und mit dem zuletzt in der Tourenplanung gewählten Profil vorbelegt. Die Wahl wird nicht gespeichert.
- P2a: Ein Kreisverkehr wird ein allgemeiner Course-Punkt mit Text, z. B. „Kreisel 2. Ausf.“; das FIT-Format kennt keinen Kreisverkehr.
- P3a: Eine Planung erhält Zeiten bei 15 km/h, eine gefahrene Strecke ihre eigenen Fahrzeiten (virtueller Partner).
- Die Stützpunkte und die Schwellen (20 m, 1 km, 25 Punkte je Anfrage, 30 m in ±150 m) sind feste Werte im Code. Sie werden an echten Touren nachjustiert und nicht in den Einstellungen angeboten.
- Der Dateiname auf dem Gerät ist der Course-Name mit „.fit“. Die Rückfrage betrifft daher eine gleichnamige Datei in `Garmin\NewFiles`.

### 033 – Installationsprogramm

Abhängig von: `datenordner-waehlen`, `altdaten-umwandeln`

#### Was und warum
Ein Installationsprogramm richtet AndyTours auf dem Rechner ein und legt den Datenordner fest. Auf Wunsch übernimmt es die bisherigen Daten: BikeXperience-Daten über das vorhandene Umwandlungsprogramm oder, wenn nur Aufzeichnungen vorliegen, diese als Touren. Jede Änderung am Rechner hält es in einem Protokoll fest. Daran kann das Deinstallationsprogramm AndyTours später vollständig entfernen, ohne die Touren ungefragt zu löschen. So lässt sich AndyTours ohne Entwicklungsumgebung einrichten, aktualisieren und wieder entfernen.

#### Kriterien

##### Installieren
- Wenn der Benutzer das Installationsprogramm startet, dann führt es ihn in wenigen Schritten durch die Installation: Datenordner, Übernahme der bisherigen Daten, Verknüpfungen und Zusammenfassung vor dem Start.
- Wenn die Installation fertig ist, dann lässt sich AndyTours über das Startmenü starten. Weitere Programme müssen dafür nicht installiert werden.
- Wenn der Benutzer installiert, dann braucht er keine Administratorrechte, denn AndyTours wird nur für ihn installiert.
- Wenn das Installationsprogramm nach dem Datenordner fragt, dann schlägt es `Dokumente\AndyTours` vor.
- Wenn der Benutzer einen Datenordner gewählt hat, dann verwendet AndyTours beim ersten Start genau diesen Ordner.
- Wenn die Installation fertig ist, dann gibt es immer eine Verknüpfung im Startmenü.
- Wenn der Benutzer zusätzlich eine Desktop-Verknüpfung gewählt hat, dann liegt nach der Installation eine Verknüpfung auf dem Desktop.
- Wenn die Installation fertig ist, dann erscheint AndyTours in den Windows-Einstellungen unter „Apps“ und lässt sich dort deinstallieren.

##### Bisherige Daten übernehmen
- Wenn das Installationsprogramm die Übernahme anbietet, dann ist sie vorgewählt, und der Benutzer kann sie abwählen.
- Wenn der Benutzer die Übernahme gewählt hat, dann fragt das Installationsprogramm nach dem Ordner mit den bisherigen Daten, ohne einen Ordner vorzuschlagen.
- Wenn im angegebenen Ordner BikeXperience-Daten liegen, dann wandelt das vorhandene Umwandlungsprogramm sie in den gewählten Datenordner um.
- Wenn die Umwandlung fertig ist, dann zeigt das Installationsprogramm die Zusammenfassung des Umwandlungsprogramms und wo der Bericht liegt.
- Wenn im angegebenen Ordner keine BikeXperience-Daten, aber Aufzeichnungen im FIT-Format liegen (auch in Unterordnern), dann fragt das Installationsprogramm, ob diese als Touren übernommen werden sollen, und nennt die Anzahl der gefundenen Aufzeichnungen.
- Wenn der Benutzer die Übernahme der Aufzeichnungen bestätigt, dann ist danach jede lesbare Aufzeichnung eine Tour im Datenordner, mit denselben Werten wie bei einem Import vom Edge.
- Wenn Aufzeichnungen als Touren übernommen werden, dann bekommen die Touren kein Rad und die Hauptaktivität „Radfahren“ zugeordnet.
- Wenn eine Aufzeichnung nicht lesbar ist, dann wird sie übersprungen, die übrigen werden übernommen, und ein Bericht nennt die übersprungenen Dateien.
- Wenn die Übernahme der Aufzeichnungen fertig ist, dann zeigt das Installationsprogramm, wie viele Touren übernommen und wie viele Dateien übersprungen wurden und wo der Bericht liegt.
- Wenn der Benutzer die Übernahme der Aufzeichnungen verneint, dann wird nichts übernommen, und die Installation geht ohne bisherige Daten weiter.
- Wenn im angegebenen Ordner weder BikeXperience-Daten noch Aufzeichnungen liegen, dann erscheint ein Hinweis, und der Benutzer kann einen anderen Ordner wählen oder ohne Übernahme weitermachen.
- Wenn im gewählten Datenordner schon AndyTours-Daten liegen, dann bleiben sie unverändert, die Übernahme entfällt, und ein Hinweis sagt das.
- Wenn die Übernahme mit einem Fehler abbricht, dann ist AndyTours trotzdem installiert, der Datenordner bleibt wie vorher, und ein Hinweis nennt den Grund.
- Wenn die Übernahme läuft, dann bleibt der Ordner mit den bisherigen Daten unverändert.

##### Änderungen festhalten
- Wenn die Installation fertig ist, dann gibt es ein lesbares Protokoll aller Änderungen am Rechner: angelegte Ordner und Dateien, Verknüpfungen, Einträge in Windows und der eingestellte Datenordner.
- Wenn das Protokoll geöffnet wird, dann steht zu jeder Änderung, was angelegt oder geändert wurde und wann.

##### Aktualisieren
- Wenn AndyTours schon installiert ist und der Benutzer eine neuere Fassung installiert, dann bleiben Datenordner, Daten und Einstellungen erhalten, und die Übernahme der bisherigen Daten wird nicht mehr angeboten.
- Wenn eine neuere Fassung installiert wird, dann ergänzt das Installationsprogramm das vorhandene Protokoll um die neuen Änderungen.
- Wenn AndyTours beim Installieren läuft, dann bittet das Installationsprogramm, AndyTours zu beenden, und fährt erst danach fort.

##### Deinstallieren
- Wenn der Benutzer AndyTours deinstalliert, dann entfernt das Deinstallationsprogramm alles, was im Protokoll steht: Programmdateien, Verknüpfungen, Einträge in Windows und die Einstellungen der App außerhalb des Datenordners.
- Wenn der Benutzer deinstalliert, dann fragt das Deinstallationsprogramm, ob auch der Datenordner mit allen Touren gelöscht werden soll; vorgewählt ist „Behalten“.
- Wenn der Benutzer den Datenordner behält, dann sind die Daten danach unverändert vorhanden, und eine spätere Installation verwendet sie wieder.
- Wenn die Deinstallation fertig ist, dann bleibt außerhalb des Datenordners kein Ordner und keine Datei von AndyTours zurück.
- Wenn AndyTours beim Deinstallieren läuft, dann bittet das Deinstallationsprogramm, AndyTours zu beenden, und fährt erst danach fort.

#### Entscheidungen aus dem Plan

- Die Übernahme der Aufzeichnungen kommt in das vorhandene Umwandlungsprogramm, nicht in das Installationsprogramm. So bleibt alles, was Daten schreibt, in C# und testbar. Das Installationsprogramm muss dann nur ein Programm aufrufen.
- Bei der Übernahme wird nur ein leerer oder fehlender Datenordner befüllt (Zwischenordner, dann Austausch); vorhandene Daten bleiben immer unverändert.
- Der Kartenspeicher (`%LOCALAPPDATA%\AndyTours`) und die Merkdatei (`%APPDATA%\AndyTours`) gelten als „Einstellungen der App außerhalb des Datenordners“. Sie stehen im Protokoll und werden beim Deinstallieren entfernt.
- An der App ändert sich nichts: Den Datenordner bekommt sie über die vorhandene Merkdatei.
- Inno Setup statt WiX/MSI oder eines eigenen C#-Programms; Inno Setup erzeugt das Deinstallationsprogramm selbst, das lesbare Protokoll schreibt der `[Code]`-Teil zusätzlich.

### 034 – Erscheinungsbild und Programmicon

Abhängig von: —

#### Was und warum
Alle Fenster von AndyTours erhalten das moderne, helle Erscheinungsbild von Windows 11 mit abgerundeten Steuerelementen und der Akzentfarbe von Windows. Dazu bekommt AndyTours ein eigenes Programmicon – ein stilisiertes Fahrrad vor einer Bergsilhouette –, das in Titelleiste, Taskleiste, Explorer, Startmenü, auf dem Desktop, unter „Apps“ und im Installationsprogramm erscheint. Bisher wirkt die Oberfläche veraltet und die App trägt nur das allgemeine Windows-Symbol. Am Verhalten der Funktionen ändert sich nichts; die bisherigen farbigen Hervorhebungen bleiben erkennbar.

#### Kriterien

##### Erscheinungsbild
- Wenn ein Fenster von AndyTours geöffnet wird, dann zeigt es das moderne Erscheinungsbild von Windows 11: abgerundete Schaltflächen, Eingabefelder und Auswahllisten sowie moderne Menüs, Karteikarten, Tabellen und Bildlaufleisten – in allen Fenstern der App.
- Wenn Windows auf den dunklen Modus eingestellt ist, dann bleibt AndyTours trotzdem hell.
- Wenn ein Element markiert, gewählt oder mit der Tastatur angesteuert ist, dann ist es in der Akzentfarbe von Windows deutlich erkennbar.
- Wenn ein Fenster in seiner Anfangsgröße oder in seiner Mindestgröße geöffnet wird, dann sind alle Beschriftungen, Felder und Schaltflächen vollständig sichtbar, ohne abgeschnittenen Text.
- Wenn das Hauptfenster mit der gemerkten Größe und Position öffnet, dann zeigt es wie bisher die gemerkten Spaltenbreiten, die Spaltenreihenfolge und die zuletzt gewählte Karteikarte.
- Wenn die App eine Rückfrage oder einen Hinweis zeigt (z. B. „Änderungen verwerfen?“), dann erscheint wie bisher das Meldungsfenster von Windows.

##### Bestehende Hervorhebungen bleiben erkennbar
- Wenn die Tabelle der Trainingseinheiten Bestwerte enthält, dann sind diese weiter rot.
- Wenn Alarme fällig, bald fällig oder ausgeschaltet sind, dann sind ihre Zeilen weiter rot, orange bzw. grau, auch wenn sie markiert sind.
- Wenn ein Eingabefeld ungültig ist, dann ist es weiter rot markiert, und ein Hinweistext darunter ist weiter rot.
- Wenn ein Filter gesetzt ist, dann ist „Filter zurücksetzen“ weiter sichtbar hervorgehoben.
- Wenn das laufende Jahr einen Rekord hält, dann ist dieser auf der Karteikarte „Aktuell“ weiter hervorgehoben.
- Wenn Diagramme (Jahresfortschritt, Höhenprofil) und Karten angezeigt werden, dann sind sie unverändert lesbar und bedienbar.

##### Bedienung unverändert
- Wenn der Benutzer die App mit der Tastatur bedient, dann funktionieren Zugriffstasten (unterstrichene Buchstaben), Tastenkürzel, die Tab-Reihenfolge sowie Enter und Esc in den Fenstern wie bisher.
- Wenn der Benutzer eine Funktion der App aufruft, dann verhält sie sich wie vorher; nur das Aussehen hat sich geändert.

##### Programmicon
- Wenn AndyTours läuft, dann zeigen die Titelleiste jedes Fensters, die Taskleiste und die Fensterumschaltung mit Alt+Tab das Programmicon: ein stilisiertes Fahrrad mit orangem Rahmen und dunklen Rädern, Lenker und Sattel vor grünen Bergen mit weißen Gipfeln und blauem Himmel auf einem abgerundeten Quadrat.
- Wenn das Icon klein dargestellt wird (Titelleiste, Taskleiste), dann sind Fahrrad und Berge noch erkennbar.
- Wenn die Programmdatei von AndyTours im Explorer angezeigt wird, dann trägt sie das Icon.
- Wenn AndyTours installiert ist, dann tragen die Verknüpfungen im Startmenü und auf dem Desktop sowie der Eintrag in den Windows-Einstellungen unter „Apps“ das Icon.
- Wenn die Datei des Installationsprogramms im Explorer angezeigt wird oder das Installationsprogramm läuft, dann zeigt es das Icon.
- Wenn die Datei des Umwandlungsprogramms für die BikeXperience-Daten im Explorer angezeigt wird, dann trägt sie dasselbe Icon.

##### Über AndyTours
- Wenn der Benutzer „Hilfe → Über AndyTours“ wählt, dann öffnet sich ein eigenes kleines Fenster mit dem Icon in großer Darstellung, dem Namen „AndyTours“, der Versionsnummer und dem Autor.
- Wenn das Fenster „Über AndyTours“ offen ist, dann schließt es sich mit „OK“, Enter oder Esc.

#### Entscheidungen aus dem Plan

- **Eine Quelle für das Motiv:** Das Icon ist eine Vektorzeichnung in XAML. Sie dient zugleich dem Über-Fenster (scharf in jeder Größe) und als Vorlage für die `.ico`. So gibt es kein zusätzliches Grafikprogramm und kein zweites Motiv, das auseinanderlaufen kann.
- **Kleine Fassung:** Für 16–24 px gibt es eine vereinfachte Zeichnung (weniger Speichen, kräftigere Linien, ein Berg), damit das Icon in der Titelleiste erkennbar bleibt.
- **`.ico` im Repository:** Das Skript läuft nur, wenn sich das Motiv ändert. Bauen und Installationsprogramm brauchen es nicht.
- **Icon der Fenster:** Die Fenster bekommen kein eigenes `Icon`; WPF zeigt dann das Icon der Programmdatei. Dialoge zeigen es so ebenfalls.
- **Autor:** Er kommt als `Copyright` aus `Directory.Build.props` in die Programmdatei, wie die Version. So steht er auch in den Dateieigenschaften im Explorer.
- **Meldungsfenster:** bleiben Windows-Standard (Spec, Antwort K1).
- Nach dem Rundgang ergänzt: Tabellenzellen stellen ihren Inhalt senkrecht mittig. Dafür gibt es app-weit in `App.xaml` einen Zellstil, der auf `DefaultDataGridCellStyle` aufbaut, unter dem Typ `DataGridCell` abgelegt ist (die Zellstile der Ansichten bauen mit `BasedOn="{StaticResource {x:Type DataGridCell}}"` darauf auf) und die Hintergrundfarbe der Zelle übernimmt, damit fachliche Farben auch markiert sichtbar bleiben; Markierung und Fokusrahmen stehen als Stil-Trigger.
- Nach dem Rundgang ergänzt: Beschriftungen (`Label`) haben app-weit den Innenabstand `5,5,12,5`, weil das Fluent-Label sonst direkt am Feld klebt.
- `WPF0001` (ThemeMode experimentell) ist im App-Projekt per `NoWarn` unterdrückt, damit der Build ohne Warnungen bleibt.
- Später (035) ergänzt: eigene Kontrastfarben für Karteikarten, Hauptmenü, Felder und Listen, siehe `docs/adr/0008-kontrastfarben-der-oberflaeche.md`.
- Grundlage der Oberfläche als verbindliche Regel für alle Fenster: ADR 0006.

### 035 – Oberfläche skalieren

Abhängig von: `erscheinungsbild-und-icon`

#### Was und warum
Der Benutzer kann in den Einstellungen die ganze Oberfläche von AndyTours einschließlich der Schrift in festen Stufen von 100 % bis 200 % vergrößern. Auf seinem Bildschirm wirkt bisher alles recht klein; mit einer größeren Stufe werden Texte, Felder, Tabellen und Schaltflächen gleichmäßig größer und besser lesbar. Die Wahl wirkt sofort, ohne Neustart, und bleibt mit dem Datenbestand erhalten.

#### Kriterien

- Wenn der Benutzer die Einstellungen öffnet, dann findet er eine Gruppe „Darstellung“ mit der Auswahl „Größe der Oberfläche“ und den Stufen 100 %, 110 %, 125 %, 150 %, 175 % und 200 %.
- Wenn noch nie eine Größe gewählt wurde, dann ist 100 % eingestellt, und die Oberfläche sieht aus wie bisher.
- Wenn der Benutzer eine Stufe wählt, dann werden sofort alle offenen Fenster vergrößert bzw. verkleinert – Schrift, Felder, Schaltflächen, Tabellen und Abstände gleichermaßen –, ohne Neustart.
- Wenn danach ein weiteres Fenster geöffnet wird, dann erscheint es in der gewählten Größe.
- Wenn der Benutzer AndyTours beendet und wieder startet, dann gilt die zuletzt gewählte Stufe.
- Wenn der Benutzer einen anderen Datenordner wählt oder eine Sicherung wiederherstellt, dann gilt die dort gespeicherte Stufe, weil die Einstellung zum Datenbestand gehört.
- Wenn ein Dialog in einer großen Stufe nicht mehr auf den Bildschirm passt, dann wird er höchstens so groß wie der Bildschirm, sein Inhalt lässt sich rollen, und die Schaltflächen wie „OK“, „Abbrechen“ oder „Schließen“ bleiben sichtbar.
- Wenn das Hauptfenster in einer größeren Stufe angezeigt wird, dann behält es seine gemerkte Größe und Position, und sein Inhalt rollt bei Bedarf.
- Wenn Diagramme (Jahresfortschritt, Höhenprofil) angezeigt werden, dann wachsen ihre Beschriftungen mit der Stufe mit, und sie bleiben bedienbar.
- Wenn eine Karte angezeigt wird (Tourdetail, Tourenplanung), dann bleibt das Kartenbild selbst – Kacheln, Strecke, Wegpunkte und Kartenbeschriftungen – in jeder Stufe so groß wie bei 100 %, während die Bedienelemente um die Karte herum (Leisten, Schaltflächen, Regler, Hinweise) und das Fenster mit der Stufe wachsen.
- Wenn der Benutzer in einer größeren Stufe auf eine Karte klickt, Wegpunkte setzt oder zieht, zoomt oder verschiebt, dann trifft er genau die Stelle unter dem Mauszeiger, wie bei 100 %.
- Wenn Meldungsfenster von Windows erscheinen (Rückfragen, Hinweise), dann folgen sie der Einstellung von Windows und nicht der Stufe von AndyTours.
- Wenn eine Stufe gewählt ist, dann wachsen die gemerkten Spaltenbreiten der Tabellen mit, sodass dieselben Inhalte sichtbar bleiben wie bei 100 %.
- Wenn der Benutzer die Größe ändern will, dann geht das nur über die Einstellungen; es gibt dafür keine Tastenkürzel.

#### Entscheidungen aus dem Plan

- **Speicherung als Prozentzahl:** `Darstellung.Groesse` mit dem Wert `100`…`200`. Nur die sechs Stufen gelten; alles andere ergibt 100. So bleibt ein von Hand veränderter Wert harmlos.
- **Fenster vor dem Laden der Datenbank** (z. B. „Datenordner nicht erreichbar“) erscheinen in 100 %, weil die Stufe erst mit dem Datenbestand bekannt ist.
- **Fenstergrößen umrechnen:** Die Größenangaben im XAML bleiben in Einheiten von 100 %. `Anmelden` rechnet Breite, Höhe, Mindest- und Höchstgröße jedes Fensters außer dem Hauptfenster mit der Stufe um und begrenzt sie auf den Arbeitsbereich; eine neue Stufe führt offene Fenster im Verhältnis neu/alt nach. Das ersetzt die ursprünglich geplante „Breite am Inhalt“, weil so kein Dialog umgebaut werden muss und vom Benutzer veränderte Größen erhalten bleiben.
- **Einstellungen in Karteikarten** (Wunsch des Benutzers bei der Prüfung, das Fenster war zu groß und unübersichtlich): „Allgemein“ (Datenordner mit Hinweis zur Entwicklungsversion, Darstellung), „Datensicherung“, „Tourenplanung“ (Routing, Overlays) und „Auswahllisten“. Feste Höhe 600, damit das Fenster beim Wechsel der Karteikarte nicht springt; jede Karteikarte rollt für sich, „Schließen“ bleibt unten fest. Die Liste der Auswahllisten ist fest 300 hoch.
- **Kontrastfarben** (Wunsch des Benutzers bei der Prüfung, Reiter und Menüpunkte waren im Fluent-Design kaum erkennbar): graue Karteikartenseite mit gleichfarbigem gewähltem Reiter, dunklere nicht gewählte Reiter und Hauptmenüpunkte, weiße Felder und Listen → decided in docs/adr/0008-kontrastfarben-der-oberflaeche.md
- Skalierungsverfahren als verbindliche Regel für alle Fenster: ADR 0007.
- Antworten zur Spec: O1 – die Größe lässt sich nur über die Einstellungen ändern, Tastenkürzel gibt es nicht; P1 – Dialoge, die
  nicht mehr passen, werden höchstens bildschirmgroß und rollen ihren Inhalt (statt verkleinert zu werden), „OK“, „Abbrechen“ und
  „Schließen“ bleiben sichtbar; Q1 – gemerkte Spaltenbreiten wachsen mit der Stufe mit.
- Bei der Umsetzung ergänzt: Aufklappende Elemente (Menüs, Auswahllisten, Kontextmenüs, Tooltips) übernehmen die Vergrößerung
  von ihrem Element, es ist nichts weiter nötig (bei 150 % geprüft).
- Bei der Umsetzung ergänzt: Im Hauptfenster rollt die Karteikarte „Aktuell“ (Diagramm mindestens 250 hoch); die Karteikarte
  „Wartung“ ist mindestens 450 hoch und rollt senkrecht; die Filterleiste der Trainingseinheiten schneidet nichts ab.
- Bei der Umsetzung ergänzt: Alle Dialoge mit bisheriger `StackPanel`-Wurzel (Alarm, Course, Datenordner nicht erreichbar,
  Einheit bearbeiten, Equipment bearbeiten, Equipment-Einsatz, Persönliche Daten, Saisonziele, Wartung) haben jetzt unten eine
  feste Schaltflächenleiste und darüber rollbaren Inhalt; „Fortschritt“ und „Über AndyTours“ blieben unverändert.
- Rundgang bei 100 %, 150 % und 200 % ohne Befund; Klicktreffer auf der Karte vom Benutzer bestätigt.
