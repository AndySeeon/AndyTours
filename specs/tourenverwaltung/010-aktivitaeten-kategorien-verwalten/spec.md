# 010 – Aktivitäten und Kategorien verwalten

## Was und warum
Der Benutzer pflegt Aktivitäten und Kategorien in den Einstellungen unter „Auswahllisten“, neben den Trainingsbereichen: anlegen, umbenennen, löschen, Hauptaktivität und Standardkategorien festlegen und zwei Einträge zusammenführen. Bisher gab es nur die aus BikeXperience übernommenen Einträge; neue Sportarten oder Kategorien ließen sich nicht anlegen, doppelte wie „Trainieren“ und „Training“ nicht bereinigen. Geschwindigkeitseinheit und Saisonziele der Aktivitäten gehören nicht dazu.

## Kriterien

### Anzeigen
- Wenn der Benutzer in den Einstellungen unter „Auswahllisten“ die Liste wählt, dann findet er neben „Trainingsbereich“ auch „Aktivität“ und „Kategorie“.
- Wenn die Liste „Aktivität“ gewählt ist, dann zeigt sie die Aktivitäten in der festgelegten Reihenfolge, die Hauptaktivität mit dem Zusatz „(Hauptaktivität)“.
- Wenn die Liste „Kategorie“ gewählt ist, dann zeigt sie die Kategorien alphabetisch, Standardkategorien mit dem Zusatz „(Standard)“; „Nach oben“ und „Nach unten“ sind dort nicht wählbar.

### Anlegen und umbenennen
- Wenn der Benutzer eine Aktivität oder Kategorie hinzufügt, dann ist sie sofort gespeichert und überall wählbar: in der Werkzeugleiste (Aktivität), im Bearbeitungsfenster, in der Tabellenzelle und, sobald Einheiten sie verwenden, im Filter.
- Wenn ein neuer oder umbenannter Name leer ist oder es ihn in der Liste schon gibt (ohne Unterschied zwischen Groß- und Kleinschreibung; Leerzeichen am Anfang und Ende werden entfernt), dann wird er abgewiesen, und ein Hinweis nennt den Grund.
- Wenn der Benutzer eine Aktivität oder Kategorie umbenennt, dann zeigen Tabelle, Werkzeugleiste, Filter und Auswahllisten sofort den neuen Namen; eine Rückfrage gibt es nicht, weil die Zuordnung der Einheiten erhalten bleibt.
- Wenn eine umbenannte Kategorie bei einem Equipment als Standardkategorie eingetragen ist, dann wird sie dort mit umbenannt.

### Hauptaktivität, Standardkategorien, Reihenfolge
- Wenn der Benutzer eine Aktivität markiert und „Als Hauptaktivität“ wählt, dann ist genau diese die Hauptaktivität: Sie ist beim nächsten Start in der Werkzeugleiste vorgewählt und wird bei „Neue Einheit“ unter „alle Aktivitäten“ vorbelegt.
- Wenn der Benutzer eine Kategorie markiert und „Standard“ umschaltet, dann ist sie Standardkategorie bzw. nicht mehr; Standardkategorien sind bei „Neue Einheit“ angehakt, beliebig viele sind möglich.
- Wenn der Benutzer eine Aktivität nach oben oder unten verschiebt, dann folgen Werkzeugleiste und Auswahllisten sofort der neuen Reihenfolge, und sie bleibt beim nächsten Start erhalten.

### Löschen
- Wenn der Benutzer eine Aktivität löschen will, die Einheiten oder Equipments verwenden, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl der Einheiten bzw. Equipments.
- Wenn der Benutzer die Hauptaktivität oder die letzte verbleibende Aktivität löschen will, dann ist das nicht möglich, und ein Hinweis nennt den Grund.
- Wenn der Benutzer eine Kategorie löschen will, die Einheiten verwenden oder die bei einem Equipment als Standardkategorie eingetragen ist, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl der Einheiten bzw. das Equipment.
- Wenn eine Aktivität oder Kategorie nicht verwendet wird (und bei einer Aktivität weder Hauptaktivität noch die letzte ist), dann verschwindet sie mit „Löschen“ ohne Rückfrage.

### Zusammenführen
- Wenn der Benutzer eine Aktivität oder Kategorie markiert und „Zusammenführen mit …“ wählt, dann wählt er in einem kleinen Fenster das Ziel aus den übrigen Einträgen derselben Liste; gibt es keinen anderen Eintrag, ist „Zusammenführen mit …“ nicht wählbar.
- Wenn der Benutzer das Ziel gewählt hat, dann fragt die App mit der Anzahl („42 Einheiten von ‚Trainieren‘ nach ‚Training‘ übernehmen und ‚Trainieren‘ entfernen?“); bei „Nein“ bleibt alles unverändert.
- Wenn der Benutzer zwei Kategorien zusammenführt und „Ja“ wählt, dann haben alle Einheiten der Ursprungskategorie die Zielkategorie – eine Einheit, die beide hatte, hat sie danach nur einmal –, und die Ursprungskategorie ist aus der Liste entfernt.
- Wenn die zusammengeführte Kategorie Standardkategorie war oder bei einem Equipment als Standardkategorie eingetragen ist, dann gilt das danach für die Zielkategorie.
- Wenn der Benutzer zwei Aktivitäten zusammenführt und „Ja“ wählt, dann gehören alle Einheiten und Equipments der Ursprungsaktivität zur Zielaktivität, und die Ursprungsaktivität ist entfernt; war sie die Hauptaktivität, dann ist danach die Zielaktivität die Hauptaktivität.
- Wenn zusammengeführt wurde, dann zeigen Tabelle, Summen, Werkzeugleiste und Filter sofort den neuen Stand.

### Fehler
- Wenn das Speichern einer Änderung oder das Zusammenführen fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Listen, Einheiten und Equipments bleiben beim vorherigen Stand.
