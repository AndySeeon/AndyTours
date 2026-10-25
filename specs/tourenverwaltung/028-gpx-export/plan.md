# Plan: 028 – Track als GPX exportieren

## Vorgehen
Zwei neue Fachfunktionen im Core:

- `ErstelleGpx` erzeugt aus den Etappen des Tourdetails den Text einer GPX-1.1-Datei (mit `System.Xml.Linq` aus der Standardbibliothek). Jede Etappe mit Positionen wird ein Track (`trk`) mit dem Namen ihrer Einheit. Seine Punkte (`trkpt`) tragen Breite, Länge und Höhe (`ele`, falls vorhanden), aber keine Uhrzeit. Punkte ohne Position werden übergangen, ebenso Etappen ohne Positionen. Die Reihenfolge der Etappen bleibt erhalten; sie ist schon nach Datum und Startzeit geordnet (`OrdneEtappen`). Hat keine Etappe Positionen, liefert die Funktion `null`.
- `BildeGpxDateiname` schlägt den Dateinamen vor, z. B. „2026-05-01 Bründlingalm.gpx“. Bei mehreren Etappen lautet er „2026-05-01 bis 2026-05-04 Alpencross.gpx“, mit dem Namen der ersten Etappe. In Dateinamen unzulässige Zeichen werden durch „-“ ersetzt.

Die Schaltfläche „Als GPX exportieren…“ sitzt auf der Karteikarte „Karte“ neben „Ganze Strecke“. Der Befehl dazu liegt im `StreckenkarteViewModel` und ist nur wählbar, wenn es eine Strecke gibt (`HatStrecke`).

Der Befehl öffnet einen Speichern-Dialog von WPF mit Ordner und Dateinamen. Das Rückfragen beim Überschreiben übernimmt der Dialog selbst. Dann schreibt der Befehl die Datei in UTF-8 und zeigt neben der Schaltfläche „Track exportiert: <Dateiname>“. Bei einem Schreibfehler erscheint eine Meldung „Der Track konnte nicht exportiert werden: <Grund>“.

Den zuletzt verwendeten Ordner speichert eine neue Einstellung `GpxOrdnerEinstellung` in der Tabelle `Einstellung`. Das Hauptfenster reicht Laden und Speichern beim Öffnen des Tourdetails als Funktionen durch `TourdetailViewModel` an `StreckenkarteViewModel` weiter. Ein Fehler beim Speichern dieses Ordners bleibt ohne Meldung (`SpeichereStill`), wie bei Fensterposition und Spaltenanordnung.

## Dateien
- `src/AndyTours.Core/Berechnungen/ErstelleGpx.cs` (neu) — GPX-Text aus den Etappen
- `src/AndyTours.Core/Berechnungen/BildeGpxDateiname.cs` (neu) — vorgeschlagener Dateiname
- `src/AndyTours.Daten/Einstellungsschluessel.cs` (ändern) — Schlüssel `GpxExport.Ordner`
- `src/AndyTours.Daten/GpxOrdnerEinstellung.cs` (neu) — zuletzt verwendeten Ordner laden und speichern
- `src/AndyTours.App/ViewModels/StreckenkarteViewModel.cs` (ändern) — Befehl „Als GPX exportieren…“, Hinweis nach dem Speichern
- `src/AndyTours.App/ViewModels/TourdetailViewModel.cs` (ändern) — Laden und Speichern des Ordners an die Streckenkarte weiterreichen
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Tourdetail.cs` (ändern) — Laden und Speichern des Ordners übergeben
- `src/AndyTours.App/Views/TourdetailFenster.xaml` (ändern) — Schaltfläche und Hinweis neben „Ganze Strecke“
- `tests/AndyTours.Tests/Berechnungen/ErstelleGpxTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BildeGpxDateinameTests.cs` (neu)
- `tests/AndyTours.Tests/Daten/GpxOrdnerTests.cs` (neu)

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Exportieren
- **Kriterium:** Wenn das Tourdetail geöffnet ist, dann gibt es auf der Karteikarte „Karte“ neben „Ganze Strecke“ die Schaltfläche „Als GPX exportieren…“.
  - Umsetzung: `TourdetailFenster.xaml`
  - Manuell: Tourdetail öffnen, Karteikarte „Karte“.
- **Kriterium:** Wenn „Als GPX exportieren…“ gewählt wird, dann öffnet sich ein Speichern-Dialog mit dem zuletzt dafür verwendeten Ordner und dem Dateinamen aus Datum und Name der Einheit, z. B. „2026-05-01 Bründlingalm.gpx“.
  - Umsetzung: `StreckenkarteViewModel` (Dialog), `BildeGpxDateiname`, `GpxOrdnerEinstellung`
  - Prüfung: `BildeGpxDateinameTests`, `GpxOrdnerTests`
  - Manuell: exportieren, App neu starten, erneut exportieren → derselbe Ordner ist vorgeschlagen.
- **Kriterium:** Wenn gespeichert wird, dann enthält die Datei die Strecke der Aufzeichnung mit Position und Höhe jedes Messpunkts, ohne Uhrzeiten; der Track trägt den Namen der Einheit.
  - Umsetzung: `ErstelleGpx`
  - Prüfung: `ErstelleGpxTests` (kein `time`-Element)
- **Kriterium:** Wenn die Datei in einem anderen Programm geöffnet wird (z. B. Garmin BaseCamp, Komoot, ein GPS-Viewer), dann zeigt es dieselbe Strecke wie die Karte im Tourdetail.
  - Umsetzung: `ErstelleGpx` (gültiges GPX 1.1 mit Namensraum `http://www.topografix.com/GPX/1/1`)
  - Prüfung: `ErstelleGpxTests` (Punkte der Datei = Punkte der Streckenkarte laut `ErmittleStrecke`; Datei lässt sich als XML lesen)
  - Manuell: exportierte Datei in einem GPS-Viewer oder auf gpx.studio öffnen und mit der Karte vergleichen.
