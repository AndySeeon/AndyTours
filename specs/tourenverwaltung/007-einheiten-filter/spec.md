# 007 – Filter für die Trainingseinheiten

## Was und warum
Über der Tabelle der Trainingseinheiten stehen Filter für Kategorie, Equipment, Trainingsbereich, Distanz, Höhenmeter und Name, wie in BikeXperience. Damit grenzt der Benutzer die Einheiten der Jahres- und Aktivitätsauswahl weiter ein, etwa auf alle Touren mit einem bestimmten Rad oder alle über 100 km, und sieht Summen und Bestwerte genau dieser Einheiten. Die Filter wirken sofort und sind nach einem Neustart wieder leer.

## Kriterien
- Wenn das Hauptfenster geöffnet ist, dann stehen über der Tabelle Filter für Kategorie, Equipment, Trainingsbereich, Distanz (von–bis), Höhenmeter (von–bis) und Name, und keiner davon ist gesetzt.
- Wenn der Benutzer die App neu startet, dann sind alle Filter wieder leer.
- Wenn der Benutzer eine der Auswahllisten Kategorie, Equipment oder Trainingsbereich öffnet, dann bietet sie nur die Werte an, die in der aktuellen Jahres- und Aktivitätsauswahl vorkommen, alphabetisch sortiert.
- Wenn der Benutzer eine Kategorie wählt, dann zeigt die Tabelle nur Einheiten, denen diese Kategorie zugeordnet ist, auch wenn sie weitere Kategorien haben.
- Wenn der Benutzer ein Equipment wählt, dann zeigt die Tabelle nur Einheiten mit diesem Equipment.
- Wenn der Benutzer einen Trainingsbereich wählt, dann zeigt die Tabelle nur Einheiten mit diesem Trainingsbereich.
- Wenn der Benutzer bei der Distanz einen Von- und/oder Bis-Wert einträgt, dann zeigt die Tabelle nur Einheiten in diesem Bereich, die Grenzwerte eingeschlossen.
- Wenn der Benutzer bei den Höhenmetern einen Von- und/oder Bis-Wert einträgt, dann zeigt die Tabelle nur Einheiten in diesem Bereich, die Grenzwerte eingeschlossen.
- Wenn bei aktivem Distanz- oder Höhenmeterfilter eine Einheit keinen solchen Wert hat, dann wird sie nicht angezeigt.
- Wenn der Benutzer Text in das Namensfeld schreibt, dann zeigt die Tabelle nur Einheiten, deren Name diesen Text enthält, ohne Rücksicht auf Groß- und Kleinschreibung.
- Wenn der Benutzer einen Filter ändert, dann wird die Tabelle sofort angepasst, ohne dass er eine Schaltfläche drücken muss.
- Wenn mehrere Filter gesetzt sind, dann zeigt die Tabelle nur Einheiten, die alle Filter gleichzeitig erfüllen.
- Wenn ein Filter gesetzt ist, dann beziehen sich die Summenzeile und die roten Bestwerte nur auf die angezeigten Einheiten.
- Wenn keine Einheit zu den Filtern passt, dann zeigt die Karteikarte den Hinweis „Keine Einheiten passen zum Filter.“ und keine Summenzeile.
- Wenn bei Distanz oder Höhenmetern keine gültige Zahl steht oder „von“ größer als „bis“ ist, dann wird das Feld rot markiert und dieser Filter nicht angewendet.
- Wenn der Benutzer „Filter zurücksetzen“ wählt, dann werden alle Filter geleert, und die Tabelle zeigt wieder die ganze Jahres- und Aktivitätsauswahl.
- Wenn mindestens ein Filter gesetzt ist, dann ist die Schaltfläche „Filter zurücksetzen“ hervorgehoben, sodass erkennbar ist, dass die Tabelle gefiltert ist.
- Wenn der Benutzer Jahr oder Aktivität wechselt oder der Datenbestand neu geladen wird, dann bleiben die gesetzten Filter erhalten und wirken auf die neue Auswahl.
- Wenn ein gewählter Wert einer Auswahlliste in der neuen Auswahl nicht mehr vorkommt, dann bleibt er gewählt, und die Tabelle zeigt den Hinweis „Keine Einheiten passen zum Filter.“
