# 021 – Wartung erfassen

## Was und warum
Die Karteikarte „Wartung“ zeigt alle Wartungen der Räder. Der Benutzer erfasst, ändert und löscht dort Wartungen mit Datum, Rad, Tätigkeit und Kommentar. Zu jeder Wartung steht der Kilometerstand des Rads an diesem Tag. Die Tätigkeiten pflegt er in den Einstellungen als eigene Auswahlliste, wie die Trainingsbereiche. So bleibt wie in BikeXperience nachvollziehbar, wann an welchem Rad was gemacht wurde. Alarme, die Haltbarkeit der Teile und der CSV-Austausch sind eigene Funktionen.

## Kriterien

### Anzeigen
- Wenn der Benutzer die Karteikarte „Wartung“ wählt, dann zeigt sie eine Tabelle aller Wartungen mit Datum, Rad, km-Stand, Tätigkeit und Kommentar, die neueste oben.
- Wenn eine Wartung angezeigt wird, dann steht unter „km-Stand“ die Summe der Distanzen aller Einheiten mit diesem Rad bis einschließlich zum Tag der Wartung, zuzüglich der Anfangskilometer des Rads.
- Wenn der Benutzer oben ein Rad wählt, dann zeigt die Tabelle nur dessen Wartungen; mit „Alle Räder“ erscheinen wieder alle.
- Wenn der Benutzer auf einen Spaltenkopf klickt, dann wird nach dieser Spalte sortiert; ein zweiter Klick kehrt die Reihenfolge um.
- Wenn es noch keine Wartung gibt, dann steht statt der Tabelle „Noch keine Wartungen erfasst.“

### Erfassen, ändern, löschen
- Wenn der Benutzer über der Tabelle oder im Kontextmenü der Tabelle „Neue Wartung…“ wählt, dann öffnet sich ein Fenster mit Datum (vorbelegt mit heute), Rad, Tätigkeit und Kommentar.
- Wenn oben ein Rad gewählt ist und der Benutzer eine neue Wartung anlegt, dann ist dieses Rad vorgewählt.
- Wenn der Benutzer im Wartungsfenster die Tätigkeit wählt, dann bietet die Auswahl die Einträge der Liste „Tätigkeit“ nach ihrer bisherigen Häufigkeit in den Wartungen aller Räder an, die häufigste zuerst, bei Gleichstand in der Reihenfolge der Liste; andere Tätigkeiten lassen sich dort nicht eingeben.
- Wenn der Benutzer mit „OK“ bestätigt, dann erscheint die Wartung sofort in der Tabelle und bleibt nach einem Neustart erhalten.
- Wenn der Benutzer doppelt auf eine Wartung klickt oder „Bearbeiten…“ wählt, dann öffnet sich dasselbe Fenster mit ihren Werten; Änderungen erscheinen nach „OK“ in der Tabelle, „Abbrechen“ verwirft sie.
- Wenn der Benutzer eine oder mehrere Wartungen markiert und „Löschen“ wählt oder Entf drückt, dann erscheint eine Rückfrage mit der Anzahl, und erst nach „Ja“ werden sie gelöscht.

### Liste der Tätigkeiten
- Wenn der Benutzer in den Einstellungen unter „Auswahllisten“ die Liste „Tätigkeit“ wählt, dann sieht er deren Einträge in der festgelegten Reihenfolge.
- Wenn die App zum ersten Mal mit dieser Funktion gestartet wird, dann enthält die Liste „Tätigkeit“ alle Tätigkeiten der vorhandenen Wartungen, alphabetisch geordnet.
- Wenn der Benutzer in der Liste „Tätigkeit“ einen Eintrag hinzufügt, verschiebt oder einen leeren oder doppelten Eintrag erfassen will, dann verhält sie sich wie die Liste „Trainingsbereich“.
- Wenn der Benutzer eine Tätigkeit umbenennt, die in Wartungen vorkommt, dann fragt die App mit der Anzahl („In 32 Wartungen ‚Kettenwechsel‘ durch ‚Kette gewechselt‘ ersetzen?“) und ändert nach „Ja“ die Liste und alle diese Wartungen; bei „Nein“ bleibt alles unverändert.
- Wenn der Benutzer eine Tätigkeit löschen will, die in Wartungen vorkommt, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl dieser Wartungen.
- Wenn die Liste „Tätigkeit“ geändert wurde, dann bieten danach geöffnete Wartungsfenster den neuen Stand an, und umbenannte Tätigkeiten erscheinen sofort in der Tabelle.

### Grenz- und Fehlerfälle
- Wenn im Wartungsfenster kein Rad oder keine Tätigkeit gewählt ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
- Wenn das Datum ungültig ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
- Wenn das Datum in der Zukunft liegt, dann ist „OK“ nicht möglich, und es erscheint „Das Datum liegt in der Zukunft.“
- Wenn die Liste „Tätigkeit“ leer ist, dann weist das Wartungsfenster darauf hin, dass zuerst in den Einstellungen Tätigkeiten angelegt werden müssen.
- Wenn ein Rad ausgemustert ist, dann wird es für neue Wartungen nicht angeboten; seine Wartungen bleiben sichtbar und lassen sich weiter bearbeiten.
- Wenn keine oder mehrere Wartungen markiert sind, dann ist „Bearbeiten…“ nicht wählbar; ohne Markierung ist auch „Löschen“ nicht wählbar.
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Tabelle und Liste bleiben beim vorherigen Stand.
