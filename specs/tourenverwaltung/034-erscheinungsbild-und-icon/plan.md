# Plan: 034 – Erscheinungsbild und Programmicon

## Vorgehen
Das Erscheinungsbild entsteht anwendungsweit an einer Stelle: `App.xaml` schaltet das in WPF (.NET 10) eingebaute Fluent-Design fest auf „hell“ (`ThemeMode="Light"`), unabhängig vom Windows-Modus. Die Steuerelemente folgen damit der Akzentfarbe von Windows. Die vorhandenen Stile der Fenster bleiben; nur wo das Fluent-Design sie überdeckt (Zeilenfarben der Alarme, Summenzeile, „Filter zurücksetzen“) oder wo größere Schrift und Abstände Inhalte abschneiden, werden die Fenster angepasst.

Das Programmicon wird einmal als Vektorzeichnung in XAML beschrieben (große Fassung und vereinfachte Fassung für 16–24 px). Ein Skript rendert daraus die Icon-Datei mit allen Größen; die fertige `.ico` liegt im Repository, damit Bauen und Installationsprogramm ohne das Skript auskommen. App, Umwandlungsprogramm und Installationsprogramm binden dieselbe Datei ein. Fenster ohne eigenes Icon übernehmen das Icon der Programmdatei. „Hilfe → Über AndyTours“ wird ein eigenes kleines Fenster, das die Vektorzeichnung groß zeigt.

## Dateien
- `src/AndyTours.App/App.xaml` (ändern) — `ThemeMode="Light"`, Ressourcen-Wörterbuch mit der Icon-Zeichnung einbinden
- `src/AndyTours.App/AndyTours.App.csproj` (ändern) — `ApplicationIcon`, Icon als Ressource; `WPF0001` mit Begründung in `NoWarn`
- `src/AndyTours.App/Ressourcen/Programmicon.xaml` (neu) — Vektorzeichnung des Icons (`DrawingImage` groß und klein), einzige Quelle des Motivs
- `src/AndyTours.App/Ressourcen/AndyTours.ico` (neu) — erzeugte Icon-Datei (16, 20, 24, 32, 40, 48, 64, 256 px)
- `tools/AndyTours.Programmicon/Erstelle-Programmicon.ps1` (neu) — rendert `Programmicon.xaml` in alle Größen und schreibt `AndyTours.ico`
- `src/AndyTours.App/Views/UeberFenster.xaml` (neu) — Fenster „Über AndyTours“: großes Icon, Name, Version, Autor, „OK“
- `src/AndyTours.App/Views/UeberFenster.xaml.cs` (neu) — Code-Behind, nur Anzeige
- `src/AndyTours.App/ViewModels/UeberViewModel.cs` (neu) — Name, Version und Autor aus den Angaben der Programmdatei
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — `Ueber()` öffnet `UeberFenster` statt der Meldung
- `src/AndyTours.App/Views/WartungenAnsicht.xaml` (ändern) — Zeilenfarben der Alarme so setzen, dass das Fluent-Design sie nicht überdeckt
- `src/AndyTours.App/Views/EinheitenTabelle.xaml` (ändern) — Summenzeile und „Filter zurücksetzen“ an das Fluent-Design anpassen
- `src/AndyTours.App/Views/EinheitenTabelle.xaml.cs` (ändern) — Mindestbreite der Summenspalten mit der Schrift und dem Zellabstand des Fluent-Designs messen
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Menü „Hilfe“ unverändert, gelbe Statusmeldung prüfen und angleichen
- Übrige Fenster in `src/AndyTours.App/Views/` (ändern, nur soweit die Prüfung es zeigt) — Breiten, Höhen und Mindestgrößen, wo Text abgeschnitten wird: `AlarmBearbeitenFenster`, `CourseUebertragenFenster`, `DatenordnerNichtErreichbarFenster`, `EinheitBearbeitenFenster`, `EinstellungenFenster`, `EintragWaehlenFenster`, `EquipmentBearbeitenFenster`, `EquipmentEinsatzFenster`, `FortschrittsFenster`, `GarminImportFenster`, `KategorienZuordnenFenster`, `OverlaysFenster`, `PersoenlicheDatenFenster`, `SaisonzieleFenster`, `SicherungAuswahlFenster`, `TourdetailFenster`, `TourenplanungFenster`, `WartungBearbeitenFenster`, `ZusammenfuehrenFenster`, `AktuellAnsicht`
- `tools/AndyTours.Altdatenuebernahme/AndyTours.Altdatenuebernahme.csproj` (ändern) — `ApplicationIcon` auf dieselbe `.ico`
- `tools/AndyTours.Installation/AndyTours.iss` (ändern) — `SetupIconFile` auf dieselbe `.ico`; `UninstallDisplayIcon` bleibt die Programmdatei
- `Directory.Build.props` (ändern) — `Copyright` „© Andreas Hensel“, damit der Autor in der Programmdatei steht und das Über-Fenster ihn lesen kann
- `docs/tech.md` (ändern) — Erscheinungsbild (Fluent, hell), `Copyright` in `Directory.Build.props`, Befehl zum Erzeugen des Icons
- `docs/structure.md` (ändern) — `src/AndyTours.App/Ressourcen/` und `tools/AndyTours.Programmicon/`

