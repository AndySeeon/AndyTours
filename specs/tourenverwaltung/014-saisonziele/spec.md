# 014 – Saisonziele

## Was und warum
Der Benutzer legt je Aktivität und Jahr Saisonziele für Kilometer, Höhenmeter und Anzahl der Einheiten fest. Die Ansicht „Aktuell“ vergleicht später den Jahresfortschritt mit diesen Zielen. Ein Ziel gilt so lange weiter, bis es für ein späteres Jahr geändert wird; so behalten vergangene Jahre ihre damaligen Ziele. Gepflegt werden nur das laufende und das folgende Jahr.

## Kriterien

### Anzeigen
- Wenn der Benutzer „Extras → Saisonziele…“ wählt, dann zeigt ein Fenster für das gewählte Jahr alle Aktivitäten mit ihren Zielen für Kilometer, Höhenmeter und Anzahl; vorgewählt ist das laufende Jahr.
- Wenn der Benutzer im Fenster das Jahr wählt, dann stehen nur das laufende und das folgende Jahr zur Wahl, und das Fenster zeigt die Ziele des gewählten Jahres.
- Wenn für ein Jahr kein eigenes Ziel eingetragen ist, dann gilt das Ziel des nächstfrüheren Jahres, und das Fenster kennzeichnet es als „übernommen aus <Jahr>“ bzw. bei einem Ziel aus BikeXperience als „übernommen aus BikeXperience“.
- Wenn das Fenster zum ersten Mal geöffnet wird, dann zeigt es die aus BikeXperience übernommenen Ziele; sie gelten für alle Jahre bis zur ersten Änderung.
- Wenn eine Aktivität kein Ziel hat, dann sind ihre Felder leer.

### Ändern
- Wenn der Benutzer Ziele ändert und „OK“ wählt, dann gelten sie ab dem gewählten Jahr; frühere Jahre bleiben unverändert, ein späteres Jahr ohne eigenes Ziel übernimmt sie, und nach einem Neustart sind sie noch da.
- Wenn der Benutzer ein Feld leert oder 0 einträgt, dann hat die Aktivität ab diesem Jahr dafür kein Ziel.
- Wenn der Benutzer „Abbrechen“ wählt, dann bleibt alles unverändert.
- Wenn eine Aktivität neu angelegt wird, dann hat sie zunächst keine Ziele; wird sie gelöscht, verschwinden ihre Ziele mit ihr.

### Prüfen
- Wenn ein Wert keine ganze Zahl ist oder außerhalb der Grenzen liegt (Kilometer 0 bis 100.000, Höhenmeter 0 bis 2.000.000, Anzahl 0 bis 1.000), dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.

### Fehler
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, das Fenster bleibt offen, und die gespeicherten Ziele bleiben unverändert.
