# 029 – Track bearbeiten

## Was und warum
Im Tourdetail einer einzelnen Einheit lässt sich die Strecke auf der Karte kürzen, umkehren und glätten, wie im GPS-Viewer von BikeXperience. Das Ergebnis wird über „Als GPX exportieren…“ gespeichert, z. B. als Vorlage zum Weitergeben oder für eine neue Tour. Die Einheit und ihre Aufzeichnung bleiben dabei unverändert, damit die Originaldaten sicher sind.

## Kriterien

### Bearbeiten im Tourdetail
- Wenn das Tourdetail einer einzelnen Einheit mit GPS-Daten geöffnet ist, dann gibt es auf der Karteikarte „Karte“ unter der Karte die Leiste „Track bearbeiten“ mit den Schiebereglern „Start“ und „Ziel“, „Umkehren“, „Glätten“ und „Zurücksetzen“.
- Wenn ein Schieberegler „Start“ oder „Ziel“ verschoben wird, dann steht daneben seine Stelle in km ab dem Start der Aufzeichnung, die Karte zeigt nur noch das Stück dazwischen, und die Zeichen für Start und Ziel wandern mit.
- Wenn „Umkehren“ eingeschaltet wird, dann tauschen Start und Ziel ihre Rollen; die Strecke läuft rückwärts.
- Wenn bei „Glätten“ „schwach“ oder „stark“ gewählt wird, dann zeigt die Karte die entsprechend geglättete Linie – das GPS-Zittern verschwindet, „stark“ glättet mehr als „schwach“ – und die Strecke hat weniger Punkte; „aus“ zeigt die ungeglättete Linie.
- Wenn die Strecke angezeigt wird, dann steht in der Leiste die Länge der bearbeiteten Strecke, z. B. „Länge: 42,3 km“.
- Wenn „Zurücksetzen“ gewählt wird, dann stehen Start und Ziel wieder an Anfang und Ende, „Umkehren“ ist aus, „Glätten“ steht auf „aus“, und die Karte zeigt die ganze, unbearbeitete Strecke.

### Speichern
- Wenn nach dem Bearbeiten „Als GPX exportieren…“ gewählt wird, dann enthält die Datei die bearbeitete Strecke – gekürzt, ggf. umgekehrt und geglättet –, weiter mit der Höhe je Punkt.
- Wenn die Strecke bearbeitet wurde, dann schlägt der Speichern-Dialog den Dateinamen mit dem Zusatz „(bearbeitet)“ vor, z. B. „2026-05-01 Bründlingalm (bearbeitet).gpx“; unbearbeitet bleibt es beim bisherigen Namen.
- Wenn die Strecke bearbeitet wurde, dann bleiben Einheit, Aufzeichnung, Distanz, Höhenmeter und Höhenprofil unverändert.
- Wenn das Tourdetail geschlossen und wieder geöffnet wird, dann ist die Bearbeitung verworfen.

### Grenzfälle
- Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann fehlt die Leiste „Track bearbeiten“.
- Wenn die Aufzeichnung keine GPS-Daten enthält, dann fehlt die Leiste.
- Wenn „Start“ und „Ziel“ zusammengeschoben werden, dann bleibt immer ein Stück von mindestens zwei Punkten übrig; „Start“ kann nicht hinter „Ziel“ gezogen werden und umgekehrt.
- Wenn „Ganze Strecke“ gewählt wird, dann zeigt die Karte die ganze bearbeitete Strecke.