## Abdeckung der Kriterien

### Erscheinungsbild
- Wenn ein Fenster von AndyTours geöffnet wird, dann zeigt es das moderne Erscheinungsbild von Windows 11: abgerundete Schaltflächen, Eingabefelder und Auswahllisten sowie moderne Menüs, Karteikarten, Tabellen und Bildlaufleisten – in allen Fenstern der App. → `App.xaml` (`ThemeMode`); manuelle Prüfung: jedes der 22 Fenster und Ansichten einmal öffnen (Rundgang siehe unten).
- Wenn Windows auf den dunklen Modus eingestellt ist, dann bleibt AndyTours trotzdem hell. → `App.xaml` (`ThemeMode="Light"` statt `System`); manuelle Prüfung: Windows auf „Dunkel“ stellen, App starten, Hauptfenster und Einstellungen ansehen.
- Wenn ein Element markiert, gewählt oder mit der Tastatur angesteuert ist, dann ist es in der Akzentfarbe von Windows deutlich erkennbar. → Fluent-Design; manuelle Prüfung: Zeile der Einheitentabelle markieren, mit Tab durch das Bearbeitungsfenster gehen, Akzentfarbe in Windows wechseln und Wirkung nach Neustart sehen.
- Wenn ein Fenster in seiner Anfangsgröße oder in seiner Mindestgröße geöffnet wird, dann sind alle Beschriftungen, Felder und Schaltflächen vollständig sichtbar, ohne abgeschnittenen Text. → Größen in den betroffenen Fenstern; manuelle Prüfung im Rundgang, Hauptfenster und Tourdetail zusätzlich auf Mindestgröße ziehen.
- Wenn das Hauptfenster mit der gemerkten Größe und Position öffnet, dann zeigt es wie bisher die gemerkten Spaltenbreiten, die Spaltenreihenfolge und die zuletzt gewählte Karteikarte. → unveränderte Speicherung aus 006; manuelle Prüfung: Spalte verschieben und verbreitern, Karteikarte „Wartung“ wählen, beenden, neu starten.
- Wenn die App eine Rückfrage oder einen Hinweis zeigt (z. B. „Änderungen verwerfen?“), dann erscheint wie bisher das Meldungsfenster von Windows. → keine Änderung an den `MessageBox`-Aufrufen; manuelle Prüfung: Einheit ändern und mit „Abbrechen“ schließen.

