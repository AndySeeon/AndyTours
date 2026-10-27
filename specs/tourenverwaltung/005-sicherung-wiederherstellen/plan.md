# Plan: 005 – Sicherung wiederherstellen

## Vorgehen
- Neuer Menüpunkt „Extras → Sicherung wiederherstellen…“ im Hauptfenster. Er ermittelt den Sicherungsordner des aktuellen Datenordners (`LadeSicherungseinstellungen` + `ErmittleSicherungsordner`) und listet darin alle Sicherungen, also nur Dateien nach dem Namensmuster `andytours_JJJJ-MM-TT_hh-mm-ss.db`. Das Lesen des Ordners läuft im Hintergrund mit Wartecursor und dem Zeitlimit von 20 s, damit ein ausgeschaltetes NAS die App nicht blockiert.
- Ein neues Auswahlfenster zeigt die Sicherungen mit Wochentag, Datum und Uhrzeit, die neueste zuerst. Es hat die Schaltflächen „Wiederherstellen…“ (auch per Doppelklick) und „Abbrechen“. Gibt es keine Sicherung oder ist der Ordner nicht erreichbar, erscheint stattdessen ein Hinweis.
- Nach der Auswahl folgt eine Rückfrage mit Ja/Nein, vorbelegt ist „Nein“. Sie nennt Datum und Uhrzeit und sagt, dass spätere Änderungen verloren gehen und der aktuelle Stand vorher gesichert wird.
- Das Zurückspielen (`SpieleSicherungZurueck`, im Projekt Daten) läuft in dieser Reihenfolge. Bis Schritt 4 bleibt die Datenbank im Datenordner unberührt:
  1. **Sicherung prüfen.** Die gewählte Datei wird nur lesend geöffnet und nie mit `OeffneDatenbank`, weil das Schema sonst in eine fremde Datei geschrieben würde. Geprüft werden:
     - Lesbarkeit und `PRAGMA quick_check`,
     - Schemaversion (`PRAGMA user_version`) zwischen 1 und der aktuellen,
     - Vorhandensein der Kerntabellen.
  2. **Aktuellen Stand sichern** mit `ErstelleSicherung` in den Sicherungsordner. Schlägt das fehl, wird abgebrochen.
  3. **Neue Datenbank vorbereiten.** Die gewählte Sicherung wird per Online-Sicherung von SQLite in eine Zwischendatei im Datenordner kopiert (`andytours.db.wiederherstellen.tmp`). Darauf laufen:
     - `ErstelleSchema`: Eine ältere Sicherung erhält so fehlende Tabellen und die aktuelle Schemaversion.
     - Die vorher gelesenen Sicherungseinstellungen werden hineingeschrieben (Ordner, Intervall, Höchstzahl). Der Startzähler wird auf 0 gesetzt, weil gerade gesichert wurde.
  4. **Austausch** per `File.Move(..., overwrite: true)`. Bei einem Fehler wird die Zwischendatei entfernt, die bisherige Datenbank bleibt unverändert.
  5. **Aufräumen** mit `EntferneAlteSicherungen` erst nach dem Austausch. So kann die gewählte Sicherung nicht vor dem Zurückspielen gelöscht werden. Fehler beim Aufräumen werden ignoriert, die nächste Sicherung räumt erneut auf.
- Nach Erfolg lädt das Hauptfenster den Datenbestand neu. Jahre, Aktivitäten, Summen und Statusleiste zeigen dann den Stand der Sicherung. Eine Meldung nennt außerdem die zusätzliche Sicherung des vorherigen Stands; damit lässt sich das Wiederherstellen rückgängig machen.
- Das Ermitteln der Sicherungen nach Namensmuster wird aus `EntferneAlteSicherungen` in eine eigene Funktion `ErmittleSicherungen` herausgelöst. Auswahlliste und Aufräumen verwenden dann dieselbe Regel.

