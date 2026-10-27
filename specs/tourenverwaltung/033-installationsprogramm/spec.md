# 033 – Installationsprogramm

## Was und warum
Ein Installationsprogramm richtet AndyTours auf dem Rechner ein und legt den Datenordner fest. Auf Wunsch übernimmt es die bisherigen Daten: BikeXperience-Daten über das vorhandene Umwandlungsprogramm oder, wenn nur Aufzeichnungen vorliegen, diese als Touren. Jede Änderung am Rechner hält es in einem Protokoll fest. Daran kann das Deinstallationsprogramm AndyTours später vollständig entfernen, ohne die Touren ungefragt zu löschen. So lässt sich AndyTours ohne Entwicklungsumgebung einrichten, aktualisieren und wieder entfernen.

## Kriterien

### Installieren
- Wenn der Benutzer das Installationsprogramm startet, dann führt es ihn in wenigen Schritten durch die Installation: Datenordner, Übernahme der bisherigen Daten, Verknüpfungen und Zusammenfassung vor dem Start.
- Wenn die Installation fertig ist, dann lässt sich AndyTours über das Startmenü starten. Weitere Programme müssen dafür nicht installiert werden.
- Wenn der Benutzer installiert, dann braucht er keine Administratorrechte, denn AndyTours wird nur für ihn installiert.
- Wenn das Installationsprogramm nach dem Datenordner fragt, dann schlägt es `Dokumente\AndyTours` vor.
- Wenn der Benutzer einen Datenordner gewählt hat, dann verwendet AndyTours beim ersten Start genau diesen Ordner.
- Wenn die Installation fertig ist, dann gibt es immer eine Verknüpfung im Startmenü.
- Wenn der Benutzer zusätzlich eine Desktop-Verknüpfung gewählt hat, dann liegt nach der Installation eine Verknüpfung auf dem Desktop.
- Wenn die Installation fertig ist, dann erscheint AndyTours in den Windows-Einstellungen unter „Apps“ und lässt sich dort deinstallieren.

### Bisherige Daten übernehmen
- Wenn das Installationsprogramm die Übernahme anbietet, dann ist sie vorgewählt, und der Benutzer kann sie abwählen.
- Wenn der Benutzer die Übernahme gewählt hat, dann fragt das Installationsprogramm nach dem Ordner mit den bisherigen Daten, ohne einen Ordner vorzuschlagen.
- Wenn im angegebenen Ordner BikeXperience-Daten liegen, dann wandelt das vorhandene Umwandlungsprogramm sie in den gewählten Datenordner um.
- Wenn die Umwandlung fertig ist, dann zeigt das Installationsprogramm die Zusammenfassung des Umwandlungsprogramms und wo der Bericht liegt.
- Wenn im angegebenen Ordner keine BikeXperience-Daten, aber Aufzeichnungen im FIT-Format liegen (auch in Unterordnern), dann fragt das Installationsprogramm, ob diese als Touren übernommen werden sollen, und nennt die Anzahl der gefundenen Aufzeichnungen.
- Wenn der Benutzer die Übernahme der Aufzeichnungen bestätigt, dann ist danach jede lesbare Aufzeichnung eine Tour im Datenordner, mit denselben Werten wie bei einem Import vom Edge.
- Wenn Aufzeichnungen als Touren übernommen werden, dann bekommen die Touren kein Rad und die Hauptaktivität „Radfahren“ zugeordnet.
- Wenn eine Aufzeichnung nicht lesbar ist, dann wird sie übersprungen, die übrigen werden übernommen, und ein Bericht nennt die übersprungenen Dateien.
- Wenn die Übernahme der Aufzeichnungen fertig ist, dann zeigt das Installationsprogramm, wie viele Touren übernommen und wie viele Dateien übersprungen wurden und wo der Bericht liegt.
- Wenn der Benutzer die Übernahme der Aufzeichnungen verneint, dann wird nichts übernommen, und die Installation geht ohne bisherige Daten weiter.
- Wenn im angegebenen Ordner weder BikeXperience-Daten noch Aufzeichnungen liegen, dann erscheint ein Hinweis, und der Benutzer kann einen anderen Ordner wählen oder ohne Übernahme weitermachen.
- Wenn im gewählten Datenordner schon AndyTours-Daten liegen, dann bleiben sie unverändert, die Übernahme entfällt, und ein Hinweis sagt das.
- Wenn die Übernahme mit einem Fehler abbricht, dann ist AndyTours trotzdem installiert, der Datenordner bleibt wie vorher, und ein Hinweis nennt den Grund.
- Wenn die Übernahme läuft, dann bleibt der Ordner mit den bisherigen Daten unverändert.

### Änderungen festhalten
- Wenn die Installation fertig ist, dann gibt es ein lesbares Protokoll aller Änderungen am Rechner: angelegte Ordner und Dateien, Verknüpfungen, Einträge in Windows und der eingestellte Datenordner.
- Wenn das Protokoll geöffnet wird, dann steht zu jeder Änderung, was angelegt oder geändert wurde und wann.

### Aktualisieren
- Wenn AndyTours schon installiert ist und der Benutzer eine neuere Fassung installiert, dann bleiben Datenordner, Daten und Einstellungen erhalten, und die Übernahme der bisherigen Daten wird nicht mehr angeboten.
- Wenn eine neuere Fassung installiert wird, dann ergänzt das Installationsprogramm das vorhandene Protokoll um die neuen Änderungen.
- Wenn AndyTours beim Installieren läuft, dann bittet das Installationsprogramm, AndyTours zu beenden, und fährt erst danach fort.

### Deinstallieren
- Wenn der Benutzer AndyTours deinstalliert, dann entfernt das Deinstallationsprogramm alles, was im Protokoll steht: Programmdateien, Verknüpfungen, Einträge in Windows und die Einstellungen der App außerhalb des Datenordners.
- Wenn der Benutzer deinstalliert, dann fragt das Deinstallationsprogramm, ob auch der Datenordner mit allen Touren gelöscht werden soll; vorgewählt ist „Behalten“.
- Wenn der Benutzer den Datenordner behält, dann sind die Daten danach unverändert vorhanden, und eine spätere Installation verwendet sie wieder.
- Wenn die Deinstallation fertig ist, dann bleibt außerhalb des Datenordners kein Ordner und keine Datei von AndyTours zurück.
- Wenn AndyTours beim Deinstallieren läuft, dann bittet das Deinstallationsprogramm, AndyTours zu beenden, und fährt erst danach fort.
