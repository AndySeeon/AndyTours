# 023 – Haltbarkeit der Teile

## Was und warum
Die Wartungstabelle zeigt zu jeder Wartung, wie viele Kilometer und Tage seit der vorherigen Wartung mit derselben Tätigkeit an diesem Rad vergangen sind – also wie lange z. B. die gewechselte Kette gehalten hat. Bei der ersten Wartung einer Art zählt der ganze bisherige Weg des Rads. So lässt sich wie mit der Auskunft in BikeXperience ablesen, wie lange ein Teil hält, und das nächste Wechselintervall abschätzen.

## Kriterien

### Spalten in der Wartungstabelle
- Wenn eine Wartung angezeigt wird, dann stehen neben dem km-Stand die Spalten „km seit letzter“ und „Tage seit letzter“ mit den Kilometern und Tagen seit der vorherigen Wartung mit derselben Tätigkeit an diesem Rad.
- Wenn die km seit der letzten gleichartigen Wartung berechnet werden, dann zählen die Distanzen der Einheiten mit diesem Rad nach dem Tag der vorherigen Wartung bis einschließlich zum Tag dieser Wartung (die Differenz der beiden km-Stände).
- Wenn die Tage berechnet werden, dann zählen die Kalendertage zwischen den beiden Wartungen (vom 01.05. bis zum 31.05. sind es 30 Tage).
- Wenn geprüft wird, ob zwei Wartungen gleichartig sind, dann zählt nur dieselbe Tätigkeit ohne Unterschied der Groß- und Kleinschreibung am selben Rad; andere Räder und andere Tätigkeiten zählen nicht.
- Wenn nach einer dieser Spalten sortiert wird, dann wird nach dem Zahlenwert sortiert; ein zweiter Klick kehrt die Reihenfolge um.

### Erste Wartung einer Art
- Wenn es für eine Wartung keine vorherige gleichartige gibt, dann steht unter „km seit letzter“ der km-Stand des Rads an diesem Tag (einschließlich seiner Anfangskilometer).
- Wenn es für eine Wartung keine vorherige gleichartige gibt, dann stehen unter „Tage seit letzter“ die Tage seit dem Anschaffungsdatum des Rads; ist keines eingetragen, die Tage seit der ersten Einheit mit diesem Rad.
- Wenn es weder ein Anschaffungsdatum noch eine Einheit mit diesem Rad bis zum Tag der Wartung gibt, dann bleibt „Tage seit letzter“ leer.

### Grenz- und Fehlerfälle
- Wenn beim Speichern einer Wartung für dasselbe Rad am selben Tag schon eine Wartung mit derselben Tätigkeit (ohne Unterschied der Groß- und Kleinschreibung) existiert, dann wird nicht gespeichert und es erscheint der Hinweis „Für ‚Genius‘ gibt es am 01.05.2026 schon die Wartung ‚Kettenwechsel‘.“; beim Bearbeiten zählt die Wartung selbst nicht.
- Wenn eine Wartung keinem Rad zugeordnet ist, dann bleiben beide Spalten leer.
- Wenn eine Wartung erfasst, geändert oder gelöscht wird oder sich Einheiten oder Räder ändern, dann zeigen die Spalten sofort den neuen Stand.
