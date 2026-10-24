# 018 – Höhenprofil

## Was und warum
Zu einer Trainingseinheit lässt sich der Verlauf der Tour ansehen: das Höhenprofil aus der Aufzeichnung, auf Wunsch zusammen mit Puls, Geschwindigkeit, Trittfrequenz und Temperatur. Es erscheint in einem eigenen Fenster „Tourdetail“, in das später auch die Strecke auf der Karte kommt. So lässt sich eine Tour wie in BikeXperience nachvollziehen, ohne ein anderes Programm zu öffnen.

## Kriterien

### Anzeigen
- Wenn genau eine Einheit mit Aufzeichnung markiert ist und der Benutzer „Einheit → Tourdetail…“ oder im Kontextmenü der Tabelle „Tourdetail…“ wählt, dann öffnet sich das Fenster „Tourdetail“ mit der Karteikarte „Höhenprofil“.
- Wenn das Fenster offen ist, dann stehen oben Datum, Name, Distanz, Höhenmeter und höchster Punkt der Einheit.
- Wenn das Höhenprofil angezeigt wird, dann zeigt die waagrechte Achse die Distanz in km und die senkrechte Achse die Höhe in m.
- Wenn der Benutzer die Achse auf „Fahrzeit“ umschaltet, dann zeigt die waagrechte Achse die Fahrzeit; Pausen werden dabei herausgelassen.
- Wenn der Benutzer bei Puls, Geschwindigkeit, Trittfrequenz oder Temperatur ein Häkchen setzt, dann erscheint dieser Wert als zusätzliche Linie mit eigener Skala; ohne Häkchen verschwindet sie wieder.
- Wenn die Aufzeichnung einen dieser Werte nicht enthält, dann ist sein Häkchen nicht wählbar.
- Wenn der Benutzer mit der Maus über das Profil fährt, dann erscheinen an dieser Stelle Distanz, Fahrzeit, Höhe und die Werte der eingeblendeten Linien.
- Wenn der Benutzer mit der Maus einen Bereich aufzieht, dann wird dieser Bereich vergrößert; ein Doppelklick zeigt wieder die ganze Tour.
- Wenn der Benutzer doppelt auf eine Zeile der Tabelle klickt, dann öffnet sich wie bisher das Bearbeitungsfenster.

### Grenz- und Fehlerfälle
- Wenn keine oder mehrere Einheiten markiert sind, dann ist „Tourdetail…“ nicht wählbar.
- Wenn die markierte Einheit keine Aufzeichnung hat, dann ist „Tourdetail…“ nicht wählbar.
- Wenn die Datei der Aufzeichnung fehlt, dann erscheint der Hinweis „Die Aufzeichnung ‚2019/2019-11-11-06-25-39.fit‘ wurde im Datenordner nicht gefunden.“ mit dem Namen der fehlenden Datei.
- Wenn die Aufzeichnung keine Höhendaten enthält (z. B. eine Fahrt auf der Rolle), dann erscheint in der Karteikarte „Die Aufzeichnung enthält keine Höhendaten.“
- Wenn die Aufzeichnung nicht gelesen werden kann, dann erscheint der Hinweis „Die Aufzeichnung konnte nicht gelesen werden.“ mit dem Grund.