- **Kriterium:** Wenn das Speichern gelungen ist, dann erscheint neben der Schaltfläche der Hinweis „Track exportiert: 2026-05-01 Bründlingalm.gpx“.
  - Umsetzung: `StreckenkarteViewModel` (Hinweis), `TourdetailFenster.xaml`
  - Manuell: Sichtprüfung nach dem Speichern.

### Mehrtagestour
- **Kriterium:** Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann speichert „Als GPX exportieren…“ alle Etappen in eine Datei, jede Etappe als eigener Track mit dem Namen ihrer Einheit, nach Datum und Startzeit geordnet.
  - Umsetzung: `ErstelleGpx` (ein `trk` je Etappe in der Reihenfolge aus `OrdneEtappen`)
  - Prüfung: `ErstelleGpxTests`
  - Manuell: drei Etappen markieren, Tourdetail, exportieren; im Viewer drei Tracks.
- **Kriterium:** Wenn eine Mehrtagestour exportiert wird, dann lautet der vorgeschlagene Dateiname „2026-05-01 bis 2026-05-04 Alpencross.gpx“ mit dem ersten und letzten Datum und dem Namen der ersten Etappe.
  - Umsetzung: `BildeGpxDateiname`
  - Prüfung: `BildeGpxDateinameTests`
- **Kriterium:** Wenn einzelne Etappen keine GPS-Daten haben, dann fehlen sie in der Datei; die übrigen Etappen werden exportiert.
  - Umsetzung: `ErstelleGpx`
  - Prüfung: `ErstelleGpxTests`

### Grenz- und Fehlerfälle
- **Kriterium:** Wenn keine der angezeigten Aufzeichnungen GPS-Daten enthält (z. B. eine Fahrt auf der Rolle), dann ist „Als GPX exportieren…“ nicht wählbar.
  - Umsetzung: Befehl mit `CanExecute = HatStrecke` in `StreckenkarteViewModel`; `ErstelleGpx` liefert dann `null`
  - Prüfung: `ErstelleGpxTests` (ohne Positionen → `null`)
  - Manuell: Tourdetail einer Rollenfahrt öffnen. Die Schaltfläche sitzt in der Leiste, die ohne Strecke ausgeblendet ist; sie ist dort also nicht erreichbar.
- **Kriterium:** Wenn einzelne Messpunkte keine Position haben (z. B. im Tunnel), dann fehlen sie in der Datei; die übrigen Punkte bleiben erhalten.
  - Umsetzung: `ErstelleGpx`
  - Prüfung: `ErstelleGpxTests`
- **Kriterium:** Wenn ein Zeichen im Namen der Einheit in Dateinamen nicht erlaubt ist (z. B. „/“), dann wird es im vorgeschlagenen Dateinamen durch „-“ ersetzt.
  - Umsetzung: `BildeGpxDateiname`
  - Prüfung: `BildeGpxDateinameTests`
- **Kriterium:** Wenn die gewählte Datei schon existiert, dann fragt Windows wie üblich, ob sie ersetzt werden soll.
  - Umsetzung: Speichern-Dialog mit `OverwritePrompt` (Standard)
  - Manuell: zweimal in dieselbe Datei exportieren.
- **Kriterium:** Wenn der Dialog abgebrochen wird, dann wird nichts geschrieben und kein Hinweis angezeigt.
  - Umsetzung: `StreckenkarteViewModel` (Abbruch beendet den Befehl vor dem Schreiben)
  - Manuell: Dialog mit „Abbrechen“ schließen.
- **Kriterium:** Wenn die Datei nicht geschrieben werden kann (z. B. Ordner schreibgeschützt), dann erscheint der Hinweis „Der Track konnte nicht exportiert werden: …“ mit dem Grund.
  - Umsetzung: `StreckenkarteViewModel` (fängt `IOException` und `UnauthorizedAccessException`)
  - Manuell: in einen schreibgeschützten Ordner exportieren, z. B. `C:\Windows`.

## Entscheidungen
- Der GPX-Text entsteht im Core als reine Funktion und ist so testbar; nur Dialog und Schreiben liegen in der App. Eine zusätzliche Bibliothek ist nicht nötig.
- Ohne Uhrzeiten (Wunsch des Benutzers): GPX 1.1 verlangt je Punkt nur Breite und Länge, die Datei bleibt normkonform. Damit bleiben `Verlaufspunkt` und `LeseFitVerlauf` unverändert.
- Der zuletzt verwendete Ordner wird im Datenordner gespeichert und gilt damit auch nach einem Neustart. Ist noch keiner gespeichert oder gibt es ihn nicht mehr, schlägt der Dialog den Ordner „Dokumente“ vor.
- Liegen bei einer Mehrtagestour erstes und letztes Datum auf demselben Tag, enthält der Dateiname nur dieses eine Datum.
- Das Datum im Dateinamen stammt von allen Etappen des Tourdetails, auch von solchen ohne GPS-Daten, damit er zur Kopfzeile passt.

