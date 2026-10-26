# 032 – Courses übertragen

## Was und warum
Geplante und gefahrene Strecken lassen sich als Course direkt auf den angeschlossenen Garmin Edge übertragen, um sie unterwegs nachzufahren. Der Edge zeigt dann die Linie mit Höhenprofil und kündigt die Abbiegungen an: bei einer Planung die beim Planen ermittelten Abbiegepunkte, bei einer gefahrenen Strecke nachträglich ermittelte. Übertragen wird aus dem Planungsfenster und aus dem Tourdetail, auf den Edge 1040 ebenso wie auf die älteren Edge 1000 und Edge 800. Damit entfällt der Umweg über andere Programme.

Nicht enthalten: Courses aus der Overlay-Verwaltung übertragen und Courses als Datei speichern (eine Planung lässt sich schon als GPX speichern, eine Einheit als GPX exportieren).

## Kriterien

### Übertragen
- Wenn der Benutzer im Planungsfenster „Auf Garmin übertragen…“ wählt, dann wird die geplante Strecke als Course auf den angeschlossenen Edge übertragen.
- Wenn der Benutzer im Tourdetail „Auf Garmin übertragen…“ wählt, dann wird die gefahrene Strecke der Einheit als Course übertragen.
- Wenn der Benutzer überträgt, dann kann er vorher den Namen des Course eingeben; vorgeschlagen wird bei einer gespeicherten Planung der Dateiname ohne Endung, bei einer ungespeicherten „Planung JJJJ-MM-TT“, bei einer Einheit Datum und Name wie beim GPX-Export.
- Wenn der Benutzer die Namenseingabe abbricht, dann wird nichts übertragen.
- Wenn die Übertragung gelungen ist, dann erscheint der Hinweis „Course „X“ übertragen. Er erscheint auf dem Edge nach dem Trennen vom Computer unter „Courses“.“

### Inhalt des Course
- Wenn ein Course auf dem Edge geöffnet wird, dann zeigt er dieselbe Strecke wie die Karte in AndyTours, mit Höhenprofil.
- Wenn eine Planung mit Abbiegepunkten übertragen wird, dann kündigt der Edge beim Nachfahren die Abbiegungen mit ihrer Richtung an.
- Wenn eine gefahrene Strecke übertragen wird, dann werden ihre Abbiegungen mit dem gewählten Routingdienst ermittelt und der Edge kündigt sie beim Nachfahren an; die Linie des Course bleibt die gefahrene Strecke.
- Wenn die Abbiegungen einer gefahrenen Strecke nicht ermittelt werden können (z. B. ohne Internet), dann wird der Course ohne Abbiegehinweise übertragen und ein Hinweis nennt den Grund.
- Wenn eine gefahrene Strecke abseits von Wegen verläuft, dann enthält der Course dort keine Abbiegehinweise.

### Mehrtagestour
- Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann wird jede Etappe mit GPS-Daten als eigener Course übertragen, benannt nach Datum und Name ihrer Einheit; der Name lässt sich je Etappe ändern.

### Geräte
- Wenn ein Edge 1040, Edge 1000 oder Edge 800 per USB angeschlossen ist, dann lässt sich auf dieses Gerät übertragen.
- Wenn mehrere Garmin-Geräte angeschlossen sind, dann wählt der Benutzer, auf welches übertragen wird.
- Wenn kein Garmin-Gerät angeschlossen ist, dann erscheint der Hinweis „Kein Garmin-Gerät gefunden. Bitte den Edge per USB anschließen.“

### Grenz- und Fehlerfälle
- Wenn die Planung weniger als zwei Wegpunkte hat oder keine der angezeigten Einheiten GPS-Daten enthält, dann ist „Auf Garmin übertragen…“ nicht wählbar.
- Wenn auf dem Gerät schon ein noch nicht übernommener Course mit demselben Namen wartet, dann fragt die App, ob er ersetzt werden soll; bei „Nein“ wird dieser Course nicht übertragen.
- Wenn die Übertragung scheitert (z. B. weil das Gerät getrennt wurde), dann erscheint ein Hinweis mit dem Grund, und Planung bzw. Einheit bleiben unverändert.
