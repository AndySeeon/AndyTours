# Plan: 004 – Automatische Datensicherung

## Vorgehen
- Die Sicherungseinstellungen sind Sicherungsordner (leer = Standard), Intervall, Höchstzahl und die Zahl der Programmstarts seit der letzten Sicherung. Sie liegen in einer neuen Tabelle `Einstellung` (Schlüssel/Wert) in der Datenbank des Datenordners. Damit gehören sie zum Datenbestand: Nach einem Wechsel des Datenordners gelten die Einstellungen und der Zähler des neuen Ordners.
- Die Startsicherung läuft im Programmstart (`App`) nach der Prüfung des Datenordners und vor dem Erzeugen des Hauptfensters. Sie läuft im Hintergrund mit Wartecursor und einem Zeitlimit von 20 s, damit ein ausgeschaltetes NAS den Start nicht blockiert.
- Ablauf beim Start:
  1. Den Zähler um 1 erhöhen.
  2. Ist er mindestens so groß wie das Intervall und enthält die Datenbank Einheiten, wird gesichert.
  3. Danach werden alte Sicherungen entfernt und der Zähler auf 0 gesetzt.
  4. Bei einem Fehler bleibt der Zähler stehen, damit die Sicherung beim nächsten Start erneut fällig ist. Es erscheint eine Warnung, danach startet die App normal.
- Die Kopie wird über die Online-Sicherung von SQLite (`SqliteConnection.BackupDatabase`) erstellt. Sie ist auch bei geöffneter Datenbank in sich stimmig. Sie wird zuerst unter einem temporären Namen geschrieben und erst danach umbenannt. So gilt eine abgebrochene Kopie nie als Sicherung.
- Der Dateiname lautet `andytours_JJJJ-MM-TT_hh-mm-ss.db`. Beim Aufräumen zählen nur Dateien, die genau diesem Muster entsprechen; alle anderen Dateien im Ordner bleiben unberührt. Die ältesten werden nach dem Zeitstempel im Namen bestimmt.
- In den Einstellungen kommt die Gruppe „Datensicherung“ hinzu: Sicherungsordner mit „Ändern…“ (Ordnerdialog), Intervall und Höchstzahl als Eingabefelder. Ein gültiger Wert wird bei der Eingabe sofort gespeichert. Ein ungültiger Wert wird nicht gespeichert, und ein Hinweis nennt den Bereich 1 bis 99. Anders als der Datenordner ist der Sicherungsordner auch im Debug-Build änderbar.

## Dateien
- `src/AndyTours.Core/Modell/Sicherungseinstellungen.cs` (neu) — record mit Sicherungsordner (`null` = Standard), Intervall, Höchstzahl und Starts seit der letzten Sicherung. Enthält die Standardwerte 3 / 10 und den Standard-Unterordner `Sicherungen`.
- `src/AndyTours.Core/Berechnungen/PruefeSicherungswert.cs` (neu) — Eingabetext → ganze Zahl 1–99 oder `null`
- `src/AndyTours.Core/Berechnungen/IstSicherungFaellig.cs` (neu) — Starts seit der Sicherung, Intervall und Anzahl Einheiten → fällig ja/nein
- `src/AndyTours.Daten/ErstelleSchema.cs` (ändern) — Tabelle `Einstellung (Schluessel TEXT PRIMARY KEY, Wert TEXT NOT NULL)`, Schemaversion 2; bestehende Datenbanken erhalten die Tabelle per `CREATE TABLE IF NOT EXISTS`
- `src/AndyTours.Daten/LadeSicherungseinstellungen.cs` (neu) — liest die Einstellungen; fehlende Werte → Standardwerte
- `src/AndyTours.Daten/SpeichereSicherungseinstellungen.cs` (neu) — schreibt die Einstellungen
- `src/AndyTours.Daten/Datensicherung/Sicherungsdatei.cs` (neu) — Konstanten: Präfix, Zeitstempelformat, Endung
- `src/AndyTours.Daten/Datensicherung/ErmittleSicherungsordner.cs` (neu) — gewählter Ordner oder `<Datenordner>\Sicherungen`
- `src/AndyTours.Daten/Datensicherung/ErstelleSicherung.cs` (neu) — legt den Ordner bei Bedarf an, erstellt die Online-Sicherung unter einem temporären Namen und benennt sie dann um; liefert den Dateipfad
- `src/AndyTours.Daten/Datensicherung/EntferneAlteSicherungen.cs` (neu) — löscht die ältesten Dateien nach dem Namensmuster, bis die Höchstzahl erreicht ist
- `src/AndyTours.Daten/Datensicherung/Startsicherungsergebnis.cs` (neu) — Ergebnis: Gesichert (Pfad), NichtFaellig, Fehlgeschlagen (Meldungstext)
- `src/AndyTours.Daten/Datensicherung/FuehreStartsicherungDurch.cs` (neu) — Ablauf beim Start (Zähler, Fälligkeit, Sichern, Aufräumen, Zähler zurücksetzen); asynchron mit Zeitlimit als Parameter
- `src/AndyTours.App/App.xaml.cs` (ändern) — Startsicherung vor dem Hauptfenster aufrufen, bei einem Fehler eine Warnung zeigen und den Start fortsetzen
- `src/AndyTours.App/ViewModels/EinstellungenViewModel.cs` (ändern) — Sicherungsordner, Intervall- und Höchstzahl-Text, Hinweistexte, Befehl „Sicherungsordner ändern“; liest und speichert die Einstellungen im aktuellen Datenordner und lädt sie nach einem Datenordnerwechsel neu
- `src/AndyTours.App/Views/EinstellungenFenster.xaml` (ändern) — Gruppe „Datensicherung“ mit Ordner und „Ändern…“, zwei Eingabefeldern und den Hinweisen; Kopfkommentar anpassen
- `docs/Konventionen.md` (ändern) — Regel: Einstellungen, die zum Datenbestand gehören, liegen in der Tabelle `Einstellung` der Datenbank im Datenordner
- `tests/AndyTours.Tests/Datensicherung/PruefeSicherungswertTests.cs` (neu)
- `tests/AndyTours.Tests/Datensicherung/IstSicherungFaelligTests.cs` (neu)
- `tests/AndyTours.Tests/Datensicherung/SicherungseinstellungenTests.cs` (neu) — Standardwerte bei leerer DB, Speichern und erneutes Laden
- `tests/AndyTours.Tests/Datensicherung/ErstelleSicherungTests.cs` (neu) — Sicherung ist vollständig, Name mit Zeitstempel, Ordner wird angelegt
- `tests/AndyTours.Tests/Datensicherung/EntferneAlteSicherungenTests.cs` (neu)
- `tests/AndyTours.Tests/Datensicherung/FuehreStartsicherungDurchTests.cs` (neu) — Ablauf über mehrere Starts

