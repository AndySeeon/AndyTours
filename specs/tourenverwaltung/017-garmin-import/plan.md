# Plan: 017 – Garmin-Import

## Vorgehen
Das Import-Projekt erhält einen Bereich `Garmin/`. Dort werden die angeschlossenen Geräte gesucht, ihre Aufzeichnungen aufgelistet und mit `Garmin.FIT.Sdk` gelesen; das Paket ist dort schon eingebunden. Edge 800 und Edge 1000 erscheinen als Laufwerk mit dem Ordner `Garmin/Activities`. Den Edge 1040, der nur als Mediengerät erscheint, liest das App-Projekt über das Paket `MediaDevices` (ADR 0002), weil dieses ein Windows-Zielframework braucht. Beide Wege liefern dieselbe Art Geräteaufzeichnung (Dateiname, Gerätename und eine Möglichkeit zum Kopieren).

Schon vor dem Lesen werden Dateien übersprungen, deren Name (`2026-09-26-08-02-49.fit`, lokale Startzeit) einer vorhandenen Einheit entspricht. Die übrigen Aufzeichnungen werden in einen temporären Ordner kopiert und ganz gelesen. Das Ergebnis ist ein `Aufzeichnung`-Objekt im Core mit:
- den Werten der Sitzung,
- dem Profil,
- der Rad-Kennung: undokumentierte FIT-Nachricht 526, Feld 0, seit Edge-1040-Software 31.33,
- den Seriennummern der Geschwindigkeits- und Trittfrequenzsensoren: `DeviceInfo` mit ANT+-Gerätetyp 121, 122 oder 123.

Fachfunktionen im Core übernehmen den Rest:
- die Duplikatprüfung,
- die Vorbelegung des Equipments (zuerst Rad-Kennung, dann Sensor, dann Standard-Equipment der Hauptaktivität),
- das Erzeugen der Einheit,
- das Merken der Kennungen im vorhandenen Feld `Equipment.Geraetezuordnung`.

Ein Importfenster zeigt die neuen Touren mit Häkchen und einem änderbaren Equipment.

Beim Importieren geschieht für jede angehakte Tour Folgendes:
1. Die Aufzeichnung wird nach `<Datenordner>/<Jahr>/<Dateiname>` abgelegt, wie bei den übernommenen Altdaten.
2. Die Einheit wird samt Anhängen (Höhenprofil und GPS-Track auf dieselbe Datei) eingefügt.
3. Die geänderten Equipments werden gespeichert.

Danach markiert das Hauptfenster die neuen Einheiten.

