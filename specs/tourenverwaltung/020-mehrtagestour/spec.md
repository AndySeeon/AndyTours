# 020 – Mehrtagestour

## Was und warum
Mehrere Einheiten, z. B. die Etappen einer Radreise, lassen sich gemeinsam im Tourdetail ansehen: oben die Summen, ein durchgehendes Höhenprofil über alle Etappen und eine Karte mit der ganzen Reise. Die Mehrtagestour entsteht jedes Mal aus den markierten Einheiten und wird nicht gespeichert. Die Summen stammen aus den Werten der Einheiten; sie stehen deshalb auch für Etappen ohne Aufzeichnung zur Verfügung, etwa wenn unterwegs der Radcomputer ausfällt. So lässt sich eine Reise wie mit der Mehrtagestour in BikeXperience als Ganzes nachvollziehen.

## Kriterien

### Anzeigen
- Wenn mehrere Einheiten markiert sind und der Benutzer „Einheit → Tourdetail…“ oder im Kontextmenü der Tabelle „Tourdetail…“ wählt, dann öffnet sich das Tourdetail für alle markierten Einheiten gemeinsam.
- Wenn das Tourdetail mehrerer Einheiten geöffnet wird, dann sind die Etappen nach Datum und Startzeit geordnet, unabhängig von der Reihenfolge, in der sie markiert wurden.
- Wenn das Tourdetail mehrerer Einheiten offen ist, dann stehen oben der Zeitraum vom ersten bis zum letzten Datum, die Anzahl der Etappen, die Summen von Distanz, Höhenmetern und Fahrzeit, die Durchschnittsgeschwindigkeit über alle Etappen und der höchste Punkt.
- Wenn das Höhenprofil angezeigt wird, dann folgen die Etappen hintereinander, die Distanz zählt über die Etappen weiter, und jeder Wechsel zur nächsten Etappe ist mit einer senkrechten Linie markiert.
- Wenn der Benutzer die Achse auf „Fahrzeit“ umschaltet, dann zählt die Fahrzeit über die Etappen weiter; Nächte und Pausen zwischen den Etappen werden herausgelassen.
- Wenn der Benutzer mit der Maus über das Profil fährt, dann erscheinen zusätzlich zu den bisherigen Werten Datum und Name der Etappe an dieser Stelle.
- Wenn der Benutzer die Karteikarte „Karte“ wählt, dann zeigt sie alle Etappen, so vergrößert, dass die ganze Reise zu sehen ist.
- Wenn die Karte mehrere Etappen zeigt, dann hat jede Etappe eine eigene Farbe, wobei benachbarte Etappen sich in der Farbe unterscheiden.
- Wenn die Karte mehrere Etappen zeigt, dann sind der Start der ersten Etappe mit dem Startzeichen, das Ziel der letzten Etappe mit dem Zielzeichen und jeder Etappenwechsel mit einem kleinen Punkt markiert.
- Wenn der Benutzer im Tourdetail mehrerer Einheiten Häkchen setzt, vergrößert, die Kartenart wechselt, „Ganze Strecke“ wählt oder ohne Internet arbeitet, dann verhält es sich wie das Tourdetail einer einzelnen Einheit.

### Grenz- und Fehlerfälle
- Wenn genau eine Einheit markiert ist, dann öffnet „Tourdetail…“ wie bisher das Tourdetail dieser Einheit.
- Wenn mehrere Einheiten markiert sind, dann ist „Tourdetail…“ wählbar, auch wenn keine davon eine Aufzeichnung hat.
- Wenn keine der markierten Einheiten eine Aufzeichnung hat, dann zeigt das Tourdetail oben die Summen, und in den Karteikarten „Höhenprofil“ und „Karte“ erscheint der Hinweis „Keine der Einheiten hat eine Aufzeichnung.“
- Wenn unter den markierten Einheiten welche ohne Aufzeichnung sind, dann zählen sie zu den Summen oben, fehlen aber in Profil und Karte, und ein Hinweis nennt sie mit Datum und Name.
- Wenn die Aufzeichnung einer Etappe im Datenordner fehlt oder nicht gelesen werden kann, dann erscheint derselbe Hinweis wie beim Tourdetail einer einzelnen Einheit mit dem Namen der Datei.
- Wenn nur einige Etappen Höhendaten bzw. GPS-Daten enthalten, dann zeigen Profil bzw. Karte die übrigen Etappen, und ein Hinweis nennt die Etappen ohne diese Daten.
- Wenn keine der Etappen Höhendaten bzw. GPS-Daten enthält, dann erscheint in der Karteikarte der Hinweis „Die Aufzeichnung enthält keine Höhendaten.“ bzw. „Die Aufzeichnung enthält keine GPS-Daten.“