## Dateien
- `src/AndyTours.Daten/Datensicherung/Sicherungseintrag.cs` (neu) — record aus Dateipfad und Zeitpunkt aus dem Namen
- `src/AndyTours.Daten/Datensicherung/ErmittleSicherungen.cs` (neu) — alle Sicherungen eines Ordners nach dem Namensmuster, neueste zuerst; ein fehlender Ordner ergibt eine leere Liste
- `src/AndyTours.Daten/Datensicherung/EntferneAlteSicherungen.cs` (ändern) — nutzt `ErmittleSicherungen`
- `src/AndyTours.Daten/Datensicherung/Sicherungspruefung.cs` (neu) — Aufzählung `Brauchbar`, `Unbrauchbar`, `NeuereVersion`
- `src/AndyTours.Daten/Datensicherung/PruefeSicherung.cs` (neu) — öffnet die Datei nur lesend und prüft Lesbarkeit, Unversehrtheit, Schemaversion und Kerntabellen
- `src/AndyTours.Daten/Datensicherung/Wiederherstellungsstatus.cs` (neu) — Aufzählung `Wiederhergestellt`, `Unbrauchbar`, `NeuereVersion`, `SicherungFehlgeschlagen`, `Fehlgeschlagen`
- `src/AndyTours.Daten/Datensicherung/Wiederherstellungsergebnis.cs` (neu) — record aus Status, Pfad der zusätzlichen Sicherung und Fehlergrund
- `src/AndyTours.Daten/Datensicherung/SpieleSicherungZurueck.cs` (neu) — Ablauf aus Prüfen, Sichern, Vorbereiten, Austauschen und Aufräumen (siehe Vorgehen). Erwartete Fehler werden nicht geworfen, sondern als Ergebnis geliefert.
- `src/AndyTours.Daten/Datensicherung/BeschreibeSicherungsfehler.cs` (ändern) — Konstanten für „Datenträger voll“ `internal`, damit `SpieleSicherungZurueck` sie mitbenutzt (bei der Umsetzung ergänzt)
- `src/AndyTours.App/ViewModels/Sicherungszeile.cs` (neu) — record aus Sicherung und Anzeigetext für die Liste (bei der Umsetzung ergänzt)
- `src/AndyTours.App/ViewModels/SicherungAuswahlViewModel.cs` (neu) — Liste der Sicherungen mit Anzeigetext (de-DE), gewählte Sicherung, Befehl „Wiederherstellen…“ nur bei Auswahl
- `src/AndyTours.App/Views/SicherungAuswahlFenster.xaml` (neu) — Liste mit Doppelklick, Schaltflächen „Wiederherstellen…“ und „Abbrechen“
- `src/AndyTours.App/Views/SicherungAuswahlFenster.xaml.cs` (neu) — verbindet das ViewModel, schließt das Fenster mit `DialogResult`
- `src/AndyTours.App/Funktionen/FuehreWiederherstellungDurch.cs` (neu) — Ablauf mit der Oberfläche: Ordner lesen (Zeitlimit), Hinweis bei keinen Sicherungen, Auswahlfenster, Rückfrage, `SpieleSicherungZurueck` mit Wartecursor, Meldungen; liefert, ob wiederhergestellt wurde
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — Befehl `SicherungWiederherstellen`, danach `LadeDatenbank(Datenordner)`
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Menüpunkt „Sicherung _wiederherstellen…“ unter „Extras“
- `tests/AndyTours.Tests/Datensicherung/ErmittleSicherungenTests.cs` (neu)
- `tests/AndyTours.Tests/Datensicherung/PruefeSicherungTests.cs` (neu)
- `tests/AndyTours.Tests/Datensicherung/SpieleSicherungZurueckTests.cs` (neu)
- `tests/AndyTours.Tests/Datensicherung/TestSicherungen.cs` (neu) — erzeugt ältere, neuere, fremde und beschädigte Sicherungsdateien (bei der Umsetzung ergänzt)

## Abdeckung der Kriterien
- Wenn der Benutzer „Extras → Sicherung wiederherstellen…“ wählt, dann zeigt die App alle Sicherungen des aktuellen Sicherungsordners mit Datum und Uhrzeit, die neueste zuerst.
  - Umsetzung: `Hauptfenster.xaml`, `HauptfensterViewModel`, `FuehreWiederherstellungDurch`, `ErmittleSicherungen`, `SicherungAuswahlFenster`
  - Prüfung: `ErmittleSicherungenTests` (Reihenfolge neueste zuerst; fremde und ähnlich benannte Dateien sowie `.tmp` fehlen in der Liste) und manuell: Menüpunkt wählen; die Liste zeigt die Sicherungen aus `AppDaten\Sicherungen` mit Datum und Uhrzeit, die neueste oben.
