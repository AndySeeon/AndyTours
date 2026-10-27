# Plan: 018 – Höhenprofil

## Vorgehen
Eine neue Lesefunktion im Import-Projekt liest aus der FIT-Aufzeichnung den Verlauf: je Messpunkt Distanz, Fahrzeit ohne Pausen, Höhe, Puls, Geschwindigkeit, Trittfrequenz und Temperatur. Die Pausen erkennt sie an den Start- und Stopp-Ereignissen der Zeitmessung des Edge; fehlen diese, läuft die Zeit durchgehend. Die Prüfung auf eine gültige FIT-Datei und die Fehlertexte folgen `LeseFitAufzeichnung`.

Fachfunktionen im Core liefern:
- die Aufzeichnung einer Einheit (Höhenprofil-Anhang, sonst GPS-Track),
- die Kopfzeile des Fensters,
- welche Zusatzwerte im Verlauf vorhanden sind.

Der Befehl „Tourdetail…“ im Hauptfenster ist nur bei genau einer markierten Einheit mit Aufzeichnung wählbar. Er prüft, ob die Datei da ist, liest den Verlauf und öffnet das nicht modale Fenster „Tourdetail“ mit der Karteikarte „Höhenprofil“. Das Diagramm ist ein OxyPlot-Datenmodell im ViewModel (ADR 0001):
- eine Fläche für die Höhe,
- je Zusatzwert eine eigene Linie mit eigener senkrechter Achse,
- eine waagrechte Achse für Distanz bzw. Fahrzeit,
- eine eigene Maussteuerung: linke Taste ziehen = Bereich vergrößern, Doppelklick = alles zeigen, Überfahren = Werte.

## Dateien
- `src/AndyTours.Core/Modell/Verlaufspunkt.cs` (neu) — ein Messpunkt: Distanz (km), Fahrzeit ohne Pausen, Höhe, Puls, Geschwindigkeit (km/h), Trittfrequenz, Temperatur; Zusatzwerte und Höhe optional
- `src/AndyTours.Core/Modell/Verlaufswert.cs` (neu) — Aufzählung der Zusatzwerte: Puls, Geschwindigkeit, Trittfrequenz, Temperatur
- `src/AndyTours.Core/Berechnungen/FindeAufzeichnung.cs` (neu) — relativer Pfad der Aufzeichnung einer Einheit (Höhenprofil vor GPS-Track) oder `null`
- `src/AndyTours.Core/Berechnungen/BeschreibeTourdetail.cs` (neu) — Kopfzeile, z. B. „Sa, 26.09.2026 · Name · 90,83 km · 1.206 hm · höchster Punkt 1.379 m“
- `src/AndyTours.Core/Berechnungen/ErmittleVerlaufswerte.cs` (neu) — vorhandene Zusatzwerte und ob es Höhendaten gibt
- `src/AndyTours.Import/Garmin/LeseFitVerlauf.cs` (neu) — liest die Messpunkte einer FIT-Datei samt Pausen; liefert Verlauf oder Fehlergrund
- `src/AndyTours.App/ViewModels/TourdetailViewModel.cs` (neu) — Kopfzeile, Diagrammmodell, Maussteuerung, Achsenwahl, Häkchen samt Wählbarkeit, Hinweis ohne Höhendaten, Format des Hinweises beim Überfahren
- `src/AndyTours.App/Views/TourdetailFenster.xaml` (neu) — Fenster mit Kopfzeile und Karteikarte „Höhenprofil“: Achsenwahl, Häkchen, Diagramm bzw. Hinweis
- `src/AndyTours.App/Views/TourdetailFenster.xaml.cs` (neu) — Code-Behind
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Tourdetail.cs` (neu) — Befehl „Tourdetail…“: Wählbarkeit, Dateiprüfung, Lesen, Fehlermeldungen, Fenster öffnen
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Einheiten.cs` (ändern) — Markierung meldet die Wählbarkeit auch an „Tourdetail…“
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Menüeintrag „Einheit → Tourdetail…“
- `src/AndyTours.App/Views/EinheitenTabelle.xaml` (ändern) — Eintrag „Tourdetail…“ im Kontextmenü der Zeilen
- `tests/AndyTours.Tests/Garmin/FitTestdatei.cs` (ändern) — Messpunkte mit Distanz, Geschwindigkeit, Trittfrequenz; optional Pause über Zeitmessungsereignisse und Datei ohne Höhe
- `tests/AndyTours.Tests/Garmin/LeseFitVerlaufTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/FindeAufzeichnungTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BeschreibeTourdetailTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErmittleVerlaufswerteTests.cs` (neu)

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Anzeigen
- **Kriterium:** Wenn genau eine Einheit mit Aufzeichnung markiert ist und der Benutzer „Einheit → Tourdetail…“ oder im Kontextmenü der Tabelle „Tourdetail…“ wählt, dann öffnet sich das Fenster „Tourdetail“ mit der Karteikarte „Höhenprofil“.
  - Umsetzung: `HauptfensterViewModel.Tourdetail`, `Hauptfenster.xaml`, `EinheitenTabelle.xaml`, `TourdetailFenster`, `LeseFitVerlauf`
  - Prüfung: `LeseFitVerlaufTests`
  - Manuell: Einheit markieren, beide Wege wählen; das Fenster öffnet sich, das Hauptfenster bleibt bedienbar.