### Bestehende Hervorhebungen bleiben erkennbar
- Wenn die Tabelle der Trainingseinheiten Bestwerte enthält, dann sind diese weiter rot. → Stile `Best…` in `EinheitenTabelle.xaml`; manuelle Prüfung: Jahr 2026 wählen, rote Werte bei Distanz, hm, Fahrzeit, Ø und max km/h sehen, auch in markierter Zeile.
- Wenn Alarme fällig, bald fällig oder ausgeschaltet sind, dann sind ihre Zeilen weiter rot, orange bzw. grau, auch wenn sie markiert sind. → `WartungenAnsicht.xaml`; manuelle Prüfung: Testalarm mit kleinem Intervall (fällig), mit knapp unterschrittenem Intervall (bald fällig) und ausgeschaltet anlegen, jeweils markiert und unmarkiert ansehen.
- Wenn ein Eingabefeld ungültig ist, dann ist es weiter rot markiert, und ein Hinweistext darunter ist weiter rot. → Stile `Fehlerrahmen`, `EinheitenTabelle.xaml.cs` (Zellprüfung); manuelle Prüfung: „abc“ als Distanz im Bearbeitungsfenster und in einer Tabellenzelle, Größe „20“ in den persönlichen Daten.
- Wenn ein Filter gesetzt ist, dann ist „Filter zurücksetzen“ weiter sichtbar hervorgehoben. → `FilterZuruecksetzenStil` in `EinheitenTabelle.xaml`; manuelle Prüfung: Suchbegriff eingeben, Schaltfläche ist gelb und fett, auch beim Überfahren mit der Maus.
- Wenn das laufende Jahr einen Rekord hält, dann ist dieser auf der Karteikarte „Aktuell“ weiter hervorgehoben. → unveränderte Hervorhebung in `AktuellAnsicht.xaml`; manuelle Prüfung: Testeinheit mit Rekorddistanz im laufenden Jahr anlegen, Zeile fett und blau, danach löschen.
- Wenn Diagramme (Jahresfortschritt, Höhenprofil) und Karten angezeigt werden, dann sind sie unverändert lesbar und bedienbar. → keine Änderung an OxyPlot und Mapsui; manuelle Prüfung: Klick ins Jahresdiagramm, Höhenprofil mit Puls, Bereich aufziehen, Karte zoomen und verschieben, Tourenplanung mit zwei Punkten.

### Bedienung unverändert
- Wenn der Benutzer die App mit der Tastatur bedient, dann funktionieren Zugriffstasten (unterstrichene Buchstaben), Tastenkürzel, die Tab-Reihenfolge sowie Enter und Esc in den Fenstern wie bisher. → keine Änderung an Bindungen; manuelle Prüfung: Alt+E, Strg+N, Tab durch das Bearbeitungsfenster, Enter speichert, Esc schließt.
- Wenn der Benutzer eine Funktion der App aufruft, dann verhält sie sich wie vorher; nur das Aussehen hat sich geändert. → geprüft durch `dotnet test AndyTours.slnx` (alle vorhandenen Tests grün) und den Rundgang.

### Programmicon
- Wenn AndyTours läuft, dann zeigen die Titelleiste jedes Fensters, die Taskleiste und die Fensterumschaltung mit Alt+Tab das Programmicon: ein stilisiertes Fahrrad mit orangem Rahmen und dunklen Rädern, Lenker und Sattel vor grünen Bergen mit weißen Gipfeln und blauem Himmel auf einem abgerundeten Quadrat. → `Programmicon.xaml`, `AndyTours.ico`, `ApplicationIcon` im App-Projekt; manuelle Prüfung: App starten, Titelleiste von Hauptfenster und Tourdetail, Taskleiste, Alt+Tab.
- Wenn das Icon klein dargestellt wird (Titelleiste, Taskleiste), dann sind Fahrrad und Berge noch erkennbar. → vereinfachte kleine Fassung in `Programmicon.xaml`; manuelle Prüfung: Titelleiste bei 100 % Windows-Skalierung, Explorer-Ansicht „Kleine Symbole“.
- Wenn die Programmdatei von AndyTours im Explorer angezeigt wird, dann trägt sie das Icon. → `ApplicationIcon`; manuelle Prüfung: `bin\Debug\…\AndyTours.App.exe` im Explorer.
- Wenn AndyTours installiert ist, dann tragen die Verknüpfungen im Startmenü und auf dem Desktop sowie der Eintrag in den Windows-Einstellungen unter „Apps“ das Icon. → Verknüpfungen und `UninstallDisplayIcon` zeigen auf die Programmdatei; manuelle Prüfung: Installation in der Windows-Sandbox mit Test-Datenordner.
- Wenn die Datei des Installationsprogramms im Explorer angezeigt wird oder das Installationsprogramm läuft, dann zeigt es das Icon. → `SetupIconFile` in `AndyTours.iss`; manuelle Prüfung: `AndyTours-Setup-<Version>.exe` im Explorer und beim Start in der Sandbox.
- Wenn die Datei des Umwandlungsprogramms für die BikeXperience-Daten im Explorer angezeigt wird, dann trägt sie dasselbe Icon. → `ApplicationIcon` im Umwandlungsprogramm; manuelle Prüfung: `bin\Werkzeug\AndyTours.Altdatenuebernahme.exe` nach dem Bauskript.