- Wenn der Sicherungsordner keine Sicherungen enthält oder nicht erreichbar ist, dann erscheint ein Hinweis und der Datenbestand bleibt unverändert.
  - Umsetzung: `ErmittleSicherungen` (fehlender Ordner → leer), `FuehreWiederherstellungDurch` (Hinweis, auch bei Fehler oder Zeitüberschreitung)
  - Prüfung: `ErmittleSicherungenTests` (fehlender und leerer Ordner → leere Liste) und manuell: Sicherungsordner auf einen leeren Ordner setzen, Menüpunkt wählen; der Hinweis erscheint, und das Hauptfenster bleibt unverändert.
- Wenn der Benutzer die Auswahl abbricht, dann bleibt der Datenbestand unverändert.
  - Umsetzung: `FuehreWiederherstellungDurch` (Abbruch → kein Aufruf von `SpieleSicherungZurueck`)
  - Prüfung: manuell: Auswahlfenster mit „Abbrechen“ bzw. Esc schließen; die Summen sind unverändert, und im Sicherungsordner entsteht keine neue Datei.
- Wenn der Benutzer eine Sicherung auswählt, dann fragt die App nach; die Rückfrage nennt Datum und Uhrzeit der Sicherung und weist darauf hin, dass alle späteren Änderungen verloren gehen.
  - Umsetzung: `FuehreWiederherstellungDurch`
  - Prüfung: manuell: eine Sicherung wählen; die Rückfrage nennt deren Datum und Uhrzeit und den Hinweis auf den Verlust späterer Änderungen.
- Wenn der Benutzer die Rückfrage ablehnt, dann bleibt der Datenbestand unverändert.
  - Umsetzung: `FuehreWiederherstellungDurch` (Nein → Ende)
  - Prüfung: manuell: Rückfrage mit „Nein“ beantworten; die Summen sind unverändert, und im Sicherungsordner entsteht keine neue Datei.
- Wenn der Benutzer bestätigt, dann entspricht der Datenbestand dem Stand der Sicherung, und das Hauptfenster zeigt diesen Stand sofort an (Jahre, Aktivitäten, Summen), ohne Neustart.
  - Umsetzung: `SpieleSicherungZurueck`, `HauptfensterViewModel` (`LadeDatenbank` nach Erfolg)
  - Prüfung: `SpieleSicherungZurueckTests` (danach gleichen Einheiten, Equipments und Wartungen der Sicherung, nicht dem vorherigen Stand) und manuell: in einer Kopie von `AppDaten` eine Sicherung vor einer bekannten Änderung zurückspielen; die Summen springen sofort auf den alten Stand.
- Wenn der Benutzer bestätigt, dann legt die App vorher eine Sicherung des aktuellen Stands im Sicherungsordner an; sie zählt wie jede andere Sicherung zur Höchstzahl.
  - Umsetzung: `SpieleSicherungZurueck` (Schritte 2 und 5)
  - Prüfung: `SpieleSicherungZurueckTests` (die zusätzliche Sicherung enthält den vorherigen Stand; bei Höchstzahl 2 und zwei vorhandenen Sicherungen bleiben danach 2, die älteste ist gelöscht)
- Wenn die Sicherung des aktuellen Stands fehlschlägt, dann erscheint eine verständliche Meldung, und es wird nichts wiederhergestellt.
  - Umsetzung: `SpieleSicherungZurueck` (Status `SicherungFehlgeschlagen` mit `BeschreibeSicherungsfehler`), `FuehreWiederherstellungDurch` (Meldung)
  - Prüfung: `SpieleSicherungZurueckTests` (schreibgeschützter Sicherungsordner → `SicherungFehlgeschlagen`, Datenbank unverändert)
- Wenn die gewählte Sicherung beschädigt ist oder keine AndyTours-Daten enthält, dann erscheint eine verständliche Meldung und der Datenbestand bleibt unverändert.
  - Umsetzung: `PruefeSicherung`, `SpieleSicherungZurueck` (Status `Unbrauchbar`), `FuehreWiederherstellungDurch` (Meldung)
  - Prüfung: `PruefeSicherungTests` (Textdatei, abgeschnittene Datei und SQLite-Datei ohne AndyTours-Tabellen → `Unbrauchbar`; die geprüfte Datei bleibt unverändert) und `SpieleSicherungZurueckTests` (unbrauchbar → Datenbank unverändert, keine zusätzliche Sicherung)
