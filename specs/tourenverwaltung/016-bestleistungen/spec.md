# 016 – Bestleistungen

## Was und warum
Die Karteikarte „Aktuell“ zeigt über dem Diagramm die Bestleistungen, wie die Übersicht „Aktuell“ in BikeXperience: die Jahresrekorde mit ihrem Rekordjahr und die Einzelrekorde mit der Einheit, in der sie aufgestellt wurden. So sieht der Benutzer auf einen Blick, was seine besten Jahre und Touren waren und ob das laufende Jahr einen Rekord hält.

## Kriterien

### Anzeige
- Wenn der Benutzer die Karteikarte „Aktuell“ öffnet, dann steht über dem Diagramm eine Liste der Bestleistungen.
- Wenn es Jahresrekorde gibt, dann nennt die Liste für Kilometer, Höhenmeter, Anzahl Einheiten und Fahrzeit jeweils den höchsten Jahreswert mit seinem Jahr, z. B. „Kilometer: 8.123,4 km (2019)“.
- Wenn es Einzelrekorde gibt, dann nennt die Liste die längste Tour, die meisten Höhenmeter, die längste Fahrzeit, die höchste Ø-Geschwindigkeit, die höchste Höchstgeschwindigkeit und den höchsten Punkt, jeweils mit Wert, Datum und Name der Einheit.
- Wenn das laufende Jahr einen Rekord hält, dann ist dieser hervorgehoben.
- Wenn der Benutzer auf einen Einzelrekord doppelklickt, dann öffnet sich die zugehörige Einheit zum Bearbeiten.

### Auswahl
- Wenn in der Werkzeugleiste eine Aktivität oder in der Filterleiste eine Kategorie gewählt ist, dann gelten die Bestleistungen nur für diese Einheiten, wie beim Diagramm; die Jahresauswahl und die übrigen Filter ändern die Bestleistungen nicht.

### Grenzfälle
- Wenn mehrere Jahre oder Einheiten denselben Rekordwert haben, dann hält der frühere den Rekord.
- Wenn bei einer Einheit ein Wert fehlt (z. B. keine Höchstgeschwindigkeit erfasst), dann zählt er nicht als Rekord; hat keine Einheit diesen Wert, entfällt die Zeile.
- Wenn die Auswahl keine Einheiten enthält, dann entfällt die Liste.
- Wenn Einheiten angelegt, geändert oder gelöscht werden oder sich die Aktivität der Werkzeugleiste oder die Kategorie der Filterleiste ändert, dann zeigt die Liste sofort den neuen Stand.
