# 022 – Wartungsalarme

## Was und warum
Für ein Rad lässt sich ein Alarm einrichten, der nach einer bestimmten Kilometerzahl an eine Wartung erinnert, z. B. „Kettenwechsel alle 3.000 km“. Gezählt wird ab der letzten Wartung mit derselben Tätigkeit an diesem Rad; gibt es noch keine, ab dem Tag, an dem der Alarm angelegt wurde. Wird die Wartung erfasst, beginnt der Alarm von selbst neu. Fällige und bald fällige Alarme fallen auf der Karteikarte „Wartung“ und beim Programmstart auf, damit keine Wartung vergessen wird – wie die Alarme in BikeXperience, aber ohne sie nach jeder Wartung neu setzen zu müssen.

## Kriterien

### Anzeigen
- Wenn der Benutzer die Karteikarte „Wartung“ öffnet, dann sieht er über der Wartungstabelle eine eigene Liste der Alarme mit Rad, Tätigkeit, Intervall (km), gefahrenen km seit dem Bezugspunkt und verbleibenden km.
- Wenn ein Alarm angezeigt wird, dann sind die gefahrenen km die Distanzen aller Einheiten mit diesem Rad nach dem Bezugspunkt; Bezugspunkt ist der Tag der letzten Wartung mit derselben Tätigkeit an diesem Rad, ohne eine solche der Tag, an dem der Alarm angelegt wurde.
- Wenn ein Rad seit dem Bezugspunkt mindestens die Intervall-km gefahren ist, dann ist der Alarm fällig: Die Zeile ist rot, und die verbleibenden km erscheinen als Überschreitung (z. B. „−120“).
- Wenn bei einem Alarm nur noch höchstens 10 % des Intervalls übrig sind, er aber noch nicht fällig ist, dann ist er bald fällig, und die Zeile ist orange.
- Wenn mindestens ein Alarm fällig ist, dann trägt die Karteikarte die Anzahl der fälligen Alarme im Titel, z. B. „Wartung (2)“; ohne fällige Alarme heißt sie „Wartung“.
- Wenn beim Programmstart Alarme fällig sind, dann erscheint ein Hinweis, der jeden fälligen Alarm mit Rad, Tätigkeit und Überschreitung nennt.
- Wenn oben ein Rad gewählt ist, dann zeigt die Alarmliste nur dessen Alarme; mit „Alle Räder“ wieder alle.
- Wenn es keine Alarme gibt, dann steht statt der Alarmliste „Keine Alarme eingerichtet.“

### Anlegen, ändern, löschen
- Wenn der Benutzer „Neuer Alarm…“ wählt, dann öffnet sich ein Fenster mit Rad, Tätigkeit (aus der Liste „Tätigkeit“, ohne freie Eingabe, geordnet wie im Wartungsfenster nach bisheriger Häufigkeit) und Intervall in km; ist oben ein Rad gewählt, ist es vorgewählt.
- Wenn der Benutzer mit „OK“ bestätigt, dann erscheint der Alarm sofort in der Liste und bleibt nach einem Neustart erhalten.
- Wenn der Benutzer einen Alarm doppelt anklickt oder „Bearbeiten…“ wählt, dann öffnet sich dasselbe Fenster mit seinen Werten; Änderungen erscheinen nach „OK“, „Abbrechen“ verwirft sie.
- Wenn der Benutzer einen oder mehrere Alarme markiert und „Löschen“ wählt oder Entf drückt, dann erscheint eine Rückfrage mit der Anzahl, und erst nach „Ja“ werden sie gelöscht.
- Wenn der Benutzer einen Alarm ausschaltet, dann wird er grau dargestellt, nicht fällig oder bald fällig und nicht beim Start gemeldet, bis er ihn wieder einschaltet.

### Zurücksetzen
- Wenn der Benutzer für ein Rad eine Wartung mit der Tätigkeit eines Alarms erfasst, ändert oder löscht, dann zählt der Alarm sofort ab dem Tag der nun letzten solchen Wartung.
- Wenn eine Tätigkeit in der Liste „Tätigkeit“ umbenannt wird, dann tragen auch die Alarme mit dieser Tätigkeit den neuen Namen, und eine verwendete Tätigkeit lässt sich nicht löschen, solange ein Alarm sie nutzt.

### Grenz- und Fehlerfälle
- Wenn das Intervall leer, keine ganze Zahl oder nicht größer als 0 ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
- Wenn kein Rad oder keine Tätigkeit gewählt ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
- Wenn für dasselbe Rad und dieselbe Tätigkeit schon ein Alarm besteht, dann ist „OK“ nicht möglich, und ein Hinweis nennt den vorhandenen Alarm.
- Wenn ein Rad ausgemustert ist, dann wird es für neue Alarme nicht angeboten, und seine Alarme werden weder fällig noch bald fällig noch beim Start gemeldet.
- Wenn keine oder mehrere Alarme markiert sind, dann ist „Bearbeiten…“ nicht wählbar; ohne Markierung ist auch „Löschen“ nicht wählbar.
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und die Liste bleibt beim vorherigen Stand.