## Abdeckung der Kriterien
- Wenn die App startet und seit der letzten Sicherung die eingestellte Zahl von Programmstarts erreicht ist, dann legt sie vor der Anzeige der Touren eine Kopie der Datenbank mit Datum und Uhrzeit im Namen im Sicherungsordner an.
  - Umsetzung: `FuehreStartsicherungDurch`, `IstSicherungFaellig`, `ErstelleSicherung`, `App.xaml.cs`
  - Prüfung: `FuehreStartsicherungDurchTests` (Intervall 3: Kopie erst beim 3. Start, Name nach dem Muster) und `IstSicherungFaelligTests`
- Wenn die eingestellte Zahl von Programmstarts noch nicht erreicht ist, dann legt die App keine Kopie an.
  - Umsetzung: `IstSicherungFaellig`, `FuehreStartsicherungDurch`
  - Prüfung: `FuehreStartsicherungDurchTests` (1. und 2. Start ohne Kopie) und `IstSicherungFaelligTests`
- Wenn eine Sicherung angelegt wurde, dann beginnt die Zählung der Programmstarts von vorn.
  - Umsetzung: `FuehreStartsicherungDurch`
  - Prüfung: `FuehreStartsicherungDurchTests` (Starts 4 und 5 ohne Kopie, Start 6 mit Kopie)
- Wenn eine Sicherung angelegt wurde, dann ist sie vollständig: zurückgespielt enthält sie alle Touren, Räder und Wartungen mit dem Stand zum Zeitpunkt der Sicherung.
  - Umsetzung: `ErstelleSicherung` (Online-Sicherung)
  - Prüfung: `ErstelleSicherungTests` (die Sicherung mit `LadeDatenbestand` öffnen; Einheiten, Equipments und Wartungen gleichen dem Original)
- Wenn danach mehr Sicherungen vorhanden sind als die eingestellte Höchstzahl, dann löscht die App die ältesten; andere Dateien im Sicherungsordner bleiben unberührt.
  - Umsetzung: `EntferneAlteSicherungen`, `Sicherungsdatei`
  - Prüfung: `EntferneAlteSicherungenTests` (12 Sicherungen bei Höchstzahl 10 → die 2 ältesten gelöscht, fremde Dateien und ähnlich benannte Dateien bleiben)
- Wenn die Sicherung fehlschlägt (Ordner nicht erreichbar, nicht beschreibbar oder Datenträger voll), dann erscheint eine verständliche Meldung, die App startet normal weiter und versucht es beim nächsten Start erneut.
  - Umsetzung: `FuehreStartsicherungDurch` (Ergebnis Fehlgeschlagen, Zähler bleibt), `App.xaml.cs` (Warnung, Start geht weiter)
  - Prüfung: `FuehreStartsicherungDurchTests` (Sicherungsordner auf einem nicht vorhandenen Laufwerk und als schreibgeschützter Ordner → Fehlgeschlagen; der nächste Start versucht es erneut) und manuell: Release-Build, Sicherungsordner auf eine ausgeschaltete NAS-Freigabe setzen und neu starten; die Warnung erscheint, das Hauptfenster öffnet.
- Wenn die Datenbank noch keine Touren enthält, dann legt die App keine Sicherung an.
  - Umsetzung: `IstSicherungFaellig`, `FuehreStartsicherungDurch`
  - Prüfung: `FuehreStartsicherungDurchTests` (leere Datenbank, mehrere Starts → kein Sicherungsordner, keine Kopie)
