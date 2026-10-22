# BikeXperience → RadlDB: Analyse und Plan für die Neuprogrammierung in C#

Stand: 18.10.2026 · analysiert: `bikeXperience.exe` (28,3 MB), Programmversion 6.0.4 (laut `bikexperience.ini`), Handbuch-Version 5.7.x

---

## 1. Was steckt in der EXE?

| Befund | Detail |
|---|---|
| Wrapper | **Launch4j** – eine kleine native Startdatei (~365 KB), die nur eine installierte Java-Laufzeit sucht und startet |
| Eigentliches Programm | ein angehängtes **JAR-Archiv** ab Byte 374 272 (≈ 28 MB) |
| Build | Maven, JDK 23, Startklasse `de.bikexperience.view.BikeXperience` |
| Eigener Code | Paket `de.bikexperience.*`, **ca. 330 Klassen** (ohne innere Klassen) in 27 Paketen |
| Fremdbibliotheken | JFreeChart (Diagramme), JGoodies/SwingX/L2FProd (Oberfläche), RxTx (serielle Schnittstelle), Dropbox API v2, Garmin ANT+ FIT SDK, JDOM (XML), Tango-Icons, OpenStreetMap-Kacheln |
| Oberfläche | Java Swing, Sprachen DE/EN/NL/RU (Property-Dateien) |
| Autoren | Andreas Günther & Hans-Joachim Willi (bikeXperience.de), Donateware |

Für die Analyse wurde das JAR herausgelöst, die Klassensignaturen mit `javap` ausgelesen und das mitgelieferte deutsche Benutzerhandbuch (`BxpUserGuide_de.html`) ausgewertet. Ein Quellcode-Decompiler wurde **nicht** verwendet; für einzelne Rechenformeln (siehe Abschnitt 6) sollte das bei der Umsetzung gezielt nachgeholt werden.

---

## 2. Ihre vorhandenen Daten (Ist-Zustand im Unterordner `Daten/`)

| Datei | Inhalt | Umfang |
|---|---|---|
| `workouts.csv` | **Hauptdatenbank der Trainingseinheiten** | 932 Einheiten (2016–2026) |
| `2016/` … `2026/` | angehängte Aufzeichnungen, ausschließlich Garmin **.fit** | 930 Dateien |
| `bikexperience.ini` | sämtliche Einstellungen: Aktivitäten, Kategorien, Fahrer, Pulszonen, Schwellwerte, Farben, **18 Equipments** (Räder, Laufschuhe) | 12 KB |
| `maintanence.dat` | Wartungseinträge und Alarme (Schreibweise im Original so) | ~90 Zeilen |
| `planning.dat` | Tourenplanung | **leer** |
| `train.ini` | Trainingsplan | **leer** (nur Kopfzeile) |

**Folgerung:** Tourenplanung und Trainingsplan nutzen Sie offenbar nicht; Ihre Datenquelle ist ausschließlich Garmin-FIT. Das verkleinert den sinnvollen Umfang der Neuprogrammierung erheblich.

### 2.1 Format `workouts.csv` (aus `TourEntry.out()` ermittelt)

Erste Zeile = Anzahl der Einheiten. Danach eine Zeile pro Einheit, Trennzeichen `;`, Texte in `'…'`, Dezimalpunkt `.`:

| Nr. | Feld (Java) | Bedeutung | Beispiel |
|---|---|---|---|
| 1 | `date` | Datum | `05.03.2016` |
| 2 | `name` | Tourname | `2016-03-05-08-40-41` |
| 3 | `tm` | Fahrzeit hh:mm:ss | `04:33:23` |
| 4 | `dst` | Distanz km | `77.83` |
| 5 | `hm` | Höhenmeter | `859` |
| 6 | `maxhm` | höchster Punkt (m) | `970` |
| 7 | `maxSpeed` | Höchstgeschwindigkeit | `53.07` |
| 8 | `heartrate` | Ø Herzfrequenz | `135` |
| 9 | `equipment` | Equipment (Rad/Schuh) | `Namur` |
| 10 | `trainevent` | Trainingsbereich | `WSA` |
| 11 | `starthm` | Starthöhe | `538` |
| 12 | `avcc` | Ø Trittfrequenz | `83` |
| 13 | `temp` | Ø Temperatur | `5` |
| 14 | `maxheartrate` | max. Herzfrequenz | `166` |
| 15 | `colKey` | Farbschlüssel | ` ` |
| 16 | `starttime` | Startzeit | `08:40` |
| 17 | `activity` | Aktivität | `Bike` |
| 18 | `category` | Kategorien, durch `,` getrennt | `Training` |
| 19 | `weight` | Körpergewicht zum Zeitpunkt | `95.0` |
| 20 | `minheartrate` | min. Herzfrequenz | `91` |
| 21 | `avPower` | Ø Leistung (W) | `150.0` |
| 22 | `recoveryPulse` | Erholungspuls | `0` |
| 23 | `comment` | Kommentar | `''` |
| 24 | `changeid` | Änderungszähler (Sync) | `1` |
| 25 | `profile` | Anhang Höhenprofil (relativ) | `'2016/…fit'` |
| 26 | `gpstrack` | Anhang GPS-Track (relativ) | `'2016/…fit'` |
| 27 | `text` | Anhang Dokumentation | `''` |

