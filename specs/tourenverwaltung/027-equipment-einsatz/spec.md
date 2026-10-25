# 027 – Einsatz je Equipment

## Was und warum
Eine Auskunft zeigt für jedes Equipment die insgesamt gefahrenen Kilometer und Höhenmeter, wie die Auskunft „Einsatz je Equipment“ in BikeXperience. So lässt sich ablesen, wie viel jedes Rad gefahren wurde. Die Kilometer stimmen mit dem km-Stand der Wartung überein.

## Kriterien

### Aufruf und Aufbau
- Wenn im Menü „Extras“ der Eintrag „Equipment-Einsatz…“ gewählt wird, dann öffnet sich ein Fenster mit einer Tabelle: eine Zeile je Equipment mit den Spalten „Equipment“, „vorher (km)“, „Gesamt (km)“ und „Gesamt (hm)“.
- Wenn die Auskunft berechnet wird, dann zählen alle Einheiten, unabhängig von der Jahres- und Aktivitätsauswahl im Hauptfenster.
- Wenn „Gesamt (km)“ berechnet wird, dann ist es die Summe der Distanzen aller Einheiten mit diesem Equipment plus seine Anfangskilometer; es entspricht damit dem km-Stand des Rads.
- Wenn „Gesamt (hm)“ berechnet wird, dann ist es die Summe der Höhenmeter aller Einheiten mit diesem Equipment.
- Wenn für ein Equipment Anfangskilometer eingetragen sind, dann stehen sie in der Spalte „vorher (km)“.
- Wenn Werte angezeigt werden, dann sind sie auf ganze Zahlen gerundet; ist ein Wert 0, bleibt die Zelle leer.
- Wenn die Tabelle angezeigt wird, dann bildet die letzte Zeile „Summe“ die Summe jeder Spalte.

### Welche Equipments
- Wenn das Fenster geöffnet wird, dann stehen zuerst die Equipments im Einsatz, absteigend nach „Gesamt (km)“, danach die ausgemusterten mit dem Zusatz „(ausgemustert)“, ebenfalls absteigend nach „Gesamt (km)“, danach die Zeile „ohne Equipment“ und zuletzt „Summe“.
- Wenn ein Equipment weder eine Einheit noch Anfangskilometer hat, dann erscheint es nicht.
- Wenn Einheiten kein Equipment haben, dann erscheinen sie in der Zeile „ohne Equipment“; gibt es keine solchen Einheiten, fehlt die Zeile.

### Grenzfälle
- Wenn es keine Einheit und keine Anfangskilometer gibt, dann zeigt das Fenster statt der Tabelle den Hinweis „Keine Einheiten vorhanden.“.
- Wenn nach einer Spalte sortiert wird, dann gilt bei den Zahlenspalten der Zahlenwert, bei „Equipment“ das Alphabet; ein zweiter Klick kehrt die Reihenfolge um; die Zeile „Summe“ bleibt dabei unten.
- Wenn das Fenster geöffnet wird, dann zeigt es den aktuellen Stand der Einheiten und Equipments; geändert wird darin nichts.
