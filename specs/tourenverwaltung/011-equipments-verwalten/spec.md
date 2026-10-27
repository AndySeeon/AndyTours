# 011 – Equipments verwalten

## Was und warum
Der Benutzer pflegt seine Räder und seine Ausrüstung (Equipments) in den Einstellungen unter „Auswahllisten“: anlegen, bearbeiten, ausmustern und löschen. Je Aktivität kann ein Equipment Standard sein; es wird bei neuen Einheiten vorbelegt, und seine Standardkategorie wird mit angehakt. Im Bearbeitungsfenster einer Einheit stehen nur die Equipments der passenden Aktivität. Bisher gab es nur die aus BikeXperience übernommenen Equipments, und neue Räder ließen sich nicht anlegen. Gewicht, Radumfang, Widerstandswerte, Kettenblätter, Ritzel und Bild gehören nicht dazu, die Gerätezuordnung gehört zum Garmin-Import.

## Kriterien

### Anzeigen
- Wenn der Benutzer in den Einstellungen unter „Auswahllisten“ die Liste wählt, dann findet er auch „Equipment“.
- Wenn die Liste „Equipment“ gewählt ist, dann zeigt sie alle Equipments alphabetisch mit ihrer Aktivität, die ausgemusterten am Ende der Liste, ausgemusterte mit dem Zusatz „(ausgemustert)“ und Standard-Equipments mit dem Zusatz „(Standard)“.

### Anlegen und bearbeiten
- Wenn der Benutzer bei „Equipment“ „Hinzufügen“ oder für ein markiertes Equipment „Bearbeiten…“ wählt, dann öffnet sich ein Fenster mit Name, Aktivität, Standardkategorie, Anschaffungsdatum, Anfangskilometer, Standard und ausgemustert.
- Wenn der Benutzer im Fenster „OK“ wählt, dann ist das Equipment sofort gespeichert und im Bearbeitungsfenster der Einheiten, in der Tabellenzelle und, sobald Einheiten es verwenden, im Filter wählbar.
- Wenn der Name leer ist oder es ihn schon gibt (ohne Unterschied zwischen Groß- und Kleinschreibung; Leerzeichen am Anfang und Ende werden entfernt), dann wird nicht gespeichert, und ein Hinweis nennt den Grund.
- Wenn das Anschaffungsdatum kein gültiges Datum ist oder die Anfangskilometer keine Zahl von 0 an sind, dann wird nicht gespeichert, und ein Hinweis nennt den Grund.
- Wenn der Benutzer ein Equipment umbenennt, dann zeigen Tabelle, Filter und Auswahllisten sofort den neuen Namen, und die Einheiten behalten ihre Zuordnung.
- Wenn der Benutzer im Fenster „Abbrechen“ wählt, dann bleibt alles unverändert.

### Standard
- Wenn der Benutzer ein Equipment als Standard kennzeichnet, dann ist es das einzige Standard-Equipment seiner Aktivität; ein bisheriges Standard-Equipment derselben Aktivität verliert die Kennzeichnung.
- Wenn ein Equipment ausgemustert ist, dann ist es nicht Standard: Beim Ausmustern fällt die Kennzeichnung weg, und ein ausgemustertes Equipment lässt sich nicht als Standard kennzeichnen.
- Wenn ein ausgemustertes Equipment aus dem bisherigen Bestand noch als Standard gekennzeichnet ist, dann wird es weder als Standard angezeigt noch vorbelegt.
- Wenn der Benutzer „Neue Einheit“ wählt, dann ist das Standard-Equipment der Aktivität vorbelegt; gibt es keines, dann ist kein Equipment gewählt.

### Equipment und Einheit
- Wenn der Benutzer im Bearbeitungsfenster einer Einheit das Equipment aufklappt, dann stehen dort nur die nicht ausgemusterten Equipments der gewählten Aktivität, Equipments ohne Aktivität und das Equipment, das die Einheit bereits hat.
- Wenn der Benutzer bei einer neuen Einheit die Aktivität wechselt, dann ist das Standard-Equipment der neuen Aktivität gewählt, bzw. kein Equipment, wenn es keines gibt.
- Wenn der Benutzer bei einer Einheit ein Equipment mit Standardkategorie wählt, dann wird diese Kategorie zusätzlich angehakt; schon angehakte Kategorien bleiben angehakt.
- Wenn ein Equipment ausgemustert ist, dann behalten die Einheiten, die es verwenden, die Zuordnung, und Tabelle und Filter zeigen es weiter an.
- Wenn der Benutzer die Kennzeichnung „ausgemustert“ wieder entfernt, dann wird das Equipment wieder angeboten.

### Löschen
- Wenn der Benutzer ein Equipment löschen will, das Einheiten verwenden oder zu dem Wartungen erfasst sind, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl der Einheiten bzw. Wartungen und empfiehlt, es stattdessen auszumustern.
- Wenn ein Equipment von keiner Einheit verwendet wird und keine Wartungen hat, dann verschwindet es mit „Löschen“ ohne Rückfrage.

### Fehler
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Equipments und Einheiten bleiben beim vorherigen Stand.