- Wenn das Wiederherstellen fehlschlägt (z. B. Ordner nicht erreichbar, Datenträger voll), dann erscheint eine verständliche Meldung und der bisherige Datenbestand bleibt vollständig erhalten.
  - Umsetzung: `SpieleSicherungZurueck` (Zwischendatei und Austausch erst am Ende, Status `Fehlgeschlagen`), `FuehreWiederherstellungDurch` (Meldung)
  - Prüfung: `SpieleSicherungZurueckTests` (die Datenbankdatei ist von einem anderen Programm geöffnet, Austausch nicht möglich → `Fehlgeschlagen`; der Bestand ist unverändert, keine Zwischendatei bleibt zurück)
- Wenn die Sicherung von einer älteren AndyTours-Version stammt, dann wird sie übernommen und ist danach normal nutzbar.
  - Umsetzung: `PruefeSicherung` (Schemaversion 1 ist brauchbar), `SpieleSicherungZurueck` (`ErstelleSchema` auf der Zwischendatei)
  - Prüfung: `PruefeSicherungTests` und `SpieleSicherungZurueckTests` (Sicherung mit Schemaversion 1 ohne Tabelle `Einstellung` → wiederhergestellt, ladbar, Schemaversion 2, Einstellungen speicherbar)
- Wenn die Sicherung von einer neueren AndyTours-Version stammt, dann lehnt die App sie mit einer Meldung ab und der Datenbestand bleibt unverändert.
  - Umsetzung: `PruefeSicherung` (Schemaversion größer als die aktuelle → `NeuereVersion`), `FuehreWiederherstellungDurch` (Meldung)
  - Prüfung: `PruefeSicherungTests` und `SpieleSicherungZurueckTests` (Schemaversion 3 → `NeuereVersion`, Datenbank unverändert)
- Wenn wiederhergestellt wurde, dann bleiben die Einstellungen der Datensicherung (Sicherungsordner, Intervall, Höchstzahl) so, wie sie vor dem Wiederherstellen waren.
  - Umsetzung: `SpieleSicherungZurueck` (Schritt 3)
  - Prüfung: `SpieleSicherungZurueckTests` (die Sicherung hat Intervall 3 und Höchstzahl 10, die aktuelle Datenbank Intervall 5, Höchstzahl 7 und einen eigenen Ordner; danach gelten 5, 7 und dieser Ordner)

## Entscheidungen
- Eigenes Auswahlfenster statt des Windows-Dateidialogs. Nur so lassen sich ausschließlich Sicherungen anzeigen, verständlich mit Datum und Uhrzeit und in fester Reihenfolge.
- Die neue Datenbank wird vollständig in einer Zwischendatei im Datenordner vorbereitet und erst am Ende in einem Schritt ausgetauscht. Bis dahin bleibt der bisherige Bestand unberührt, auch bei einem Abbruch mittendrin.
- Der Startzähler steht nach dem Wiederherstellen auf 0, weil der vorherige Stand gerade gesichert wurde.
- Aufräumen erst nach dem Austausch. Fehler beim Aufräumen gelten nicht als Fehlschlag des Wiederherstellens.
- Das Zeitlimit von 20 s gilt nur für das Lesen des Sicherungsordners. Das Zurückspielen selbst läuft ohne Zeitlimit, denn ein abgebrochener Hintergrundvorgang könnte den Austausch sonst noch nach der Fehlermeldung ausführen.
- Erkannt wird die Sicherung einer neueren Version an der Schemaversion. Eine neuere Programmversion mit unverändertem Schema gilt als passend, was technisch auch zutrifft.

## Risiken
- Ein NAS, das während des Zurückspielens ausfällt, kann die App hängen lassen, bis Windows den Zugriff aufgibt (meist unter einer Minute). Der bisherige Bestand bleibt dabei erhalten.
- Liegt der Sicherungsordner auf demselben Datenträger wie der Datenordner (Standard `Sicherungen`), schützt die Sicherung nicht vor einem Ausfall dieses Datenträgers. Das gilt schon seit 004 und ist nicht Teil dieser Funktion.
