# 017 – Garmin-Import

## Was und warum
Neue Touren werden direkt vom angeschlossenen Garmin-Radcomputer übernommen: vom Edge 1040, der sich nicht wie ein USB-Laufwerk auslesen lässt, ebenso wie von den älteren Edge 1000 und Edge 800, die als Laufwerk erscheinen. AndyTours zeigt die Touren, die noch fehlen, legt für die gewählten Touren Einheiten mit allen Werten der Aufzeichnung an und legt die Aufzeichnung dazu ab. Das Rad erkennt AndyTours an der Rad-Kennung, die der Edge nach der Tour abfragt und in der Aufzeichnung speichert, ersatzweise an den Sensoren am Rad; die Zuordnung lernt AndyTours beim Import selbst. Als Ausweg ohne Gerät lassen sich Aufzeichnungsdateien auch von der Festplatte wählen. Damit entfällt das bisherige Übertragen über Umwege, und BikeXperience wird für den Import nicht mehr gebraucht.

## Kriterien

### Import vom Gerät
- Wenn der Benutzer „Vom Garmin importieren…“ wählt und ein Edge 1040, Edge 1000 oder Edge 800 per USB angeschlossen ist, dann zeigt ein Fenster alle Touren des Geräts, die noch nicht in AndyTours sind, mit Datum, Startzeit, Distanz, Fahrzeit, Profil und Equipment; alle sind angehakt und lassen sich abwählen.
- Wenn mehrere Garmin-Geräte gleichzeitig angeschlossen sind, dann zeigt die Liste die neuen Touren aller Geräte, jeweils mit dem Namen des Geräts.
- Wenn der Benutzer auf „Importieren“ klickt, dann wird für jede angehakte Tour eine Einheit angelegt; Datum, Startzeit, Fahrzeit, Distanz, Höhenmeter, Höhen, Geschwindigkeiten, Puls, Trittfrequenz, Temperatur und Leistung kommen aus der Aufzeichnung, der Name bleibt leer.
- Wenn eine Einheit importiert wird, dann ist ihre Fahrzeit die gestoppte Zeit ohne Pausen, wie sie der Edge als „Zeit“ anzeigt.
- Wenn eine Tour importiert wird, dann wird ihre Aufzeichnung bei der Einheit abgelegt, so wie bei den aus BikeXperience übernommenen Touren.
- Wenn der Import fertig ist, dann sind die neuen Einheiten in der Tabelle markiert, und die Statusleiste meldet die Anzahl, z. B. „3 Touren importiert.“

### Import aus Dateien
- Wenn der Benutzer „FIT-Dateien importieren…“ wählt und eine oder mehrere Aufzeichnungsdateien aussucht, dann erscheint dieselbe Liste wie beim Gerät, mit den Touren aus diesen Dateien, die noch nicht in AndyTours sind.

### Gerätezuordnung
- Wenn die Aufzeichnung eine Rad-Kennung enthält, die bei einem Equipment gemerkt ist, dann steht in der Liste dieses Equipment.
- Wenn die Rad-Kennung fehlt oder bei keinem Equipment gemerkt ist, aber ein Geschwindigkeits- oder Trittfrequenzsensor der Aufzeichnung bei einem Equipment gemerkt ist, dann steht in der Liste dieses Equipment.
- Wenn weder Rad-Kennung noch Sensor passen, dann steht in der Liste das Standard-Equipment der Hauptaktivität.
- Wenn der Benutzer in der Liste für eine Tour ein anderes Equipment wählt, dann erhält die Einheit dieses Equipment.
- Wenn eine Einheit importiert wird, dann erhält sie die Aktivität ihres Equipments (ohne Aktivität: die Hauptaktivität), die Standardkategorien und die Standardkategorie des Equipments.
- Wenn eine Tour importiert wird, dann merkt sich AndyTours ihre Rad-Kennung und ihre Geschwindigkeits- und Trittfrequenzsensoren bei dem Equipment der Einheit; waren sie bei einem anderen Equipment gemerkt, werden sie dort entfernt.
- Wenn der Benutzer ein Equipment bearbeitet, dann sieht er dort die gemerkten Rad-Kennungen und Sensoren und kann einzelne entfernen.

### Grenz- und Fehlerfälle
- Wenn eine Tour mit gleichem Datum und gleicher Startzeit schon in AndyTours ist, dann erscheint sie nicht in der Liste.
- Wenn kein Garmin-Gerät angeschlossen ist, dann erscheint der Hinweis „Kein Garmin-Gerät gefunden. Bitte den Edge per USB anschließen.“
- Wenn alle Touren schon in AndyTours sind, dann erscheint der Hinweis „Keine neuen Touren gefunden.“
- Wenn eine Aufzeichnung nicht gelesen werden kann, dann wird sie übersprungen und in der Meldung am Ende genannt; die übrigen Touren werden trotzdem importiert.
