# 009 – Auswahllisten verwalten

## Was und warum
Der Benutzer pflegt in den Einstellungen unter „Auswahllisten“ die festen Werte, die in den Eingabefeldern angeboten werden. Diese Funktion legt den Bereich an und beginnt mit der Liste der Trainingsbereiche. Bisher kamen die angebotenen Kürzel nur aus den vorhandenen Einheiten, ein neues Kürzel ließ sich nicht erfassen und ein falsches nicht berichtigen. Aktivitäten, Kategorien und Equipments kommen später als weitere Listen in denselben Bereich; sie sind eigene Funktionen.

## Kriterien

### Pflegen
- Wenn der Benutzer die Einstellungen öffnet, dann findet er einen Bereich „Auswahllisten“ mit einer Auswahl der Liste (zunächst nur „Trainingsbereich“) und darunter deren Einträge in der festgelegten Reihenfolge.
- Wenn die App zum ersten Mal mit dieser Funktion gestartet wird, dann enthält die Liste „Trainingsbereich“ die Kürzel, die im Datenbestand vorkommen, alphabetisch geordnet.
- Wenn der Benutzer einen Eintrag hinzufügt, dann erscheint er am Ende der Liste und ist sofort dauerhaft gespeichert.
- Wenn ein neuer oder umbenannter Eintrag leer ist oder es ihn in der Liste schon gibt (ohne Unterschied zwischen Groß- und Kleinschreibung; Leerzeichen am Anfang und Ende werden entfernt), dann wird er abgewiesen, und ein Hinweis nennt den Grund.
- Wenn der Benutzer einen Eintrag nach oben oder unten verschiebt, dann gilt die neue Reihenfolge sofort und bleibt beim nächsten Start erhalten.
- Wenn der Benutzer einen Eintrag umbenennt, der in keiner Einheit vorkommt, dann ändert sich nur die Liste, ohne Rückfrage.
- Wenn der Benutzer einen Eintrag umbenennt, der in Einheiten vorkommt, dann fragt die App mit der Anzahl („In 714 Einheiten ‚GA2‘ durch ‚GA 2‘ ersetzen?“) und ändert nach „Ja“ die Liste und alle diese Einheiten; bei „Nein“ bleibt alles unverändert.
- Wenn der Benutzer einen Eintrag löschen will, der in Einheiten vorkommt, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl dieser Einheiten.
- Wenn der Benutzer einen Eintrag, der in keiner Einheit vorkommt, ausdrücklich mit „Löschen“ entfernt, dann verschwindet er ohne weitere Rückfrage aus der Liste; Einträge, die er nicht löscht, bleiben erhalten, auch wenn sie noch keiner Einheit zugeordnet sind.
- Wenn das Speichern einer Änderung fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Liste und Einheiten bleiben beim vorherigen Stand.

### Verwenden
- Wenn der Benutzer im Bearbeitungsfenster oder in einer Tabellenzelle den Trainingsbereich wählt, dann bietet die Auswahlliste einen leeren Eintrag und die Einträge der Liste in deren Reihenfolge an.
- Wenn die Liste geändert wurde, dann bieten danach geöffnete Bearbeitungsfenster und Tabellenzellen den neuen Stand an.
- Wenn ein Eintrag umbenannt wurde, dann zeigen Tabelle und Filterleiste sofort den neuen Namen.
- Wenn der Benutzer in der Filterleiste nach Trainingsbereich filtert, dann bietet die Liste weiterhin die Werte an, die in den Einheiten vorkommen.
