# 013 – Persönliche Daten

## Was und warum
Der Benutzer pflegt seine persönlichen Daten in einem eigenen Fenster: Vorname, Nachname, Geburtstag, Größe, Gewicht, Ruhe- und Maximalpuls sowie den Heimatort. Die Werte wurden aus BikeXperience übernommen und lassen sich bisher nicht ansehen oder ändern. Spätere Funktionen greifen darauf zurück, etwa die Streckenkarte auf den Heimatort.

## Kriterien

### Anzeigen
- Wenn der Benutzer „Extras → Persönliche Daten…“ wählt, dann zeigt ein Fenster die gespeicherten Werte: Vorname, Nachname, Geburtstag, Größe in cm, Gewicht in kg, Ruhepuls, Maximalpuls und Heimatort (nur der Name des Orts).
- Wenn das Fenster zum ersten Mal geöffnet wird, dann zeigt es die aus BikeXperience übernommenen Werte.
- Wenn noch keine persönlichen Daten vorhanden sind (neuer, leerer Datenordner), dann sind die Felder leer.

### Ändern
- Wenn der Benutzer Werte ändert und „OK“ wählt, dann sind sie gespeichert und erscheinen auch nach einem Neustart.
- Wenn der Benutzer „Abbrechen“ wählt, dann bleibt alles unverändert.
- Wenn ein Feld leer gelassen wird, dann gilt der Wert als unbekannt und wird so gespeichert.

### Prüfen
- Wenn der Geburtstag kein gültiges Datum im Format TT.MM.JJJJ ist oder in der Zukunft liegt, dann erscheint ein Hinweis, und „OK“ ist nicht wählbar.
- Wenn die Größe keine ganze Zahl ist oder außerhalb von 100 bis 250 cm liegt, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
- Wenn das Gewicht keine Zahl mit höchstens einer Nachkommastelle ist (z. B. „78,5“) oder außerhalb von 30 bis 250 kg liegt, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
- Wenn der Ruhepuls außerhalb von 30 bis 120 oder der Maximalpuls außerhalb von 100 bis 230 liegt oder keine ganze Zahl ist, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
- Wenn der Ruhepuls nicht kleiner als der Maximalpuls ist, dann erscheint der Hinweis „Der Ruhepuls muss kleiner als der Maximalpuls sein.“, und „OK“ ist nicht wählbar.

### Fehler
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, das Fenster bleibt offen, und die gespeicherten Werte bleiben unverändert.
