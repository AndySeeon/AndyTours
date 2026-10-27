# Plan: 023 – Haltbarkeit der Teile

## Vorgehen
Eine neue Fachfunktion im Core berechnet zu einer Wartung die km und Tage seit der vorherigen gleichartigen Wartung (gleiches Rad, gleiche Tätigkeit ohne Unterschied der Schreibweise, nur Typ „Wartung“). Die km sind die Differenz der km-Stände aus `BerechneKmStand`; die Tage die Kalendertage zwischen den beiden Daten. Ohne vorherige gleichartige Wartung sind die km der km-Stand selbst und die Tage die seit dem Anschaffungsdatum bzw. der ersten Einheit des Rads.

Das Wartungsfenster weist zusätzlich eine zweite gleichartige Wartung am selben Tag für dasselbe Rad ab – nach dem Muster der Doppelprüfung der Alarme (022): `WartungEingaben.AlsWartung` erhält den Datenbestand, markiert das Feld „Tätigkeit“ und liefert den Hinweis.

`Wartungszeile` erhält die beiden Werte, `ErstelleWartungszeilen` füllt sie, und die Wartungstabelle zeigt sie als Spalten neben dem km-Stand. Da die Zeilen schon nach jeder Änderung an Wartungen, Einheiten und Rädern neu aufgebaut werden (021), bleiben die Spalten ohne weitere Änderung aktuell.

