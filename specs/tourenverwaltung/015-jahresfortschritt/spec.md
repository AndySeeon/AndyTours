# 015 – Jahresfortschritt

## Was und warum
Die Karteikarte „Aktuell“ zeigt in einem Liniendiagramm die Jahressummen aller erfassten Jahre für Kilometer, Anzahl Einheiten, Höhenmeter und Fahrzeit, dazu die Saisonziele. Über dem Diagramm stehen die Kennzahlen des laufenden Jahres: Stand gegenüber Ziel und Soll-Stand sowie die nötige Wochenleistung bis Saisonende. So sieht der Benutzer auf einen Blick, wie das laufende Jahr im Vergleich zu früheren Jahren und zum Ziel steht. Das Diagramm deckt auch den Map-Eintrag `mehrjahresdiagramm` ab.

## Kriterien

### Diagramm
- Wenn der Benutzer die Karteikarte „Aktuell“ öffnet, dann zeigt sie ein Liniendiagramm mit ausgefüllter Fläche unter der Linie: waagrecht alle Jahre mit erfassten Einheiten (das laufende Jahr ganz rechts), senkrecht die Jahressumme des gezeigten Werts, mit einem Punkt je Jahr.
- Wenn der Benutzer in das Diagramm klickt, dann wechselt es von Kilometern (Summe Entfernung) zu Anzahl Einheiten, dann zu Höhenmetern (Summe Höhenmeter), dann zu Fahrzeit und wieder zu Kilometern; die Überschrift nennt den gezeigten Wert.
- Wenn Jahre ein Saisonziel für den gezeigten Wert haben, dann zeigt das Diagramm es als gestrichelte Ziellinie über diesen Jahren, und im laufenden Jahr ist zusätzlich der Soll-Stand zum heutigen Tag markiert.

### Auswahl
- Wenn in der Werkzeugleiste eine Aktivität gewählt ist, dann enthält das Diagramm nur deren Einheiten und Ziele; bei „Alle Aktivitäten“ werden Einheiten und Ziele aller Aktivitäten zusammengezählt.
- Wenn in der Filterleiste eine Kategorie gewählt ist, dann enthält das Diagramm nur Einheiten dieser Kategorie; die übrigen Filter und die Jahresauswahl der Werkzeugleiste ändern das Diagramm nicht.

### Kennzahlen des laufenden Jahres
- Wenn für den gezeigten Wert ein Ziel im laufenden Jahr besteht, dann stehen über dem Diagramm Stand, Ziel, Anteil in Prozent, Soll-Stand zum heutigen Tag (Ziel anteilig zu den verstrichenen Tagen des Jahres) und Vorsprung bzw. Rückstand.
- Wenn das Ziel noch nicht erreicht ist, dann nennt die Karteikarte die nötige Wochenleistung bis zum 31. Dezember, z. B. „noch 2.345 km – 112,4 km pro Woche“; die verbleibenden Wochen sind die restlichen Tage einschließlich heute geteilt durch 7.
- Wenn das Ziel erreicht ist, dann steht dort „Ziel erreicht“ mit dem Überschuss.
- Wenn für Kilometer, Anzahl oder Höhenmeter kein Ziel besteht, dann stehen nur der Stand und der Hinweis „kein Saisonziel – unter Extras → Saisonziele… festlegen“; für die Fahrzeit gibt es kein Saisonziel, dort steht nur der Stand.

### Aktualität und Leerzustand
- Wenn Einheiten angelegt, geändert oder gelöscht werden oder sich die Saisonziele, die Aktivität der Werkzeugleiste oder die Kategorie der Filterleiste ändern, dann zeigt die Karteikarte sofort den neuen Stand.
- Wenn die Auswahl keine Einheiten enthält, dann erscheint statt des Diagramms der Hinweis „Keine Einheiten in dieser Auswahl.“