- **Kriterium:** Wenn das Fenster offen ist, dann stehen oben Datum, Name, Distanz, Höhenmeter und höchster Punkt der Einheit.
  - Umsetzung: `BeschreibeTourdetail`, `TourdetailViewModel`
  - Prüfung: `BeschreibeTourdetailTests`
- **Kriterium:** Wenn das Höhenprofil angezeigt wird, dann zeigt die waagrechte Achse die Distanz in km und die senkrechte Achse die Höhe in m.
  - Umsetzung: `LeseFitVerlauf` (Distanz in km, Höhe in m), `TourdetailViewModel`
  - Prüfung: `LeseFitVerlaufTests`
  - Manuell: Achsenbeschriftung „km“ und „m“; das Profil passt zum höchsten Punkt der Kopfzeile.
- **Kriterium:** Wenn der Benutzer die Achse auf „Fahrzeit“ umschaltet, dann zeigt die waagrechte Achse die Fahrzeit; Pausen werden dabei herausgelassen.
  - Umsetzung: `LeseFitVerlauf` (Fahrzeit ohne Pausen), `TourdetailViewModel`
  - Prüfung: `LeseFitVerlaufTests` (Testdatei mit Pause)
  - Manuell: Umschalten; das Ende der Achse entspricht der Fahrzeit der Einheit.
- **Kriterium:** Wenn der Benutzer bei Puls, Geschwindigkeit, Trittfrequenz oder Temperatur ein Häkchen setzt, dann erscheint dieser Wert als zusätzliche Linie mit eigener Skala; ohne Häkchen verschwindet sie wieder.
  - Umsetzung: `TourdetailViewModel`
  - Prüfung: `LeseFitVerlaufTests` (Werte je Messpunkt)
  - Manuell: Häkchen setzen und entfernen.
- **Kriterium:** Wenn die Aufzeichnung einen dieser Werte nicht enthält, dann ist sein Häkchen nicht wählbar.
  - Umsetzung: `ErmittleVerlaufswerte`, `TourdetailViewModel`
  - Prüfung: `ErmittleVerlaufswerteTests`
- **Kriterium:** Wenn der Benutzer mit der Maus über das Profil fährt, dann erscheinen an dieser Stelle Distanz, Fahrzeit, Höhe und die Werte der eingeblendeten Linien.
  - Umsetzung: `TourdetailViewModel` (Maussteuerung, Hinweisformat nach eingeblendeten Werten)
  - Manuell: über das Profil fahren, mit und ohne Häkchen.