### 2.2 Format `maintanence.dat`

`Datum;Equipment;Tätigkeit;Kommentar;Distanz;aktiv;Typ` – Typ 0 = Wartung, 1 = Alarm (Alarm nach *Distanz* km). Equipment `-` = radunabhängig.

### 2.3 Format `bikexperience.ini`

Klassisches INI. Wichtige Sektionen: `[Activity]` (Name=Haupt;Einheit;Ziel km;Ziel hm;Ziel Anzahl), `[Category]`, `[Biker]` (Gewicht, Größe, Geburtstag, Ruhe-/Maximalpuls), `[LimitsPulseTrain]`/`[LimitsPulseStat]`/`[LimitsSpeed]`/`[LimitsGrad]`/`[LimitsPower]` (Name;von,bis;R,G,B), `[Equipment.<Name>]` (Gewicht, Roll-/Luftwiderstandsbeiwerte `cmCoeff`/`cwCoeff`/`cwaCoeff`/`sinCoeff`, Radumfang, Kettenblätter, Ritzel, Aktivität, Gerätezuordnung), `[GPSViewer]`, `[WebAccess]` (Dropbox-Schlüssel – **nicht** übernehmen).

### 2.4 ⚠ Datenqualität: Umlaute bereits beschädigt

`workouts.csv` enthält **119**, `maintanence.dat` mehrere Unicode-Ersatzzeichen (`�`, z. B. „D�mpfer"). Die Umlaute sind dort schon in der Originaldatei verloren gegangen – vermutlich durch die im Handbuch erwähnte Zeichensatz-Problematik bei Dropbox-/Android-Synchronisation (UTF-8 ↔ ISO-8859-1). Bei der Migration kann das nicht automatisch rückgängig gemacht werden; vorgesehen ist eine Korrekturliste (Wörterbuch häufiger Begriffe wie „Dämpfer", „für") plus manuelle Nachkontrolle.

---

## 3. Funktionsinventar des Originals

Gegliedert nach den Bereichen der Anwendung, jeweils mit den zuständigen Java-Paketen/Klassen.

### 3.1 Programmrahmen & Benutzer
- **Mehrbenutzer-Verwaltung** mit getrennten Datenverzeichnissen, „automatisch starten" (`user.*`, `UserView`)
- Hauptfenster mit Karteikarten, Menü-/Werkzeugleiste, Jahres- und Aktivitätsauswahl (`core.BikeXperience`, `DynMenuBar`, `DynToolBar`, `BxpPanel`)
- Willkommensseite, About-Box, Log-Fenster, Hilfe-Viewer (`Welcome`, `AboutBox`, `SimpleHTMLViewer`)
- Automatische Sicherung (`tours.bak`, Backup-Intervall)

### 3.2 Einstellungen (`setting.*`, 21 Klassen)
Persönliche Daten · Saisonziele je Aktivität (km/hm/Anzahl) · Aktivitäten · Kategorien · **Equipments** inkl. Leistungsbeiwerten, Gerätezuordnung, „ausgemustert" · Puls-/Geschwindigkeits-/Steigungs-/Leistungsschwellen mit Farben · Einheiten · Verzeichnisse · Sprache · Look & Feel/Hintergrundbild · Proxy · Kalender · GPS-Viewer (Kachelanbieter, Heimatort, Farben) · Radcomputer/GPS-Gerät · ausblendbare Felder.

### 3.3 Karteikarte „Aktuell" (Übersicht) (`core.Overview`)
Jahresfortschritt km/hm gegen Saisonziel · nötige Wochenleistung bis Saisonende · Bestleistungen mit Rekordjahr · Mehrjahresdiagramm km/hm/Zeit.

### 3.4 Karteikarte „Trainingseinheiten" (`core.BxpTable`, `TourEntryDialog`, `datamodel.TourLogsData`)
- Tabelle mit Sortierung, Filtern (Jahr, Aktivität, Kategorie, Equipment, Trainingsbereich, Distanz, hm, Name), Summenzeile, Bestleistungen rot
- Einheit neu / bearbeiten / kopieren / löschen; Mehrfachauswahl → Kategorie zu-/abordnen, Aktivität verschieben, Equipment neu zuordnen
- Anhänge: Höhenprofil, GPS-Track, Dokumentation (HTML/Text/Bildergalerie-XML) mit Vorschau (`core.ui.TourPreview`)
- Detailauswertung unterhalb: Höhenprofil, Statistiken (Kuchendiagramme), Analysen, Strecke (Karte), Bilder
- Mehrtagestour: mehrere Einheiten gemeinsam auswerten
- Leistungsberechnung (Steig-/Gesamtleistung, Kalorien) (`datamodel.PowerCalc`, `tour.BikePower`)
- Rundenzeiten (Default 1 km, einstellbar) und Geräte-Laps (`tour.LapTimes`, `LapTable`)
- Höhenprofile vergleichen (Overlay, verschiebbar)
- „Werte aus Höhenprofilen aktualisieren" (DST, HM, TM, höchster Berg, Ø/max HF, Ø Trittfrequenz, Ø Temperatur, Erholungspuls)
- Import/Export, Teilen per E-Mail

### 3.5 Karteikarte „Diagramme" (`core.Diagram`, `graphic.*`)
Zeiträume: Kalenderwochen / Monate / Jahre / Monatsverlauf · Werte km / hm / Zeit · Aufteilung nach Trainingsbereich, Equipment oder Kategorie · einzeln oder kumuliert · Balken-/Linien-/Flächendiagramm · Ø- und MAX-Linie · Favoriten speichern.

### 3.6 Karteikarte „Statistik" (`core.Statistic`)
Monats-×-Jahres-Tabelle für Distanz, hm, Zeit, Anzahl, hm/100 km · einzeln/kumuliert · Zeilen AV und MAX · Bestwerte rot, aktuelles Jahr grau.

### 3.7 Karteikarte „Trainingsplan" (`train.*`, 16 Klassen)
Baumtabelle Wochen → Tage mit Minuten je Trainingsbereich · Meilensteine · Plangenerator (Kompensation/Permanent, Laufzeit, Wochentage) · Soll-Ist-Diagramm (16 Wochen) · Auswertung · Pulsbereichsrechner. *(Von Ihnen nicht genutzt.)*

### 3.8 Karteikarte „Kalender" (`core.BxpCalendar`)
Monatsansicht mit Einheiten (Thumbnail des Höhenprofils oder DST/HM), Wartungen, Planungen, Trainingsplan; Tagesansicht mit Schnellerfassung.

### 3.9 Karteikarte „Wartung" (`core.MaintenanceView`, `MaintenanceDialog`, `datamodel.Maintenance`)
Wartungseinträge (Datum, Equipment, Tätigkeit aus erweiterbarer Liste, Kommentar) · **Alarme nach n km** mit blinkendem Symbol · erweitertes Löschen · Auskunft „wie viele km/Tage hielt das Teil" (`MtnLogEntry`) · Kuchendiagramm · CSV-Import/-Export.

### 3.10 Karteikarte „Planung" (`core.PlanningView`, `PlanningDialog`)
Zukünftige Touren mit Datum, Name, Beschreibung, GPS-Track, Kartenvorschau. *(Von Ihnen nicht genutzt.)*

### 3.11 Data Manager & Tour-Editor (`cdm.*`)
Rohdaten von Radcomputern laden, Touren bearbeiten (Trackpunkt-Tabelle, Höhen korrigieren, Marken setzen, Zielhöhe anpassen), Dateien anhängen/mischen, Speichern als CDM/TUR/CSV/HTML, Übergabe an die Haupttabelle.

### 3.12 Import / Export (`filter.ImportTours`, `ExportTours`, `gps.*`)
**Import:** FIT, TCX, GPX, KML, CRS, PWX, TRK, SLF, SDF, SRM-Text, Polar HRM, CicloSport TUR/DAT, CicloTour CRP/HRP, PalmOS PDB, CDM, Text/CSV.
**Export:** TUR, DAT, CDM, CSV, HTML; GPS-Viewer zusätzlich GPX (Track/Route/Wegpunkte), TCX Course, KML, OVL, Text.

### 3.13 Geräte-Synchronisation (`device.*`, `rxtx.*`, `gpsviewer.GPSEdgeDevice`, `web.*`)
CicloSport HAC4/HAC4Pro/HAC5/CM4xx über serielle Schnittstelle · **Garmin Edge per USB-Laufwerk** (Activities/History lesen, Courses/NewFiles schreiben, Duplikaterkennung über Datum+km) · gpsbabel · Dropbox-Sync.

### 3.14 GPS-Viewer (`gpsviewer.*`, 24 Klassen)
OSM-Karte · Track anzeigen/bearbeiten/kürzen/umkehren · Trackpunkt-Tabelle · Höhenprofil gekoppelt mit Kartenposition · Höhenwerte aus SRTM3/GTOPO30 · Zeiten neu berechnen · Glätten (Ramer-Douglas-Peucker) · Streckenpunkte (Abbiegehinweise, Gipfel) · Wegpunkte/POI · Routen · **Overlays** mit Ordnerverwaltung · Tourenplanung per Mausklick inkl. „Zauberstab" (Teilstücke von Overlays übernehmen) · Track-Simulation mit Diashow · Bilder-Geotagging · Offline-Kachelcache.

### 3.15 Bilder (`picture.*`, `pictureviewer.*`)
Galerie als XML, Thumbnails, EXIF-Anzeige, Zuordnung zu Trackposition über Aufnahmezeit.

### 3.16 Auskünfte / Reports (`core.Report`, 80 KB – größte Fachklasse)
Trainingskontrolle je Bereich · Leistungskontrolle + Diagramm · Ø-Geschwindigkeits-Diagramm · Trittfrequenz-Diagramm · Laufbestzeiten (5/10/21,1/42,2/Ultra) · Laufzeitendiagramm · **Jahresprognose** (Hochrechnung/AV/MAX) · Pulsdiagramm · Maximal-/Ruhepuls · AVS-HF-Vergleich mit Fitnesslevel · Vergleich Aktivitäten / Kategorien · Einsatz Equipment · Temperatur · Gewichtsverlauf · Erholungspuls · **virtuelle Weltumrundung** (und Weg zum Mond) · Top 50 · Suche (auch in Anhängen).

### 3.17 Werkzeuge (`util.*`)
Marathonzeitenplaner (inkl. Riegel-Formel) · **Ritzelrechner** mit Grafik je Equipment (`Ritzelgrafik`, `RitzelRechner`) · Pulsrechner (Zonen aus Ruhe-/Maximalpuls).

### 3.18 Hilfsfunktionen (`numeric.*`, `util.*`)
Matrix-/Vektorrechnung, Regression, Splines (für Glättung/Prognosen) · INI-Leser · Tabellen-Sortierer · Eingabeprüfer · Logging.

---

## 4. Empfehlung zum Umfang: übernehmen, vereinfachen, weglassen

| Bewertung | Funktionen |
|---|---|
| **Kern – übernehmen** | Trainingseinheiten (Tabelle, Filter, CRUD, Kategorien, Equipment), FIT-Import, Übersicht „Aktuell", Statistik, Diagramme, Wartung mit Alarmen, Kalender, Einstellungen, Tourdetail mit Höhenprofil + Karte + Puls-/Zonenauswertung, Leistungsberechnung |
| **Wichtig – zweite Stufe** | Auskünfte/Reports, Rundenzeiten, Höhenprofil-Vergleich, Werkzeuge (Ritzel-, Puls-, Marathonrechner), Garmin-Edge-Sync per USB-Laufwerk, GPX/TCX-Import und -Export, Suche |
| **Optional – später** | GPS-Viewer-Editor/Tourenplanung, Overlays, Trainingsplan, Planungs-Karteikarte, Bildergalerie/Geotagging, Track-Simulation, Weltumrundung |
| **Weglassen (veraltet)** | CicloSport-Seriellgeräte (HAC/CM), PalmOS PDB, Polar HRM/Suunto SDF/Sigma SLF/CicloTour/Timex usw. (nur bei Bedarf nachrüsten), Dropbox-Sync, gpsbabel, Google-Maps-HTML-Export, Look-&-Feel/Skins, Proxy, Data-Manager-Formate CDM/TUR/DAT, Android-Kopplung |
| **Klären** | Mehrbenutzer-Verwaltung (nötig oder Einzelplatz?) |

---

## 5. Zielarchitektur in C#

### 5.1 Technologie

| Bereich | Vorschlag | Begründung |
|---|---|---|
| Plattform | **.NET 10 (LTS)** | aktuelle Langzeitversion |
| Oberfläche | **WPF** mit MVVM (`CommunityToolkit.Mvvm`) | Windows-Desktop, gute Tabellen/Datenbindung; ein `.vs`-Ordner deutet auf Visual Studio hin |
| Datenhaltung | **SQLite** über `Microsoft.Data.Sqlite` (+ optional EF Core) | eine Datei, transaktionssicher, ersetzt CSV/INI/DAT |
| FIT-Dateien | **Garmin FIT SDK für C#** (NuGet `Garmin.FIT.Sdk`) | offizielles SDK, liest alle Felder |
| GPX/TCX | eigene Leser mit `System.Xml.Linq` | einfache XML-Formate |
| Diagramme | **ScottPlot** (oder LiveCharts2) | Höhenprofil mit mehreren Achsen, Balken/Linie/Fläche/Kuchen |
| Karte | **Mapsui** mit OpenStreetMap-Kacheln | OSM-basiert wie das Original, Offline-Cache möglich |
| Tests | **xUnit** | v. a. für Migration und Berechnungen |

### 5.2 Projektstruktur (Solution `RadlDB.sln`)

```
RadlDB.Core/        Domänenmodell + Berechnungen (keine UI, keine DB)
  Modell/           Trainingseinheit, Equipment, Aktivitaet, Kategorie, Wartung, Trackpunkt, Pulszone …
  Berechnungen/     je Funktion eine Datei, z. B. BerechneDurchschnittsgeschwindigkeit.cs
RadlDB.Daten/       SQLite-Schema, Repositories
RadlDB.Import/      FitLeser, GpxLeser, TcxLeser, Altdaten-Migration (workouts.csv, ini, maintanence.dat)
RadlDB.App/         WPF-Oberfläche (Views, ViewModels)
RadlDB.Tests/       Unit-Tests
```

**Projektregel „jede Funktion in eine eigene Datei" (CLAUDE.md):** Fachfunktionen werden als kleine statische Klassen mit genau einer öffentlichen Methode umgesetzt (z. B. `Berechnungen/BerechneSteigleistung.cs` → `BerechneSteigleistung.Ausfuehren(...)`), Modelle und ViewModels als je eine Klasse pro Datei. Alle Dateien erhalten ausführliche XML-Doc-Kommentare und erläuternde Inline-Kommentare.

### 5.3 Datenmodell (SQLite)

```
Aktivitaet      (Id, Name, IstHaupt, Geschwindigkeitseinheit, ZielKm, ZielHm, ZielAnzahl)
Kategorie       (Id, Name, Standard)
Equipment       (Id, Name, AktivitaetId, Gewicht, Radumfang, CmCoeff, CwCoeff, CwaCoeff, SinCoeff,
                 Kettenblaetter, Ritzel, Geraetezuordnung, Ausgemustert, Datum)
Trainingseinheit(Id, Datum, Startzeit, Name, Fahrzeit, DistanzKm, Hoehenmeter, MaxHoehe, StartHoehe,
                 MaxGeschw, HfDurchschnitt, HfMax, HfMin, Trittfrequenz, Temperatur, Leistung,
                 Koerpergewicht, Erholungspuls, Trainingsbereich, Kommentar, AktivitaetId, EquipmentId)
EinheitKategorie(EinheitId, KategorieId)           – n:m
Anhang          (Id, EinheitId, Typ [Profil|Track|Dokument], RelativerPfad)
Trackpunkt      (optional als Cache: EinheitId, Zeit, Lat, Lon, Hoehe, Hf, Tf, Geschw, Leistung, Temp)
Wartung         (Id, Datum, EquipmentId?, Taetigkeit, Kommentar, Typ [Wartung|Alarm], AlarmKm, Aktiv)
Schwellwert     (Id, Art [PulsTraining|PulsStatistik|Geschw|Steigung|Leistung], Name, Von, Bis, Farbe)
Fahrer          (Vorname, Nachname, Geburtstag, Groesse, Gewicht, Ruhepuls, Maxpuls, Heimatort)
Einstellung     (Schluessel, Wert)                 – alles Übrige
```

Die FIT-Dateien bleiben unverändert in den Jahresordnern; die Datenbank speichert nur relative Pfade.

---

## 6. Umsetzungsplan in Phasen

Jede Phase endet mit einem lauffähigen, testbaren Stand.

### Phase 0 – Grundgerüst (klein)
Solution und Projekte anlegen, NuGet-Pakete, Kommentar-/Dateikonventionen, leeres Hauptfenster mit Karteikarten.

### Phase 1 – Datenmodell & Migration der Altdaten ⭐ Grundlage für alles
1. Modellklassen in `RadlDB.Core`
2. SQLite-Schema und Repositories
3. Migrationsfunktionen: `LeseWorkoutsCsv`, `LeseBikexperienceIni`, `LeseWartungsdatei`
4. Umlaut-Reparatur mit Korrekturliste + Bericht der nicht reparierbaren Stellen
5. **Prüfung:** 932 Einheiten, 18 Equipments, alle Wartungen übernommen; Summen km/hm je Jahr identisch mit dem Original

**Stand 22.10.2026 – umgesetzt.** Ergebnis mit den echten Altdaten: 932 Einheiten, 86 Wartungen, 20 Equipments (18 aus der INI + „GENERIC"/„MOUNTAIN" aus `workouts.csv`), 5 Kategorien (+ „Training"), 28 Schwellwerte. Jahressummen nach Speichern/Laden identisch mit `workouts.csv` (automatischer Test).
Abweichungen vom Entwurf in 5.3: zusätzliche Spalten `Equipment.CrCoeff`, `Equipment.AnfangsdistanzKm`, `Equipment.StandardKategorie`, `Fahrer.Id` und `Trainingseinheit.FahrerId` (Nutzerverwaltung); Tabelle `Einstellung` und `Trackpunkt` folgen bei Bedarf (Phase 2 ff.).
Befunde: 19 Umlaute lagen als Latin-1-Bytes vor und wurden exakt wiederhergestellt, 121 Texte über die Korrekturliste repariert; offen sind „Stoisser B�ckeralm" und zweimal „(�PD)". Die FIT-Dateien zu Zeile 344 (2019-11-11) und Zeile 933 (2026-10-05) fehlen im Datenordner.
Bedienung: *Datei → Altdaten aus BikeXperience übernehmen…* legt `radldb.db` und `migrationsbericht.txt` im Datenordner an.

### Phase 2 – FIT-Import & Trackmodell
FIT-Datei lesen (Trackpunkte, Laps, Kennwerte) · Kennwerte berechnen (Distanz, hm mit Schwellwert, Ø-/Max-Werte, Zeit in Zonen) · Neue Einheit aus FIT-Datei · Duplikaterkennung (Datum + Distanz) · Test gegen 930 vorhandene Dateien.

### Phase 3 – Trainingseinheiten-Tabelle
Tabelle mit Sortierung und allen Filtern · Summenzeile · Bestleistungen hervorheben · Dialog Neu/Bearbeiten/Kopieren/Löschen · Mehrfachauswahl-Aktionen (Kategorie, Aktivität, Equipment).

### Phase 4 – Übersicht, Statistik, Diagramme
Karteikarte „Aktuell" mit Saisonziel und Wochenbedarf · Statistik-Tabelle Monat × Jahr mit AV/MAX · Diagramme (Woche/Monat/Jahr/Monatsverlauf; km/hm/Zeit; nach Bereich/Equipment/Kategorie; Balken/Linie/Fläche).

### Phase 5 – Tourdetail
Höhenprofil mit zuschaltbaren Achsen (Höhe, Puls, Geschwindigkeit, Trittfrequenz, Temperatur, Steigung, Leistung), Pulszonen-Hintergrund, Zoom, Bereichsauswertung (Distanz, hm, Zeit, Ø, Steigleistung) · Karte mit Track und gekoppeltem Cursor · Zonen-Kuchendiagramme · Rundenzeiten · Leistungsberechnung.
*Hinweis:* Die Leistungsformeln (`PowerCalc`, `BikePower`, `TourEvaluation`) sollten hier per Bytecode-Analyse oder Decompiler exakt nachvollzogen werden, damit die Werte mit dem Original übereinstimmen.

### Phase 6 – Wartung & Kalender
Wartungsliste mit erweiterbarer Tätigkeitsliste · Alarme nach km mit Hinweis beim Start · Auskunft „Laufleistung seit Wartung" · Kuchendiagramm · Kalender Monat/Tag mit Schnellerfassung.

### Phase 7 – Auskünfte & Werkzeuge
Reports aus 3.16 (Priorität: Equipment-Einsatz, Jahresprognose, Pulsauswertungen, Top 50, Suche, Gewichtsverlauf, Laufbestzeiten) · Ritzelrechner · Pulsrechner · Marathonzeitenplaner.

### Phase 8 – Garmin & Austauschformate
Garmin Edge am USB erkennen, neue FIT-Dateien in den Jahresordner kopieren und importieren · GPX/TCX lesen und schreiben · CSV-Export.

### Phase 9 – Optional
GPS-Track-Editor und Tourenplanung, Overlays, Trainingsplan, Planungs-Karteikarte, Bildergalerie/Geotagging, Weltumrundung, Mehrbenutzer.

---

## 7. Offene Fragen an Sie

1. **Funktionsumfang:** Welche der Bereiche aus Abschnitt 3 nutzen Sie tatsächlich regelmäßig? Besonders: Auskünfte (welche?), GPS-Viewer/Tourenplanung, Kalender, Laufen-Auswertungen.
2. **Mehrbenutzer:** Brauchen Sie mehrere Benutzer, oder reicht ein Einzelplatz?
3. **Oberfläche:** Ist WPF (nur Windows) in Ordnung, oder soll das Programm auch unter Linux laufen (dann Avalonia statt WPF)? Die INI enthält einen Linux-Tastaturbefehl (`tiitoo-keyboard`), möglicherweise ein Tablet.
4. **Datenhaltung:** SQLite einverstanden, oder sollen die Daten weiterhin als lesbare Textdateien (CSV) liegen?
5. **Parallelbetrieb:** Soll BikeXperience während der Übergangszeit weiterlaufen? Dann bräuchte RadlDB einen Rück-Export nach `workouts.csv`.
6. **Garmin:** Welches Gerät nutzen Sie aktuell (für die USB-Synchronisation)?

Antworten:
1. Frage mich das für jede einzelne Funktion, bevor die im Plan aufgenpommen wird.
2. einer reicht
3. nur Windows
4. SQLite einverstanden
5. nein
6. Garmin Egde 1040 Solar. dieses kann bisher in BikeXperience nicht wie ein Laufwerk ausgelesen werden; die aktuelle Technologie dafür musst Du bitte ermitteln und mir mitteilen.


---

## Anhang: Paketübersicht des Originals (eigener Code)

| Paket | Klassen | Zweck |
|---|---|---|
| `view` | 3 | Programmstart |
| `core` | 16 | Hauptfenster, alle Karteikarten, Reports |
| `core.ui` | 7 | Tour-Vorschau, Auswahlboxen |
| `datamodel` | 19 | Einheiten, Wartung, Planung, Speicherung, Leistungsberechnung |
| `tour` | 7 | Tour mit Trackpunkten, Laps, Marken, Leistung |
| `cdm` | 15 | Data Manager, Tour-Editor, Tourauswertung |
| `filter` | 3 | Import/Export der Einheiten |
| `gps` | 40 | Leser/Schreiber für alle GPS-Formate, Geo-Berechnungen |
| `gpsviewer` (+ `overlays`) | 24 | Kartenanwendung, Routing, Overlays, Simulation |
| `graphic` | 15 | Diagrammkomponenten, Höhenprofil, Weltkarte |
| `train` | 16 | Trainingsplan |
| `setting` | 21 | Einstellungsdialog mit allen Reitern |
| `device`, `rxtx` | 15 | CicloSport-Geräte über serielle Schnittstelle |
| `web` | 8 | Dropbox, HTTP |
| `picture`, `pictureviewer` | 10 | Bilder, EXIF, Galerie |
| `numeric` | 16 | Matrizen, Regression, Splines |
| `ui` | 35 | wiederverwendbare Oberflächenbausteine |
| `user.*` | 6 | Benutzerverwaltung |
| `util` | 47 | Hilfsfunktionen, Ritzelrechner, INI, Logging |
| `event`, `resource` | 7 | Ereignisse, Lokalisierung |
