# Plan: 014 – Saisonziele

## Vorgehen
Die Ziele bekommen eine eigene Tabelle `Saisonziel` mit Aktivität, „ab Jahr“, km, hm und Anzahl. Die aus BikeXperience übernommenen Ziele stehen weiter in der Tabelle `Aktivitaet` und dienen als **Grundziel**: Es gilt, solange die Aktivität für kein Jahr bis einschließlich des gefragten Jahres ein eigenes Ziel hat. So müssen keine Altdaten umgewandelt werden.

- **Core:**
  - `ErmittleSaisonziel` liefert das wirksame Ziel einer Aktivität für ein Jahr und seine Herkunft: eigenes Ziel, übernommen aus einem früheren Jahr oder übernommen aus BikeXperience. `jahresfortschritt` wird das später ebenfalls nutzen.
  - `SaisonzielEingaben` wandelt Ziele in Feldtexte um und zurück und prüft die Grenzen.
- **Daten:**
  - Die Tabelle `Saisonziel` kommt mit Schemaversion 3. Ältere Datenbanken und Sicherungen erhalten sie beim Öffnen automatisch, wie bei Version 2.
  - Beim Löschen einer Aktivität verschwinden ihre Ziele über `ON DELETE CASCADE`.
  - `SaisonzieleTabelle` lädt alle Ziele, speichert den ganzen Bestand und ersetzt in einer Transaktion die eigenen Ziele der gepflegten Jahre.
- **App:**
  - Das Fenster „Saisonziele“ zeigt eine Jahresauswahl (laufendes und folgendes Jahr) und je Aktivität eine Zeile mit km, hm, Anzahl und Herkunft.
  - Es arbeitet auf einer Arbeitskopie der Ziele. Beim Jahreswechsel bleiben Änderungen erhalten, und die Herkunft des anderen Jahres wird aus der Arbeitskopie neu berechnet. „OK“ speichert beide Jahre gemeinsam.
  - Erst nach erfolgreichem Schreiben werden die Ziele im Datenbestand ersetzt.

## Dateien
- `src/AndyTours.Core/Modell/Saisonziel.cs` (neu) — eigenes Ziel: AktivitaetId, AbJahr, ZielKm, ZielHm, ZielAnzahl (ganze Zahlen, 0 = kein Ziel)
- `src/AndyTours.Core/Modell/Datenbestand.cs` (ändern) — Liste `Saisonziele`
- `src/AndyTours.Core/Berechnungen/ErmittleSaisonziel.cs` (neu) — wirksames Ziel einer Aktivität für ein Jahr aus einer Zielliste
  - Gilt das jüngste eigene Ziel mit `AbJahr` ≤ Jahr, dann eigenes Ziel bzw. übernommen aus diesem Jahr.
  - Sonst gilt das Grundziel der Aktivität, gekennzeichnet als übernommen aus BikeXperience.
  - Ist auch kein Grundziel vorhanden, gibt es kein Ziel; dann gibt es keine Kennzeichnung.