## Dateien
- `src/AndyTours.Core/Modell/Haltbarkeit.cs` (neu) — km und Tage seit der letzten gleichartigen Wartung (jeweils leer möglich)
- `src/AndyTours.Core/Berechnungen/BerechneHaltbarkeit.cs` (neu) — Haltbarkeit einer Wartung aus dem Datenbestand
- `src/AndyTours.Core/Modell/Wartungszeile.cs` (ändern) — zusätzlich „km seit letzter“ und „Tage seit letzter“
- `src/AndyTours.Core/Berechnungen/ErstelleWartungszeilen.cs` (ändern) — die beiden Werte je Zeile füllen
- `src/AndyTours.Core/Berechnungen/WartungEingaben.cs` (ändern) — Doppelprüfung: gleiches Rad, gleicher Tag, gleiche Tätigkeit ohne Unterschied der Schreibweise, ohne die bearbeitete Wartung selbst
- `src/AndyTours.App/ViewModels/WartungBearbeitenViewModel.cs` (ändern) — Datenbestand übernehmen und an die Prüfung weitergeben
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Wartungen.cs` (ändern) — Datenbestand an das Wartungsfenster übergeben
- `src/AndyTours.App/Views/WartungenAnsicht.xaml` (ändern) — Spalten „km seit letzter“ und „Tage seit letzter“ nach „km-Stand“, rechtsbündig, sortierbar
- `tests/AndyTours.Tests/Berechnungen/BerechneHaltbarkeitTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErstelleWartungszeilenTests.cs` (ändern) — erwartete Zeilen mit den neuen Werten
- `tests/AndyTours.Tests/Berechnungen/WartungEingabenTests.cs` (ändern) — Aufrufe mit Datenbestand, Tests der Doppelprüfung

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Spalten in der Wartungstabelle
- **Kriterium:** Wenn eine Wartung angezeigt wird, dann stehen neben dem km-Stand die Spalten „km seit letzter“ und „Tage seit letzter“ mit den Kilometern und Tagen seit der vorherigen Wartung mit derselben Tätigkeit an diesem Rad.
  - Umsetzung: `BerechneHaltbarkeit`, `ErstelleWartungszeilen`, `WartungenAnsicht.xaml`
  - Prüfung: `BerechneHaltbarkeitTests`, `ErstelleWartungszeilenTests`
  - Manuell: Karteikarte „Wartung“; bei zwei Kettenwechseln desselben Rads die Werte des zweiten nachrechnen.
- **Kriterium:** Wenn die km seit der letzten gleichartigen Wartung berechnet werden, dann zählen die Distanzen der Einheiten mit diesem Rad nach dem Tag der vorherigen Wartung bis einschließlich zum Tag dieser Wartung (die Differenz der beiden km-Stände).
  - Umsetzung: `BerechneHaltbarkeit` (über `BerechneKmStand`)
  - Prüfung: `BerechneHaltbarkeitTests` (Einheit am Tag der vorherigen Wartung zählt nicht, am Tag dieser Wartung zählt sie)
- **Kriterium:** Wenn die Tage berechnet werden, dann zählen die Kalendertage zwischen den beiden Wartungen (vom 01.05. bis zum 31.05. sind es 30 Tage).
  - Umsetzung: `BerechneHaltbarkeit`
  - Prüfung: `BerechneHaltbarkeitTests` (01.05. → 31.05. = 30)
- **Kriterium:** Wenn geprüft wird, ob zwei Wartungen gleichartig sind, dann zählt nur dieselbe Tätigkeit ohne Unterschied der Groß- und Kleinschreibung am selben Rad; andere Räder und andere Tätigkeiten zählen nicht.
  - Umsetzung: `BerechneHaltbarkeit`
  - Prüfung: `BerechneHaltbarkeitTests` (Schreibweise, anderes Rad, andere Tätigkeit, Alarme zählen nicht)
- **Kriterium:** Wenn nach einer dieser Spalten sortiert wird, dann wird nach dem Zahlenwert sortiert; ein zweiter Klick kehrt die Reihenfolge um.
  - Umsetzung: `WartungenAnsicht.xaml` (Spalten an Zahlenwerte gebunden, `SortMemberPath`)
  - Manuell: nach „km seit letzter“ sortieren, zweimal klicken.

### Erste Wartung einer Art
- **Kriterium:** Wenn es für eine Wartung keine vorherige gleichartige gibt, dann steht unter „km seit letzter“ der km-Stand des Rads an diesem Tag (einschließlich seiner Anfangskilometer).
  - Umsetzung: `BerechneHaltbarkeit`
  - Prüfung: `BerechneHaltbarkeitTests`
- **Kriterium:** Wenn es für eine Wartung keine vorherige gleichartige gibt, dann stehen unter „Tage seit letzter“ die Tage seit dem Anschaffungsdatum des Rads; ist keines eingetragen, die Tage seit der ersten Einheit mit diesem Rad.
  - Umsetzung: `BerechneHaltbarkeit`
  - Prüfung: `BerechneHaltbarkeitTests` (mit und ohne Anschaffungsdatum)
- **Kriterium:** Wenn es weder ein Anschaffungsdatum noch eine Einheit mit diesem Rad bis zum Tag der Wartung gibt, dann bleibt „Tage seit letzter“ leer.
  - Umsetzung: `BerechneHaltbarkeit`
  - Prüfung: `BerechneHaltbarkeitTests`

### Grenz- und Fehlerfälle
- **Kriterium:** Wenn beim Speichern einer Wartung für dasselbe Rad am selben Tag schon eine Wartung mit derselben Tätigkeit (ohne Unterschied der Groß- und Kleinschreibung) existiert, dann wird nicht gespeichert und es erscheint der Hinweis „Für ‚Genius‘ gibt es am 01.05.2026 schon die Wartung ‚Kettenwechsel‘.“; beim Bearbeiten zählt die Wartung selbst nicht.
  - Umsetzung: `WartungEingaben.AlsWartung`, `WartungBearbeitenViewModel` (Hinweis wie „Das Datum liegt in der Zukunft.“)
  - Prüfung: `WartungEingabenTests` (Doppel abgewiesen samt Hinweis; andere Schreibweise abgewiesen; anderes Rad, anderer Tag, andere Tätigkeit, gleichnamiger Alarm und die bearbeitete Wartung selbst erlaubt)
  - Manuell: zweiten „Kettenwechsel“ am selben Tag für dasselbe Rad anlegen → Hinweis, Fenster bleibt offen.
- **Kriterium:** Wenn eine Wartung keinem Rad zugeordnet ist, dann bleiben beide Spalten leer.
  - Umsetzung: `BerechneHaltbarkeit`
  - Prüfung: `BerechneHaltbarkeitTests`, `ErstelleWartungszeilenTests`
- **Kriterium:** Wenn eine Wartung erfasst, geändert oder gelöscht wird oder sich Einheiten oder Räder ändern, dann zeigen die Spalten sofort den neuen Stand.
  - Umsetzung: vorhandener Neuaufbau der Wartungszeilen (`HauptfensterViewModel.Wartungen`, aus 021/022)
  - Manuell: zweiten Kettenwechsel löschen und wieder anlegen; Anschaffungsdatum eines Rads ändern.

## Entscheidungen
- Vorherige gleichartige Wartung ist die mit dem spätesten Datum vor dieser. Doppelte am selben Tag kann es nach der neuen Prüfung nicht mehr geben; die Berechnung bleibt trotzdem eindeutig (bei gleichem Datum gilt die kleinere Nummer als vorherige, Ergebnis 0 km und 0 Tage).
- Zusammenführen (H5b): Tätigkeiten und Räder lassen sich nicht zusammenführen (nur Aktivitäten und Kategorien), und ein Umbenennen auf einen schon vorhandenen Namen wird ohne Unterschied der Schreibweise abgewiesen (`PruefeAuswahleintrag`). Auf diesem Weg kann also keine doppelte Wartung entstehen; es ist nichts zu sperren.
- Ein Anschaffungsdatum nach dem Tag der Wartung wird nicht verwendet; dann gilt die erste Einheit mit diesem Rad bis zum Tag der Wartung.
- Die Spalten zeigen km ohne Nachkommastellen und Tage als ganze Zahl; leere Werte bleiben leer.

## Risiken
- Die Berechnung läuft je Zeile über alle Wartungen und Einheiten; bei rund 100 Wartungen und 1.000 Einheiten ist das unmerklich.