- **Kriterium:** Wenn der Benutzer mit der Maus einen Bereich aufzieht, dann wird dieser Bereich vergrößert; ein Doppelklick zeigt wieder die ganze Tour.
  - Umsetzung: `TourdetailViewModel` (Maussteuerung)
  - Manuell: Bereich aufziehen, dann doppelklicken.
- **Kriterium:** Wenn der Benutzer doppelt auf eine Zeile der Tabelle klickt, dann öffnet sich wie bisher das Bearbeitungsfenster.
  - Umsetzung: unverändert (`EinheitenTabelle`)
  - Manuell: Doppelklick auf eine Zeile.

### Grenz- und Fehlerfälle
- **Kriterium:** Wenn keine oder mehrere Einheiten markiert sind, dann ist „Tourdetail…“ nicht wählbar.
  - Umsetzung: `HauptfensterViewModel.Tourdetail` (Wählbarkeit), `HauptfensterViewModel.Einheiten`
  - Manuell: keine bzw. zwei Einheiten markieren; Menü und Kontextmenü sind grau.
- **Kriterium:** Wenn die markierte Einheit keine Aufzeichnung hat, dann ist „Tourdetail…“ nicht wählbar.
  - Umsetzung: `FindeAufzeichnung`, `HauptfensterViewModel.Tourdetail`
  - Prüfung: `FindeAufzeichnungTests`
- **Kriterium:** Wenn die Datei der Aufzeichnung fehlt, dann erscheint der Hinweis „Die Aufzeichnung ‚2019/2019-11-11-06-25-39.fit‘ wurde im Datenordner nicht gefunden.“ mit dem Namen der fehlenden Datei.
  - Umsetzung: `HauptfensterViewModel.Tourdetail`
  - Manuell: Tourdetail der Einheit vom 11.11.2019 öffnen.
- **Kriterium:** Wenn die Aufzeichnung keine Höhendaten enthält (z. B. eine Fahrt auf der Rolle), dann erscheint in der Karteikarte „Die Aufzeichnung enthält keine Höhendaten.“
  - Umsetzung: `ErmittleVerlaufswerte`, `TourdetailViewModel`, `TourdetailFenster`
  - Prüfung: `ErmittleVerlaufswerteTests`, `LeseFitVerlaufTests` (Datei ohne Höhe)
- **Kriterium:** Wenn die Aufzeichnung nicht gelesen werden kann, dann erscheint der Hinweis „Die Aufzeichnung konnte nicht gelesen werden.“ mit dem Grund.
  - Umsetzung: `LeseFitVerlauf`, `HauptfensterViewModel.Tourdetail`
  - Prüfung: `LeseFitVerlaufTests` (beschädigte Datei, keine FIT-Datei)

## Entscheidungen
- Das Fenster ist nicht modal (Wahl G1 A1 des Benutzers): Das Hauptfenster bleibt bedienbar, mehrere Tourdetails können offen sein. Die Anzeige ist der Stand beim Öffnen.
- Pausen kommen aus den Start- und Stopp-Ereignissen der Zeitmessung, wie die gestoppte Zeit im Garmin-Import (017). So endet die Fahrzeit-Achse bei der Fahrzeit der Einheit.
- Messpunkte ohne Distanz übernehmen die letzte bekannte Distanz; Messpunkte ohne Zeitstempel werden übergangen.
- Der Verlauf wird beim Öffnen gelesen und nicht gespeichert; die FIT-Datei bleibt die einzige Quelle.

## Risiken
- Eine lange Tour hat über 20.000 Messpunkte. Erscheint das Diagramm träge, werden die Punkte für die Anzeige ausgedünnt; die Werte beim Überfahren bleiben genau.
- Der Hinweis beim Überfahren soll die Werte aller eingeblendeten Linien zeigen. OxyPlot zeigt von sich aus nur die Werte einer Linie; das Format wird deshalb nach den Häkchen zusammengesetzt.