### Über AndyTours
- Wenn der Benutzer „Hilfe → Über AndyTours“ wählt, dann öffnet sich ein eigenes kleines Fenster mit dem Icon in großer Darstellung, dem Namen „AndyTours“, der Versionsnummer und dem Autor. → `UeberFenster`, `UeberViewModel`, `HauptfensterViewModel.Ueber`; manuelle Prüfung: Menü aufrufen, Version 1.0.0 und „Andreas Hensel“ sehen.
- Wenn das Fenster „Über AndyTours“ offen ist, dann schließt es sich mit „OK“, Enter oder Esc. → `IsDefault`/`IsCancel` der Schaltfläche; manuelle Prüfung: alle drei Wege.

### Rundgang (manuelle Prüfung aller Fenster)
Hauptfenster mit allen drei Karteikarten, Einstellungen (alle Bereiche), Neue Einheit, Kategorien zuordnen, Eintrag wählen, Zusammenführen, Equipment bearbeiten, Persönliche Daten, Saisonziele, Equipment-Einsatz, Sicherung wiederherstellen, Wartung und Alarm bearbeiten, Garmin-Import (ohne Gerät: Hinweis), Tourdetail einzeln und mehrtägig, Tourenplanung, Overlays, Course übertragen, Fortschritt (Datenordner kopieren in der fertigen App), Datenordner nicht erreichbar (fertige App mit nicht vorhandenem Ordner in der Merkdatei eines Test-Datenordners), Über AndyTours.

## Entscheidungen
- **Eine Quelle für das Motiv:** Das Icon ist eine Vektorzeichnung in XAML. Sie dient zugleich dem Über-Fenster (scharf in jeder Größe) und als Vorlage für die `.ico`. So gibt es kein zusätzliches Grafikprogramm und kein zweites Motiv, das auseinanderlaufen kann.
- **Kleine Fassung:** Für 16–24 px gibt es eine vereinfachte Zeichnung (weniger Speichen, kräftigere Linien, ein Berg), damit das Icon in der Titelleiste erkennbar bleibt.
- **`.ico` im Repository:** Das Skript läuft nur, wenn sich das Motiv ändert. Bauen und Installationsprogramm brauchen es nicht.
- **Icon der Fenster:** Die Fenster bekommen kein eigenes `Icon`; WPF zeigt dann das Icon der Programmdatei. Dialoge zeigen es so ebenfalls.
- **Autor:** Er kommt als `Copyright` aus `Directory.Build.props` in die Programmdatei, wie die Version. So steht er auch in den Dateieigenschaften im Explorer.
- **Meldungsfenster:** bleiben Windows-Standard (Spec, Antwort K1).

## Offene Entscheidungen
- Wird das eingebaute Fluent-Design (`ThemeMode`, in .NET 10 noch als experimentell gekennzeichnet) die verbindliche Grundlage der Oberfläche, nach der sich auch künftige Fenster richten? → decided in docs/adr/0006-grundlage-der-oberflaeche.md

## Risiken
- `ThemeMode` ist experimentell (`WPF0001`). Ändert Microsoft die Schnittstelle, muss die eine Zeile in `App.xaml` angepasst werden; ersatzweise lässt sich das Fluent-Wörterbuch `Fluent.Light.xaml` direkt einbinden.
- Das Fluent-Design hat größere Schrift (14 statt 12 pt) und größere Innenabstände. Fenster mit fester Breite (z. B. Einstellungen, Bearbeitungsfenster) können zu schmal werden; das zeigt erst der Rundgang.
- Das Fluent-Design des `DataGrid` setzt eigene Hintergründe für Zeilen und Zellen. Die Zustandsfarben der Alarme und die gelbe Hervorhebung von „Filter zurücksetzen“ könnten überdeckt werden und müssen dann an der Zelle bzw. in einer eigenen Vorlage gesetzt werden.
- Zeigt WPF bei Fenstern ohne eigenes `Icon` wider Erwarten nicht das Icon der Programmdatei, bekommt jedes Fenster `Icon` über die Ressource gesetzt (eine Zeile je Fenster).
- OxyPlot und Mapsui bringen ihre eigenen Darstellungen mit und werden vom Fluent-Design nicht erfasst; sie können neben den neuen Steuerelementen etwas anders wirken.
