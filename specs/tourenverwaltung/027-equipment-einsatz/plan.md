# Plan: 027 – Einsatz je Equipment

## Vorgehen
Eine neue Fachfunktion `ErmittleEquipmenteinsatz` im Core berechnet aus allen Einheiten und Equipments des Datenbestands die Zeilen der Auskunft (`Einsatzzeile`). Sie bildet eine Zeile je Equipment, das eine Einheit oder Anfangskilometer hat, mit den Werten „vorher (km)“, „Gesamt (km)“ und „Gesamt (hm)“. „Gesamt (km)“ sind die Anfangskilometer plus die Distanzen, wie bei `BerechneKmStand`. Dazu kommen die Zeile „ohne Equipment“, falls es Einheiten ohne Equipment gibt, und zuletzt „Summe“. Den Namen bildet `EquipmentAnzeigename.Bilden`. Die Zeilen werden gleich in der Anfangsreihenfolge geliefert: erst die Equipments im Einsatz, dann die ausgemusterten, jeweils absteigend nach „Gesamt (km)“, danach „ohne Equipment“ und „Summe“. Gibt es nichts zu zeigen, liefert die Funktion eine leere Liste.

Eine zweite Fachfunktion `SortiereEinsatzzeilen` sortiert die Zeilen nach einer Spalte (`Einsatzspalte`) auf- oder absteigend und lässt „Summe“ dabei unten. Das Fenster fängt das Sortieren der Tabelle ab (Ereignis `Sorting`). Es lässt das ViewModel sortieren und setzt den Sortierpfeil selbst. Die eingebaute Sortierung würde „Summe“ mitsortieren.

Das Fenster „Equipment-Einsatz“ zeigt die Zeilen schreibgeschützt in einer Tabelle, die Zahlen rechtsbündig mit `N0`. Bei leerer Liste erscheint stattdessen der Hinweis „Keine Einheiten vorhanden.“. Der Befehl „Extras → Equipment-Einsatz…“ im Hauptfenster übergibt den gesamten aktuellen Datenbestand, also unabhängig von der Jahres- und Aktivitätsauswahl.

