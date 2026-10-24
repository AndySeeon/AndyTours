# 008 – Trainingseinheit anlegen, bearbeiten, kopieren und löschen

## Was und warum
Der Benutzer kann Trainingseinheiten neu anlegen, ändern, kopieren und löschen, wie mit dem Dialog in BikeXperience. Das Bearbeitungsfenster zeigt alle Werte einer Einheit, auch die, die in der Tabelle nicht vorkommen. Einzelne Werte lassen sich zusätzlich direkt in der Tabelle ändern. So pflegt er seine Touren ohne Umweg, berichtigt Fehler aus Aufzeichnungen und erfasst Einheiten ohne Aufzeichnung von Hand. Diese Funktion ist auch die Grundlage für die späteren Importe.

Die Befehle stehen im Menü „Einheit“ und im Rechtsklickmenü der Tabelle. Mehrfachaktionen (Kategorie, Aktivität, Equipment), Anhänge öffnen und Werte aus der Aufzeichnung übernehmen sind eigene Funktionen.

## Kriterien

### Aufrufen
- Wenn der Benutzer „Neue Einheit“ wählt, dann öffnet sich das Bearbeitungsfenster mit dem heutigen Datum, der Aktivität aus der Werkzeugleiste (bei „alle Aktivitäten“ die Hauptaktivität) und den als Standard markierten Kategorien; alle anderen Felder sind leer.
- Wenn der Benutzer eine Zeile doppelt anklickt oder sie markiert und „Bearbeiten“ wählt, dann öffnet sich das Bearbeitungsfenster mit allen Werten dieser Einheit.
- Wenn der Benutzer eine Einheit markiert und „Kopieren“ wählt, dann öffnet sich das Bearbeitungsfenster mit einer neuen Einheit, die alle Werte der markierten übernimmt, außer dem Datum (heute) und den Anhängen; das Original bleibt unverändert.
- Wenn keine Zeile markiert ist, dann sind „Bearbeiten“, „Kopieren“ und „Löschen“ nicht wählbar; sind mehrere markiert, dann sind „Bearbeiten“ und „Kopieren“ nicht wählbar.
- Wenn der Benutzer die Befehle sucht, dann findet er „Neue Einheit“, „Bearbeiten“, „Kopieren“ und „Löschen“ im Menü „Einheit“ und im Rechtsklickmenü der Tabelle.

### Bearbeitungsfenster
- Wenn das Bearbeitungsfenster offen ist, dann lassen sich darin Datum, Startzeit, Name, Aktivität, Kategorien (mehrere), Equipment, Distanz, Höhenmeter, Fahrzeit, max km/h, Ø, max und min Puls, Trittfrequenz, Temperatur, Leistung, Körpergewicht, Erholungspuls, Starthöhe, Maximalhöhe, Trainingsbereich und Kommentar bearbeiten.
- Wenn Distanz und Fahrzeit eingetragen sind, dann zeigt das Fenster die daraus berechneten Ø km/h an, nur zur Ansicht.
- Wenn Datum oder Aktivität fehlen oder ein Wert ungültig ist (keine Zahl, negativ außer bei der Temperatur, Fahrzeit nicht im Format „h:mm“ oder „h:mm:ss“; fehlen die Sekunden, gelten sie als 00), dann ist das Feld rot markiert und „Speichern“ nicht möglich; alle anderen Felder dürfen leer bleiben.
- Wenn der Benutzer den Trainingsbereich wählt, dann bietet die Auswahlliste die Kürzel an, die im Datenbestand vorkommen, und einen leeren Eintrag; ein neues Kürzel lässt sich nicht eintippen (das Pflegen der Liste ist eine eigene Funktion).
- Wenn der Benutzer das Equipment wählt, dann bietet die Auswahlliste „(kein)“ und alle nicht ausgemusterten Equipments an; ein ausgemustertes erscheint nur, wenn es der bearbeiteten Einheit bereits zugeordnet ist.
- Wenn der Benutzer „Speichern“ wählt, dann wird die Einheit dauerhaft gespeichert, das Fenster schließt sich, und Tabelle, Summen, Bestwerte, Jahresliste und Statusleiste zeigen sofort den neuen Stand; die gespeicherte Einheit ist in der Tabelle markiert.
- Wenn die gespeicherte Einheit nicht zur aktuellen Jahres-, Aktivitäts- oder Filterauswahl passt, dann bleibt die Auswahl unverändert, und die Statusleiste meldet „Gespeichert – liegt außerhalb der aktuellen Auswahl.“
- Wenn der Benutzer „Abbrechen“ wählt oder das Fenster schließt, ohne etwas geändert zu haben, dann schließt sich das Fenster, und der Datenbestand bleibt unverändert.
- Wenn der Benutzer „Abbrechen“ wählt oder das Fenster schließt, nachdem er etwas geändert hat, dann fragt die App „Änderungen verwerfen?“; bei „Ja“ schließt sich das Fenster ohne Speichern, bei „Nein“ bleibt es mit den Eingaben offen.
- Wenn eine Einheit mit Anhängen bearbeitet wird, dann bleiben ihre Anhänge erhalten.

