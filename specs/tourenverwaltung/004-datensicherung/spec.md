# 004 – Automatische Datensicherung

## Was und warum
AndyTours sichert die Datenbank automatisch beim Programmstart, jeweils nach einer einstellbaren Zahl von Programmstarts, in einen frei wählbaren Sicherungsordner. Die Sicherung ist immer aktiv. Nur die eingestellte Höchstzahl an Sicherungen wird aufbewahrt, die ältesten werden gelöscht. So gehen bei einem Defekt oder Bedienfehler höchstens die Änderungen der letzten Programmstarts verloren, wie bei BikeXperience.

Standardwerte, solange in den Einstellungen nichts gewählt ist: alle 3 Programmstarts, höchstens 10 Sicherungen, Sicherungsordner `Sicherungen` im Datenordner. Das Zurückspielen einer Sicherung ist nicht Teil dieser Funktion (siehe `sicherung-wiederherstellen`).

## Kriterien
- Wenn die App startet und seit der letzten Sicherung die eingestellte Zahl von Programmstarts erreicht ist, dann legt sie vor der Anzeige der Touren eine Kopie der Datenbank mit Datum und Uhrzeit im Namen im Sicherungsordner an.
- Wenn die eingestellte Zahl von Programmstarts noch nicht erreicht ist, dann legt die App keine Kopie an.
- Wenn eine Sicherung angelegt wurde, dann beginnt die Zählung der Programmstarts von vorn.
- Wenn eine Sicherung angelegt wurde, dann ist sie vollständig: zurückgespielt enthält sie alle Touren, Räder und Wartungen mit dem Stand zum Zeitpunkt der Sicherung.
- Wenn danach mehr Sicherungen vorhanden sind als die eingestellte Höchstzahl, dann löscht die App die ältesten; andere Dateien im Sicherungsordner bleiben unberührt.
- Wenn die Sicherung fehlschlägt (Ordner nicht erreichbar, nicht beschreibbar oder Datenträger voll), dann erscheint eine verständliche Meldung, die App startet normal weiter und versucht es beim nächsten Start erneut.
- Wenn die Datenbank noch keine Touren enthält, dann legt die App keine Sicherung an.
- Wenn der Benutzer in den Einstellungen nichts gewählt hat, dann wird alle 3 Programmstarts in den Ordner `Sicherungen` im Datenordner gesichert und es werden höchstens 10 Sicherungen aufbewahrt.
- Wenn der Benutzer in den Einstellungen einen Sicherungsordner wählt, dann sichert die App künftig dorthin, und die Wahl bleibt nach einem Neustart erhalten.
- Wenn der Benutzer in den Einstellungen das Intervall (Anzahl der Programmstarts) oder die Höchstzahl der Sicherungen ändert, dann gelten die neuen Werte und bleiben nach einem Neustart erhalten.
- Wenn der Benutzer für das Intervall oder die Höchstzahl einen Wert außerhalb von 1 bis 99 oder keine ganze Zahl eingibt, dann wird der Wert nicht übernommen und ein Hinweis nennt den erlaubten Bereich 1 bis 99.
- Wenn der Benutzer den Datenordner wechselt, dann sichert die App künftig die Datenbank des neuen Ordners.