- `src/AndyTours.Core/Berechnungen/SaisonzielEingaben.cs` (neu) — Ziel als Feldtexte (0 ergibt ein leeres Feld); Feldtexte prüfen (ganze Zahl, Grenzen) und in Zahlen umwandeln, mit Hinweis zum ersten Fehler
- `src/AndyTours.Core/Berechnungen/FuehreAktivitaetenZusammen.cs` (ändern) — entfernt die Ziele der Ursprungsaktivität aus dem Bestand
- `src/AndyTours.Daten/ErstelleSchema.cs` (ändern) — Tabelle `Saisonziel` (Schlüssel: Aktivität und AbJahr, Verweis mit `ON DELETE CASCADE`), Schemaversion 3
- `src/AndyTours.Daten/SaisonzieleTabelle.cs` (neu) — `Laden`, `Speichern` (ganzer Bestand), `SpeichereJahre(verbindung, jahre, ziele)` (in einer Transaktion die Ziele dieser Jahre löschen und neu schreiben)
- `src/AndyTours.Daten/DatenbestandTabellen.cs` (ändern) — Saisonziele laden und speichern
- `src/AndyTours.App/ViewModels/Saisonzielzeile.cs` (neu) — Zeile: Aktivität, Texte für km, hm und Anzahl, Herkunft, „geändert“
- `src/AndyTours.App/ViewModels/SaisonzieleViewModel.cs` (neu) — Jahre, gewähltes Jahr (Wechsel nur ohne Hinweis), Zeilen, Hinweis, „OK“ (nur ohne Hinweis), Arbeitskopie und Ergebnis
- `src/AndyTours.App/Views/SaisonzieleFenster.xaml` (neu) — Jahresauswahl, Tabelle Aktivität | km | hm | Anzahl | Herkunft, roter Hinweis, „OK“ und „Abbrechen“
- `src/AndyTours.App/Views/SaisonzieleFenster.xaml.cs` (neu) — schließt nach `Gespeichert`
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Saisonziele.cs` (neu) — Befehl `SaisonzieleOeffnen`, Speichern über `Schreibe`, danach Bestand ersetzen
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Auswahllisten.cs` (ändern) — beim Löschen einer Aktivität ihre Ziele auch aus dem Bestand entfernen
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — „_Saisonziele…“ im Menü „Extras“ nach „Persönliche Daten…“
- `tests/AndyTours.Tests/Berechnungen/ErmittleSaisonzielTests.cs` (neu) — eigenes Ziel, übernommen aus Vorjahr, aus BikeXperience, kein Ziel, 0 überschreibt
- `tests/AndyTours.Tests/Berechnungen/SaisonzielEingabenTests.cs` (neu) — Texte, leere Felder, Grenzen und Hinweise
- `tests/AndyTours.Tests/Berechnungen/FuehreZusammenTests.cs` (ändern) — Ziele der Ursprungsaktivität verschwinden
- `tests/AndyTours.Tests/Daten/SaisonzieleTests.cs` (neu) — Jahre ersetzen (andere Jahre bleiben), Laden nach Speichern, Kaskade beim Löschen der Aktivität, Datenbestand mit Zielen

## Abdeckung der Kriterien
- Wenn der Benutzer „Extras → Saisonziele…“ wählt, dann zeigt ein Fenster für das gewählte Jahr alle Aktivitäten mit ihren Zielen für Kilometer, Höhenmeter und Anzahl; vorgewählt ist das laufende Jahr.
  - Umgesetzt: `Hauptfenster.xaml`, `HauptfensterViewModel.Saisonziele`, `SaisonzieleViewModel`, `SaisonzieleFenster`
  - Geprüft: manuell – Menü wählen, alle Aktivitäten mit drei Werten, Jahr 2026 vorgewählt.
- Wenn der Benutzer im Fenster das Jahr wählt, dann stehen nur das laufende und das folgende Jahr zur Wahl, und das Fenster zeigt die Ziele des gewählten Jahres.
  - Umgesetzt: `SaisonzieleViewModel` (Jahresliste, Neuberechnung der Zeilen)
  - Geprüft: manuell – Liste enthält genau 2026 und 2027; Wechsel zeigt die Werte des Jahres.
- Wenn für ein Jahr kein eigenes Ziel eingetragen ist, dann gilt das Ziel des nächstfrüheren Jahres, und das Fenster kennzeichnet es als „übernommen aus <Jahr>“ bzw. bei einem Ziel aus BikeXperience als „übernommen aus BikeXperience“.
  - Umgesetzt: `ErmittleSaisonziel`, Spalte „Herkunft“
  - Geprüft: `ErmittleSaisonzielTests`; manuell – 2026 ändern, 2027 zeigt „übernommen aus 2026“.
- Wenn das Fenster zum ersten Mal geöffnet wird, dann zeigt es die aus BikeXperience übernommenen Ziele; sie gelten für alle Jahre bis zur ersten Änderung.
  - Umgesetzt: Grundziel aus `Aktivitaet` in `ErmittleSaisonziel`
  - Geprüft: `ErmittleSaisonzielTests`; manuell – Werte mit BikeXperience vergleichen.
- Wenn eine Aktivität kein Ziel hat, dann sind ihre Felder leer.
  - Umgesetzt: `SaisonzielEingaben` (0 ergibt ein leeres Feld)
  - Geprüft: `SaisonzielEingabenTests`
