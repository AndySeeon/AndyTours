# 006 – Tabelle der Trainingseinheiten

## Was und warum
Die Karteikarte „Trainingseinheiten“ zeigt alle Einheiten der Jahres- und Aktivitätsauswahl in einer sortierbaren Tabelle, wie in BikeXperience. Eine Summenzeile fasst die angezeigten Einheiten zusammen, die Bestwerte der Auswahl sind rot hervorgehoben. So sieht der Benutzer seine Touren auf einen Blick und erkennt sofort die längste, höchste und schnellste.

Spalten: Datum, Startzeit, Name, Aktivität, Kategorien, Equipment, Distanz, Höhenmeter, Fahrzeit, Ø km/h, max km/h, Ø Puls, max Puls, Trittfrequenz, Temperatur, Maximalhöhe, Trainingsbereich. Auswahl, Reihenfolge und Breite der Spalten passt der Benutzer an, sie bleiben erhalten. Ebenso merkt sich die App Größe und Position des Hauptfensters und die zuletzt gewählte Karteikarte (nach der ersten und zweiten Prüfung ergänzt). Filter, Bearbeiten, Anhänge und Suche sind eigene Funktionen.

## Kriterien
- Wenn das Hauptfenster geöffnet ist, dann zeigt die Karteikarte „Trainingseinheiten“ alle Einheiten der aktuellen Jahres- und Aktivitätsauswahl als Tabelle, eine Zeile je Einheit.
- Wenn der Benutzer Jahr oder Aktivität ändert oder der Datenbestand neu geladen wird (Datenordnerwechsel, Wiederherstellen einer Sicherung), dann zeigt die Tabelle sofort die passenden Einheiten.
- Wenn die Tabelle angezeigt wird, dann ist sie nach Datum sortiert, die neueste Einheit zuerst.
- Wenn der Benutzer auf eine Spaltenüberschrift klickt, dann wird nach dieser Spalte sortiert; ein erneuter Klick kehrt die Richtung um.
- Wenn der Benutzer mit der rechten Maustaste auf die Spaltenüberschriften klickt, dann kann er jede Spalte ein- oder ausblenden; anfangs sind alle Spalten sichtbar.
- Wenn der Benutzer Spalten ein- oder ausgeblendet hat, dann bleibt diese Wahl nach einem Neustart erhalten.
- Wenn nur noch eine Spalte sichtbar ist, dann lässt sie sich nicht ausblenden.
- Wenn der Benutzer im Spaltenmenü eine Spalte an- oder abwählt, dann bleibt das Menü offen, sodass er mehrere Spalten nacheinander umschalten kann; „Übernehmen“ am Ende des Menüs schließt es.
- Wenn der Benutzer eine Spaltenüberschrift an eine andere Stelle zieht, dann wird die Spalte dorthin verschoben, und die Summenzeile folgt.
- Wenn der Benutzer Reihenfolge oder Breite der Spalten geändert hat, dann bleiben sie nach einem Neustart erhalten.
- Wenn eine Spalte automatisch an ihren Inhalt angepasst wird (Doppelklick auf den Spaltenrand) oder der Benutzer sie verkleinert, dann bleibt sie mindestens so breit, dass ihr Wert in der Summenzeile vollständig sichtbar ist.
- Wenn der Benutzer die App beendet, dann werden Größe, Position und maximierter Zustand des Hauptfensters gemerkt und beim nächsten Start wiederhergestellt.
- Wenn die gemerkte Position außerhalb der vorhandenen Bildschirme liegt, dann erscheint das Hauptfenster auf dem Hauptbildschirm.
- Wenn der Benutzer die App beendet, dann wird die zuletzt gewählte Karteikarte gemerkt und beim nächsten Start wieder angezeigt.
- Wenn die Tabelle Einheiten enthält, dann zeigt eine Summenzeile die Anzahl der angezeigten Einheiten sowie die Summen von Distanz, Höhenmetern und Fahrzeit.
- Wenn der Benutzer in der Tabelle blättert oder sortiert, dann bleibt die Summenzeile fest am unteren Rand sichtbar und wird nicht mitsortiert.
- Wenn eine Einheit in einer der Spalten Distanz, Höhenmeter, Fahrzeit, Ø km/h oder max km/h den höchsten Wert der angezeigten Auswahl hat, dann ist dieser Wert rot dargestellt.
- Wenn mehrere Einheiten denselben Bestwert haben, dann sind alle diese Werte rot.
- Wenn die Auswahl keine Einheiten enthält, dann zeigt die Karteikarte den Hinweis „Keine Einheiten in dieser Auswahl.“ und keine Summenzeile.
- Wenn bei einer Einheit ein Wert fehlt (z. B. kein Puls aufgezeichnet), dann bleibt die Zelle leer statt „0“, und der Wert zählt nicht als Bestwert.
- Wenn Zahlen, Datum und Zeiten angezeigt werden, dann im deutschen Format (z. B. „1.234,5“ km, „Sa, 23.10.2026“, „3:45:12“).