- Wenn der Benutzer in den Einstellungen nichts gewählt hat, dann wird alle 3 Programmstarts in den Ordner `Sicherungen` im Datenordner gesichert und es werden höchstens 10 Sicherungen aufbewahrt.
  - Umsetzung: `Sicherungseinstellungen`, `LadeSicherungseinstellungen`, `ErmittleSicherungsordner`
  - Prüfung: `SicherungseinstellungenTests` (Standardwerte) und `FuehreStartsicherungDurchTests` (ohne Einstellungen: Kopie beim 3. Start in `<Datenordner>\Sicherungen`)
- Wenn der Benutzer in den Einstellungen einen Sicherungsordner wählt, dann sichert die App künftig dorthin, und die Wahl bleibt nach einem Neustart erhalten.
  - Umsetzung: `EinstellungenViewModel`, `SpeichereSicherungseinstellungen`, `ErmittleSicherungsordner`
  - Prüfung: `SicherungseinstellungenTests` (Ordner speichern und neu laden) und manuell: Ordner wählen, App beenden und neu starten; Einstellungen zeigen den Ordner, und die nächste Sicherung liegt dort.
- Wenn der Benutzer in den Einstellungen das Intervall (Anzahl der Programmstarts) oder die Höchstzahl der Sicherungen ändert, dann gelten die neuen Werte und bleiben nach einem Neustart erhalten.
  - Umsetzung: `EinstellungenViewModel`, `SpeichereSicherungseinstellungen`
  - Prüfung: `SicherungseinstellungenTests` und manuell: Intervall 1 und Höchstzahl 2 setzen, dreimal starten; jeder Start sichert, es bleiben 2 Dateien, und die Werte stehen nach dem Neustart weiter in den Einstellungen.
- Wenn der Benutzer für das Intervall oder die Höchstzahl einen Wert außerhalb von 1 bis 99 oder keine ganze Zahl eingibt, dann wird der Wert nicht übernommen und ein Hinweis nennt den erlaubten Bereich 1 bis 99.
  - Umsetzung: `PruefeSicherungswert`, `EinstellungenViewModel`, `EinstellungenFenster.xaml`
  - Prüfung: `PruefeSicherungswertTests` (0, 100, -1, „abc“, „2,5“, leer → `null`; 1, 99, „ 7 “ → Zahl) und manuell: „0“ eingeben; der Hinweis erscheint, und nach dem Schließen und erneuten Öffnen steht dort wieder der alte Wert.
- Wenn der Benutzer den Datenordner wechselt, dann sichert die App künftig die Datenbank des neuen Ordners.
  - Umsetzung: `FuehreStartsicherungDurch` (Datenordner als Parameter, Einstellungen aus dessen Datenbank)
  - Prüfung: `FuehreStartsicherungDurchTests` (zwei Datenordner; die Sicherung enthält jeweils die Einheiten des übergebenen Ordners) und manuell: Release-Build, Datenordner wechseln, mehrmals neu starten; die Sicherung entsteht im Ordner `Sicherungen` des neuen Datenordners.

## Entscheidungen
- Einstellungen, die zum Datenbestand gehören, liegen in der Datenbank des Datenordners (Tabelle `Einstellung`), nicht unter `%APPDATA%` (Benutzerwahl a). Sie wandern mit dem Datenordner mit, gelten auch im Debug-Build und dienen später auch für persönliche Daten und Saisonziele.
- Der Zähler zählt die Programmstarts, auch bei leerer Datenbank. Bei einem Fehler bleibt er stehen, sodass die Sicherung beim nächsten Start sofort wieder fällig ist.
- Eine kleinere Höchstzahl wirkt erst bei der nächsten Sicherung (Kriterium „danach“), nicht schon beim Speichern der Einstellung.
- Gültige Werte werden bei der Eingabe sofort gespeichert, ohne eigenes „OK“, wie beim Datenordner. Beim Verlassen des Feldes zu speichern hätte Eingaben verloren, die mit Enter oder Esc bestätigt werden (bei der Umsetzung geändert).
- Kein Fortschrittsfenster: Die Datenbank ist rund 0,3 MB groß, der Wartecursor genügt.
- Der gewählte Sicherungsordner wird beim Auswählen nicht geprüft. Ein unbrauchbarer Ordner fällt bei der nächsten Sicherung als Fehler auf (Kriterium „fehlschlägt“).

## Risiken
- Ein hängendes NAS als Sicherungsordner verzögert den Start um bis zu 20 s (Zeitlimit).
- Liegen die Einstellungen in der Datenbank, steht der Zähler auch in der Sicherung. Spielt man eine Sicherung später zurück, beginnt die Zählung auf deren Stand. Das ist unkritisch, sollte aber bei `sicherung-wiederherstellen` beachtet werden.
- Wählt der Benutzer als Sicherungsordner den Datenordner selbst oder einen seiner Unterordner, wandern die Sicherungen beim Kopieren des Datenordners mit. Das ist beim Standardordner `Sicherungen` so gewollt.