### Bearbeiten in der Tabelle
- Wenn der Benutzer in einer markierten Zelle F2 drückt, die bereits markierte Zelle anklickt oder lostippt, dann kann er den Wert direkt in der Tabelle ändern; ein Doppelklick öffnet weiterhin das Bearbeitungsfenster.
- Wenn der Benutzer eine Zelle bearbeitet, dann lassen sich alle Spalten außer Ø km/h und Kategorien ändern; Aktivität, Equipment und Trainingsbereich werden aus einer Auswahlliste gewählt.
- Wenn der Benutzer die Zelle verlässt oder Enter drückt, dann wird die Änderung sofort dauerhaft gespeichert.
- Wenn der Benutzer während der Bearbeitung einer Zelle Esc drückt, dann wird die Änderung verworfen und der alte Wert angezeigt.
- Wenn der Benutzer in einer Zelle einen ungültigen Wert einträgt (z. B. „abc“ bei Distanz, ein leeres Datum, eine negative Zahl außer bei der Temperatur), dann wird die Zelle rot markiert und bleibt in Bearbeitung, bis der Wert gültig ist oder der Benutzer mit Esc abbricht.
- Wenn ein Wert in der Tabelle geändert wurde, dann werden Ø km/h, Summenzeile und rote Bestwerte sofort neu berechnet; ist die Tabelle nach der geänderten Spalte sortiert, springt die Zeile an ihre neue Stelle.

### Löschen
- Wenn der Benutzer eine Einheit markiert und „Löschen“ wählt oder die Entf-Taste drückt (nicht während der Bearbeitung einer Zelle), dann fragt die App „Einheit ‚Name‘ vom Datum wirklich löschen?“ und löscht erst nach „Ja“.
- Wenn eine Einheit mit Anhängen gelöscht wird, dann bleiben die Dateien der Anhänge im Datenordner erhalten; nur ihre Zuordnung zur Einheit entfällt.
- Wenn mehrere Einheiten markiert sind und der Benutzer löscht, dann fragt die App einmal mit der Anzahl („3 Einheiten wirklich löschen?“) und löscht nach „Ja“ alle.
- Wenn der Benutzer bei der Rückfrage „Nein“ wählt, dann bleibt alles unverändert.
- Wenn Einheiten gelöscht sind, dann verschwinden sie sofort aus der Tabelle, und Summen, Bestwerte, Jahresliste und Statusleiste zeigen den neuen Stand.

### Fehler
- Wenn das Speichern oder Löschen fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und der angezeigte Datenbestand bleibt beim vorherigen Stand; ein offenes Bearbeitungsfenster bleibt mit den Eingaben offen, eine Zelle zeigt wieder ihren alten Wert.