## Dateien
- `src/AndyTours.Core/Modell/Einsatzzeile.cs` (neu) — eine Zeile der Auskunft: Name, vorher (km), Gesamt (km), Gesamt (hm), Kennzeichen „Summe“; Werte 0 als `null` (leere Zelle)
- `src/AndyTours.Core/Modell/Einsatzspalte.cs` (neu) — die sortierbaren Spalten
- `src/AndyTours.Core/Berechnungen/ErmittleEquipmenteinsatz.cs` (neu) — berechnet die Zeilen in Anfangsreihenfolge
- `src/AndyTours.Core/Berechnungen/SortiereEinsatzzeilen.cs` (neu) — sortiert nach einer Spalte, „Summe“ bleibt unten
- `src/AndyTours.App/ViewModels/EquipmentEinsatzViewModel.cs` (neu) — Zeilen, „hat Zeilen“, Sortieren
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.EquipmentEinsatz.cs` (neu) — Befehl „Equipment-Einsatz…“ öffnet das Fenster
- `src/AndyTours.App/Views/EquipmentEinsatzFenster.xaml` (neu) — Tabelle bzw. Hinweis, Schaltfläche „Schließen“
- `src/AndyTours.App/Views/EquipmentEinsatzFenster.xaml.cs` (neu) — Sortieren abfangen, Sortierpfeile setzen
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Menüeintrag „Extras → Equipment-Einsatz…“
- `tests/AndyTours.Tests/Berechnungen/ErmittleEquipmenteinsatzTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/SortiereEinsatzzeilenTests.cs` (neu)

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Aufruf und Aufbau
- **Kriterium:** Wenn im Menü „Extras“ der Eintrag „Equipment-Einsatz…“ gewählt wird, dann öffnet sich ein Fenster mit einer Tabelle: eine Zeile je Equipment mit den Spalten „Equipment“, „vorher (km)“, „Gesamt (km)“ und „Gesamt (hm)“.
  - Umsetzung: `Hauptfenster.xaml`, `HauptfensterViewModel.EquipmentEinsatz`, `EquipmentEinsatzFenster.xaml`
  - Manuell: „Extras → Equipment-Einsatz…“ wählen; Spalten prüfen.
- **Kriterium:** Wenn die Auskunft berechnet wird, dann zählen alle Einheiten, unabhängig von der Jahres- und Aktivitätsauswahl im Hauptfenster.
  - Umsetzung: `ErmittleEquipmenteinsatz` erhält alle Einheiten des Datenbestands, nicht die Auswahl
  - Prüfung: `ErmittleEquipmenteinsatzTests` (Einheiten verschiedener Jahre und Aktivitäten zählen)
  - Manuell: oben ein einzelnes Jahr wählen; die Auskunft bleibt gleich.
- **Kriterium:** Wenn „Gesamt (km)“ berechnet wird, dann ist es die Summe der Distanzen aller Einheiten mit diesem Equipment plus seine Anfangskilometer; es entspricht damit dem km-Stand des Rads.
  - Umsetzung: `ErmittleEquipmenteinsatz`
  - Prüfung: `ErmittleEquipmenteinsatzTests`
  - Manuell: den Wert mit dem km-Stand der jüngsten Wartung desselben Rads vergleichen. Liegen nach der Wartung noch Einheiten, ist „Gesamt (km)“ um deren Distanz höher.
- **Kriterium:** Wenn „Gesamt (hm)“ berechnet wird, dann ist es die Summe der Höhenmeter aller Einheiten mit diesem Equipment.
  - Umsetzung: `ErmittleEquipmenteinsatz`
  - Prüfung: `ErmittleEquipmenteinsatzTests`
- **Kriterium:** Wenn für ein Equipment Anfangskilometer eingetragen sind, dann stehen sie in der Spalte „vorher (km)“.
  - Umsetzung: `ErmittleEquipmenteinsatz`
  - Prüfung: `ErmittleEquipmenteinsatzTests`
- **Kriterium:** Wenn Werte angezeigt werden, dann sind sie auf ganze Zahlen gerundet; ist ein Wert 0, bleibt die Zelle leer.
  - Umsetzung: `Einsatzzeile` (0 → `null`), Spalten mit `StringFormat='N0'` in `EquipmentEinsatzFenster.xaml`
  - Prüfung: `ErmittleEquipmenteinsatzTests` (Wert 0 ergibt `null`)
  - Manuell: Zellen ohne Nachkommastellen; ein Rad ohne Anfangskilometer hat eine leere Zelle „vorher (km)“.
- **Kriterium:** Wenn die Tabelle angezeigt wird, dann bildet die letzte Zeile „Summe“ die Summe jeder Spalte.
  - Umsetzung: `ErmittleEquipmenteinsatz`
  - Prüfung: `ErmittleEquipmenteinsatzTests`

### Welche Equipments
- **Kriterium:** Wenn das Fenster geöffnet wird, dann stehen zuerst die Equipments im Einsatz, absteigend nach „Gesamt (km)“, danach die ausgemusterten mit dem Zusatz „(ausgemustert)“, ebenfalls absteigend nach „Gesamt (km)“, danach die Zeile „ohne Equipment“ und zuletzt „Summe“.
  - Umsetzung: `ErmittleEquipmenteinsatz` (Name über `EquipmentAnzeigename.Bilden`)
  - Prüfung: `ErmittleEquipmenteinsatzTests`
- **Kriterium:** Wenn ein Equipment weder eine Einheit noch Anfangskilometer hat, dann erscheint es nicht.
  - Umsetzung: `ErmittleEquipmenteinsatz`
  - Prüfung: `ErmittleEquipmenteinsatzTests`
- **Kriterium:** Wenn Einheiten kein Equipment haben, dann erscheinen sie in der Zeile „ohne Equipment“; gibt es keine solchen Einheiten, fehlt die Zeile.
  - Umsetzung: `ErmittleEquipmenteinsatz`
  - Prüfung: `ErmittleEquipmenteinsatzTests`

### Grenzfälle
- **Kriterium:** Wenn es keine Einheit und keine Anfangskilometer gibt, dann zeigt das Fenster statt der Tabelle den Hinweis „Keine Einheiten vorhanden.“.
  - Umsetzung: `ErmittleEquipmenteinsatz` (leere Liste), `EquipmentEinsatzViewModel.HatZeilen`, Hinweis in `EquipmentEinsatzFenster.xaml`
  - Prüfung: `ErmittleEquipmenteinsatzTests` (leerer Bestand → leere Liste)
  - Manuell: Sichtprüfung des Hinweistexts im XAML; der echte Bestand hat Einheiten.
- **Kriterium:** Wenn nach einer Spalte sortiert wird, dann gilt bei den Zahlenspalten der Zahlenwert, bei „Equipment“ das Alphabet; ein zweiter Klick kehrt die Reihenfolge um; die Zeile „Summe“ bleibt dabei unten.
  - Umsetzung: `SortiereEinsatzzeilen`, aufgerufen aus `EquipmentEinsatzFenster.xaml.cs` über `EquipmentEinsatzViewModel`
  - Prüfung: `SortiereEinsatzzeilenTests`
  - Manuell: jede Überschrift zweimal anklicken; „Summe“ bleibt unten.
- **Kriterium:** Wenn das Fenster geöffnet wird, dann zeigt es den aktuellen Stand der Einheiten und Equipments; geändert wird darin nichts.
  - Umsetzung: Berechnung beim Öffnen aus dem aktuellen Datenbestand; Tabelle schreibgeschützt
  - Manuell: Distanz einer Einheit ändern, Fenster erneut öffnen.

## Entscheidungen
- „Summe“ ist eine Zeile der Tabelle. Die Sortierung übernimmt `SortiereEinsatzzeilen` statt der eingebauten Sortierung, damit „Summe“ unten bleibt.
- Beim Öffnen trägt „Gesamt (km)“ den Sortierpfeil „absteigend“. Ein Klick darauf sortiert danach rein nach dem Zahlenwert, ohne Trennung nach Einsatz und Ausmusterung.
- Einheiten, deren Equipment im Bestand nicht (mehr) vorhanden ist, zählen zu „ohne Equipment“.
- Das Fenster wird modal geöffnet und zeigt den Stand beim Öffnen; es braucht deshalb keine Aktualisierung bei Änderungen.