- Wenn der Benutzer Ziele ändert und „OK“ wählt, dann gelten sie ab dem gewählten Jahr; frühere Jahre bleiben unverändert, ein späteres Jahr ohne eigenes Ziel übernimmt sie, und nach einem Neustart sind sie noch da.
  - Umgesetzt: geänderte Zeilen werden eigene Ziele mit diesem `AbJahr`; `SaisonzieleTabelle.SpeichereJahre` schreibt nur das laufende und das folgende Jahr.
  - Geprüft: `SaisonzieleTests`, `ErmittleSaisonzielTests`; manuell – ändern, neu starten, prüfen.
- Wenn der Benutzer ein Feld leert oder 0 einträgt, dann hat die Aktivität ab diesem Jahr dafür kein Ziel.
  - Umgesetzt: eigenes Ziel mit 0; es verdeckt frühere Ziele.
  - Geprüft: `ErmittleSaisonzielTests` (0 überschreibt), `SaisonzielEingabenTests`
- Wenn der Benutzer „Abbrechen“ wählt, dann bleibt alles unverändert.
  - Umgesetzt: Das Fenster arbeitet auf einer Arbeitskopie; geschrieben wird nur bei „OK“.
  - Geprüft: manuell – in beiden Jahren ändern, „Abbrechen“, erneut öffnen.
- Wenn eine Aktivität neu angelegt wird, dann hat sie zunächst keine Ziele; wird sie gelöscht, verschwinden ihre Ziele mit ihr.
  - Umgesetzt: Neue Aktivität hat Grundziel 0 und keine Zeilen in `Saisonziel`; Kaskade in der Datenbank, Bestand in `HauptfensterViewModel.Auswahllisten` bzw. `FuehreAktivitaetenZusammen`
  - Geprüft: `SaisonzieleTests` (Kaskade), `FuehreZusammenTests`; manuell – Aktivität anlegen, Fenster zeigt leere Zeile.
- Wenn ein Wert keine ganze Zahl ist oder außerhalb der Grenzen liegt (Kilometer 0 bis 100.000, Höhenmeter 0 bis 2.000.000, Anzahl 0 bis 1.000), dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
  - Umgesetzt: `SaisonzielEingaben`; Hinweis nennt Aktivität und Feld, „OK“ und Jahreswechsel gesperrt
  - Geprüft: `SaisonzielEingabenTests` (jeweils beide Grenzen, Kommazahl, Text)
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, das Fenster bleibt offen, und die gespeicherten Ziele bleiben unverändert.
  - Umgesetzt: `SpeichereJahre` in einer Transaktion über `Schreibe`. Der Bestand wird erst nach Erfolg ersetzt, das Fenster schließt nur dann.
  - Geprüft: Der Fehlerweg (`Schreibe`) ist derselbe wie bei 008/009.

## Entscheidungen
- **Jahreswechsel (E1):** Änderungen bleiben beim Wechsel zwischen den beiden Jahren erhalten, und „OK“ speichert beide gemeinsam. Gewechselt werden kann nur, solange alle Eingaben gültig sind.
- **Kennzeichnung (F1):** Ein Ziel direkt aus BikeXperience heißt „übernommen aus BikeXperience“.
- **Eigenes Ziel erst bei Änderung:** Eine Zeile wird erst dann zum eigenen Ziel des Jahres, wenn eines ihrer Felder geändert wurde. Ein schon vorhandenes eigenes Ziel bleibt eigenes Ziel. „OK“ ohne Änderung legt nichts an.
- **Grundziel bleibt in `Aktivitaet`:** Die Ziele werden nicht in die neue Tabelle umgezogen. Das spart eine Datenumwandlung, und ältere Sicherungen bleiben brauchbar.
- **Zusammenführen:** Die Ziele der Ursprungsaktivität verschwinden; die Ziele der verbleibenden Aktivität gelten.

## Risiken
- Schemaversion 3: Eine Sicherung mit Version 3 lässt sich in einer älteren AndyTours-Version nicht mehr zurückspielen. Das ist bereits so vorgesehen, die Prüfung „neuere Programmversion“ greift dann.