## Dateien
- `src/AndyTours.Core/Modell/Aufzeichnung.cs` (neu) — gelesene Werte einer Aufzeichnung: Dateiname, Gerätename, Datum, Startzeit, Fahrzeit, Distanz, Höhen, Geschwindigkeit, Puls, Trittfrequenz, Temperatur, Leistung, Profil, Rad-Kennung, Sensoren
- `src/AndyTours.Core/Modell/Radkennung.cs` (neu) — eine gemerkte Kennung (Art und Wert)
- `src/AndyTours.Core/Modell/Radkennungsart.cs` (neu) — Rad-Kennung, Sensor, Altwert (z. B. „Bike1“)
- `src/AndyTours.Core/Berechnungen/GeraetezuordnungText.cs` (neu) — `Lesen`/`Schreiben`: Text in `Equipment.Geraetezuordnung` ↔ Liste der Radkennungen; Altwerte bleiben als Altwert erhalten
- `src/AndyTours.Core/Berechnungen/IstTourVorhanden.cs` (neu) — gleiches Datum und gleiche Startzeit (auf die Minute) wie eine vorhandene Einheit
- `src/AndyTours.Core/Berechnungen/ErmittleImportEquipment.cs` (neu) — Vorbelegung: Rad-Kennung, dann Sensor, dann Standard-Equipment der Hauptaktivität; ausgemusterte Equipments zählen nicht
- `src/AndyTours.Core/Berechnungen/ErstelleImportierteEinheit.cs` (neu) — Einheit aus Aufzeichnung, Equipment und Anhangpfad. Name leer, Aktivität des Equipments (sonst Hauptaktivität), Standardkategorien plus Standardkategorie des Equipments, Fahrer wie bei `ErstelleNeueEinheit`
- `src/AndyTours.Core/Berechnungen/MerkeRadkennungen.cs` (neu) — trägt Rad-Kennung und Sensoren beim gewählten Equipment ein, entfernt sie bei anderen und liefert die geänderten Equipments
- `src/AndyTours.Core/Berechnungen/BeschreibeImportergebnis.cs` (neu) — Statustext „3 Touren importiert.“ und Meldung mit den übersprungenen Dateien
- `src/AndyTours.Core/Modell/Equipmenteingaben.cs` (ändern) — Liste der gemerkten Radkennungen
- `src/AndyTours.Core/Berechnungen/EquipmentEingaben.cs` (ändern) — Radkennungen in beide Richtungen umwandeln statt sie unverändert aus der Basis zu übernehmen
- `src/AndyTours.Import/Garmin/Geraeteaufzeichnung.cs` (neu) — eine Aufzeichnung auf einem Gerät: Dateiname, Gerätename, Kopieren in eine Datei
- `src/AndyTours.Import/Garmin/FindeLaufwerksgeraete.cs` (neu) — Edge 800/1000: Wechsellaufwerke mit `Garmin/Activities`, Gerätename aus `Garmin/GarminDevice.xml`
- `src/AndyTours.Import/Garmin/ParseAufzeichnungsname.cs` (neu) — Datum und Startzeit aus dem Dateinamen, für die Vorauswahl
- `src/AndyTours.Import/Garmin/LeseFitAufzeichnung.cs` (neu) — liest eine FIT-Datei. Startzeit in Ortszeit über `Activity.LocalTimestamp`, Fahrzeit = `TotalTimerTime`, Starthöhe aus dem ersten Messpunkt, Gerätename aus `FileId.Product`, Rad-Kennung aus Nachricht 526
- `src/AndyTours.Daten/Datenordner/LegeAufzeichnungAb.cs` (neu) — kopiert nach `<Jahr>/<Dateiname>` und liefert den relativen Pfad; eine vorhandene andere Datei gleichen Namens wird nicht überschrieben, sondern die neue erhält einen Zusatz
- `src/AndyTours.Daten/TrainingseinheitenTabelle.cs` (ändern) — `Einfuegen` schreibt auch die Anhänge in derselben Transaktion
- `src/AndyTours.App/AndyTours.App.csproj` (ändern) — Paket `MediaDevices` 2.0.0
- `src/AndyTours.App/Funktionen/FindeMtpGeraete.cs` (neu) — Edge 1040 über `MediaDevices`: Garmin-Mediengeräte mit Ordner `Garmin/Activity` im internen Speicher, liefert Geräteaufzeichnungen
- `src/AndyTours.App/ViewModels/Importzeile.cs` (neu) — eine Zeile der Importliste: Häkchen, Datum, Startzeit, Distanz, Fahrzeit, Profil, Gerät, Equipment
- `src/AndyTours.App/ViewModels/GarminImportViewModel.cs` (neu) — Liste, Equipmentauswahl, Befehl „Importieren“
- `src/AndyTours.App/Views/GarminImportFenster.xaml` (neu) — Fenster mit Tabelle (Spalte Gerät nur bei mehreren Geräten), „Importieren“ und „Abbrechen“
- `src/AndyTours.App/Views/GarminImportFenster.xaml.cs` (neu) — Code-Behind
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Import.cs` (neu) — Befehle „Vom Garmin importieren…“ und „FIT-Dateien importieren…“: Geräte bzw. Dateien suchen, im Hintergrund mit Fortschrittsfenster lesen, Hinweise zeigen, Importfenster öffnen, speichern, markieren, Statusmeldung
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — beide Befehle im Menü „Datei“
- `src/AndyTours.App/ViewModels/EquipmentBearbeitenViewModel.cs` (ändern) — gemerkte Kennungen anzeigen, Befehl „Entfernen“
- `src/AndyTours.App/Views/EquipmentBearbeitenFenster.xaml` (ändern) — Bereich „Gerätezuordnung“ mit Liste und Knopf „Entfernen“
- `tests/AndyTours.Tests/Garmin/FitTestdatei.cs` (neu) — erzeugt FIT-Testdateien mit dem SDK, auch mit Nachricht 526 und Sensoren; keine persönlichen Aufzeichnungen im Repository
- `tests/AndyTours.Tests/Garmin/LeseFitAufzeichnungTests.cs` (neu)
- `tests/AndyTours.Tests/Garmin/ParseAufzeichnungsnameTests.cs` (neu)
- `tests/AndyTours.Tests/Garmin/FindeLaufwerksgeraeteTests.cs` (neu) — mit einem Testordner als Laufwerkswurzel
- `tests/AndyTours.Tests/Berechnungen/GeraetezuordnungTextTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/IstTourVorhandenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErmittleImportEquipmentTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErstelleImportierteEinheitTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/MerkeRadkennungenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BeschreibeImportergebnisTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/EquipmentEingabenTests.cs` (ändern) — Radkennungen statt unveränderter Gerätezuordnung
- `tests/AndyTours.Tests/Datenordner/LegeAufzeichnungAbTests.cs` (neu)
- `tests/AndyTours.Tests/Daten/EinheitSchreibenTests.cs` (ändern) — `Einfuegen` mit Anhängen

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Import vom Gerät
- **Kriterium:** Wenn der Benutzer „Vom Garmin importieren…“ wählt und ein Edge 1040, Edge 1000 oder Edge 800 per USB angeschlossen ist, dann zeigt ein Fenster alle Touren des Geräts, die noch nicht in AndyTours sind, mit Datum, Startzeit, Distanz, Fahrzeit, Profil und Equipment; alle sind angehakt und lassen sich abwählen.
  - Umsetzung: `FindeLaufwerksgeraete`, `FindeMtpGeraete`, `LeseFitAufzeichnung`, `HauptfensterViewModel.Import`, `GarminImportFenster`
  - Prüfung: `FindeLaufwerksgeraeteTests`, `LeseFitAufzeichnungTests`
  - Manuell (für `FindeMtpGeraete` kein automatischer Test, da ein echtes Gerät nötig ist): Edge 1040 und Edge 1000 anschließen, „Vom Garmin importieren…“ wählen. Es erscheinen nur fehlende Touren mit allen Spalten, alle angehakt.
- **Kriterium:** Wenn mehrere Garmin-Geräte gleichzeitig angeschlossen sind, dann zeigt die Liste die neuen Touren aller Geräte, jeweils mit dem Namen des Geräts.
  - Umsetzung: `HauptfensterViewModel.Import` sammelt alle Geräte, `Importzeile.Geraet`
  - Manuell: beide Edges gleichzeitig anschließen; die Touren beider Geräte stehen mit „Edge 1040“ bzw. „Edge 1000“ in der Liste.
- **Kriterium:** Wenn der Benutzer auf „Importieren“ klickt, dann wird für jede angehakte Tour eine Einheit angelegt; Datum, Startzeit, Fahrzeit, Distanz, Höhenmeter, Höhen, Geschwindigkeiten, Puls, Trittfrequenz, Temperatur und Leistung kommen aus der Aufzeichnung, der Name bleibt leer.
  - Umsetzung: `LeseFitAufzeichnung`, `ErstelleImportierteEinheit`, `TrainingseinheitenTabelle.Einfuegen`
  - Prüfung: `LeseFitAufzeichnungTests`, `ErstelleImportierteEinheitTests`
  - Manuell: zwei von drei Touren angehakt lassen und importieren; es entstehen genau zwei Einheiten.
- **Kriterium:** Wenn eine Einheit importiert wird, dann ist ihre Fahrzeit die gestoppte Zeit ohne Pausen, wie sie der Edge als „Zeit“ anzeigt.
  - Umsetzung: `LeseFitAufzeichnung` (`TotalTimerTime`)
  - Prüfung: `LeseFitAufzeichnungTests` (gestoppte Zeit ≠ Gesamtzeit)
- **Kriterium:** Wenn eine Tour importiert wird, dann wird ihre Aufzeichnung bei der Einheit abgelegt, so wie bei den aus BikeXperience übernommenen Touren.
  - Umsetzung: `LegeAufzeichnungAb`, `ErstelleImportierteEinheit` (Anhänge Höhenprofil und GPS-Track), `TrainingseinheitenTabelle.Einfuegen`
  - Prüfung: `LegeAufzeichnungAbTests`, `EinheitSchreibenTests`
- **Kriterium:** Wenn der Import fertig ist, dann sind die neuen Einheiten in der Tabelle markiert, und die Statusleiste meldet die Anzahl, z. B. „3 Touren importiert.“
  - Umsetzung: `HauptfensterViewModel.Import` (Ereignis `MarkiereEinheiten`), `BeschreibeImportergebnis`
  - Prüfung: `BeschreibeImportergebnisTests`
  - Manuell: nach dem Import sind die neuen Zeilen markiert, die Statusleiste zeigt die Anzahl.

### Import aus Dateien
- **Kriterium:** Wenn der Benutzer „FIT-Dateien importieren…“ wählt und eine oder mehrere Aufzeichnungsdateien aussucht, dann erscheint dieselbe Liste wie beim Gerät, mit den Touren aus diesen Dateien, die noch nicht in AndyTours sind.
  - Umsetzung: `HauptfensterViewModel.Import` (Dateiauswahl, dann derselbe Ablauf)
  - Manuell: zwei FIT-Dateien wählen, eine davon schon vorhanden; nur die andere erscheint.

### Gerätezuordnung
- **Kriterium:** Wenn die Aufzeichnung eine Rad-Kennung enthält, die bei einem Equipment gemerkt ist, dann steht in der Liste dieses Equipment.
  - Umsetzung: `ErmittleImportEquipment`
  - Prüfung: `ErmittleImportEquipmentTests`
- **Kriterium:** Wenn die Rad-Kennung fehlt oder bei keinem Equipment gemerkt ist, aber ein Geschwindigkeits- oder Trittfrequenzsensor der Aufzeichnung bei einem Equipment gemerkt ist, dann steht in der Liste dieses Equipment.
  - Umsetzung: `ErmittleImportEquipment`, `LeseFitAufzeichnung` (Sensoren)
  - Prüfung: `ErmittleImportEquipmentTests`, `LeseFitAufzeichnungTests`
- **Kriterium:** Wenn weder Rad-Kennung noch Sensor passen, dann steht in der Liste das Standard-Equipment der Hauptaktivität.
  - Umsetzung: `ErmittleImportEquipment` (über `ErmittleStandardEquipment`)
  - Prüfung: `ErmittleImportEquipmentTests`
- **Kriterium:** Wenn der Benutzer in der Liste für eine Tour ein anderes Equipment wählt, dann erhält die Einheit dieses Equipment.
  - Umsetzung: `GarminImportViewModel`, `ErstelleImportierteEinheit`
  - Prüfung: `ErstelleImportierteEinheitTests`
  - Manuell: in der Liste ein anderes Rad wählen und importieren; die Einheit zeigt dieses Rad.
- **Kriterium:** Wenn eine Einheit importiert wird, dann erhält sie die Aktivität ihres Equipments (ohne Aktivität: die Hauptaktivität), die Standardkategorien und die Standardkategorie des Equipments.
  - Umsetzung: `ErstelleImportierteEinheit`
  - Prüfung: `ErstelleImportierteEinheitTests`
- **Kriterium:** Wenn eine Tour importiert wird, dann merkt sich AndyTours ihre Rad-Kennung und ihre Geschwindigkeits- und Trittfrequenzsensoren bei dem Equipment der Einheit; waren sie bei einem anderen Equipment gemerkt, werden sie dort entfernt.
  - Umsetzung: `MerkeRadkennungen`, `GeraetezuordnungText`, Speichern über `EquipmentsTabelle.Aendern`
  - Prüfung: `MerkeRadkennungenTests`, `GeraetezuordnungTextTests`
- **Kriterium:** Wenn der Benutzer ein Equipment bearbeitet, dann sieht er dort die gemerkten Rad-Kennungen und Sensoren und kann einzelne entfernen.
  - Umsetzung: `EquipmentEingaben`, `EquipmentBearbeitenViewModel`, `EquipmentBearbeitenFenster`
  - Prüfung: `EquipmentEingabenTests`
  - Manuell: Equipment öffnen, eine Kennung entfernen, speichern und erneut öffnen; die Kennung fehlt.

### Grenz- und Fehlerfälle
- **Kriterium:** Wenn eine Tour mit gleichem Datum und gleicher Startzeit schon in AndyTours ist, dann erscheint sie nicht in der Liste.
  - Umsetzung: `ParseAufzeichnungsname` (Vorauswahl), `IstTourVorhanden` (nach dem Lesen)
  - Prüfung: `IstTourVorhandenTests`, `ParseAufzeichnungsnameTests`
- **Kriterium:** Wenn kein Garmin-Gerät angeschlossen ist, dann erscheint der Hinweis „Kein Garmin-Gerät gefunden. Bitte den Edge per USB anschließen.“
  - Umsetzung: `HauptfensterViewModel.Import`
  - Manuell: ohne Gerät „Vom Garmin importieren…“ wählen.
- **Kriterium:** Wenn alle Touren schon in AndyTours sind, dann erscheint der Hinweis „Keine neuen Touren gefunden.“
  - Umsetzung: `HauptfensterViewModel.Import`
  - Manuell: zweimal hintereinander importieren; beim zweiten Mal erscheint der Hinweis.
- **Kriterium:** Wenn eine Aufzeichnung nicht gelesen werden kann, dann wird sie übersprungen und in der Meldung am Ende genannt; die übrigen Touren werden trotzdem importiert.
  - Umsetzung: `LeseFitAufzeichnung` (Fehler → `null` mit Grund), `BeschreibeImportergebnis`
  - Prüfung: `LeseFitAufzeichnungTests` (beschädigte Datei), `BeschreibeImportergebnisTests`

## Entscheidungen
- **Gerätezuordnung:** Rad-Kennung vor Sensor vor Standard-Equipment; gelernt wird bei jedem Import (Wahl B3 des Benutzers). Als Sensoren zählen nur Geschwindigkeits- und Trittfrequenzsensoren; Brustgurt und Leistungsmesser zählen nicht.
- **Speicherort der Kennungen:** Sie stehen im vorhandenen Textfeld `Equipment.Geraetezuordnung` (z. B. `Rad:7af22db6-…; Sensor:3452708077`), daher keine Schemaänderung. Altwerte wie „Bike1“ bleiben als Altwert sichtbar und lassen sich entfernen.
- **Gerätename:** Er stammt aus der Aufzeichnung (`FileId.Product`, z. B. „Edge 1040“). Damit gilt er gleichermaßen für Gerät und Datei.
- **Startzeit:** Sie wird sekundengenau gespeichert. Für die Duplikatprüfung wird auf die Minute verglichen, weil die übernommenen Altdaten nur „hh:mm“ haben.
- **Menü:** Beide Befehle stehen unter „Datei“.

## Offene Entscheidungen
- Wie greift AndyTours auf den Edge 1040 als Mediengerät (MTP) zu? → decided in docs/adr/0002-zugriff-auf-mediengeraete.md

## Risiken
- Die Rad-Kennung (Nachricht 526) ist von Garmin nicht dokumentiert und wird über ihre Nummer gelesen. Ändert ein Firmware-Update das Format, greift die Zuordnung über die Sensoren bzw. das Standard-Equipment, und die Werte der Tour bleiben unberührt.
- Ohne Kennung und Sensoren lassen sich SL Road SL und FELT (meist Edge 1000) nicht unterscheiden. Diese Touren bekommen das Standard-Equipment und müssen in der Liste umgestellt werden.
- Viele Aufzeichnungen über MTP zu lesen kann dauern. Deshalb gibt es die Vorauswahl über den Dateinamen und das Fortschrittsfenster.
- Bei sehr langen Aufzeichnungen (z. B. Gesamtzeit über mehrere Tage beim Edge 1000) ist nur `TotalTimerTime` verlässlich. Die Gesamtzeit wird nicht verwendet.
