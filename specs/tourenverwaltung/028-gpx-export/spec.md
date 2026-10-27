# 028 – Track als GPX exportieren

## Was und warum
Die Strecke einer Einheit lässt sich aus dem Tourdetail als GPX-Datei speichern, wie in BikeXperience. So kann eine gefahrene Tour in anderen Programmen angesehen, weitergegeben oder als Vorlage für eine neue Tour genutzt werden. Bei einer Mehrtagestour landen alle Etappen gemeinsam in einer Datei.

## Kriterien

### Exportieren
- Wenn das Tourdetail geöffnet ist, dann gibt es auf der Karteikarte „Karte“ neben „Ganze Strecke“ die Schaltfläche „Als GPX exportieren…“.
- Wenn „Als GPX exportieren…“ gewählt wird, dann öffnet sich ein Speichern-Dialog mit dem zuletzt dafür verwendeten Ordner und dem Dateinamen aus Datum und Name der Einheit, z. B. „2026-05-01 Bründlingalm.gpx“.
- Wenn gespeichert wird, dann enthält die Datei die Strecke der Aufzeichnung mit Position und Höhe jedes Messpunkts, ohne Uhrzeiten; der Track trägt den Namen der Einheit.
- Wenn die Datei in einem anderen Programm geöffnet wird (z. B. Garmin BaseCamp, Komoot, ein GPS-Viewer), dann zeigt es dieselbe Strecke wie die Karte im Tourdetail.
- Wenn das Speichern gelungen ist, dann erscheint neben der Schaltfläche der Hinweis „Track exportiert: 2026-05-01 Bründlingalm.gpx“.

### Mehrtagestour
- Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann speichert „Als GPX exportieren…“ alle Etappen in eine Datei, jede Etappe als eigener Track mit dem Namen ihrer Einheit, nach Datum und Startzeit geordnet.
- Wenn eine Mehrtagestour exportiert wird, dann lautet der vorgeschlagene Dateiname „2026-05-01 bis 2026-05-04 Alpencross.gpx“ mit dem ersten und letzten Datum und dem Namen der ersten Etappe.
- Wenn einzelne Etappen keine GPS-Daten haben, dann fehlen sie in der Datei; die übrigen Etappen werden exportiert.

### Grenz- und Fehlerfälle
- Wenn keine der angezeigten Aufzeichnungen GPS-Daten enthält (z. B. eine Fahrt auf der Rolle), dann ist „Als GPX exportieren…“ nicht wählbar.
- Wenn einzelne Messpunkte keine Position haben (z. B. im Tunnel), dann fehlen sie in der Datei; die übrigen Punkte bleiben erhalten.
- Wenn ein Zeichen im Namen der Einheit in Dateinamen nicht erlaubt ist (z. B. „/“), dann wird es im vorgeschlagenen Dateinamen durch „-“ ersetzt.
- Wenn die gewählte Datei schon existiert, dann fragt Windows wie üblich, ob sie ersetzt werden soll.
- Wenn der Dialog abgebrochen wird, dann wird nichts geschrieben und kein Hinweis angezeigt.
- Wenn die Datei nicht geschrieben werden kann (z. B. Ordner schreibgeschützt), dann erscheint der Hinweis „Der Track konnte nicht exportiert werden: …“ mit dem Grund.
