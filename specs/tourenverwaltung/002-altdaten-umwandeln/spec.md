# 002 – Altdaten umwandeln

## Was und warum
Ein eigenständiges Programm außerhalb von AndyTours wandelt die BikeXperience-Daten einmalig in einen AndyTours-Datenordner um: Trainingseinheiten, Räder, Wartungen, Aktivitäten, Kategorien, Schwellwerte, persönliche Daten und Aufzeichnungen. Ein Bericht zeigt, was übernommen wurde und was auffällig war. So braucht die App selbst keinen Altdaten-Import, und die Übernahme lässt sich jederzeit wiederholen, z. B. nach Korrekturen.

## Kriterien
- Wenn der Benutzer das Programm mit dem BikeXperience-Ordner und einem Zielordner aufruft, dann schreibt es in diesen Zielordner.
- Wenn der Benutzer keinen Zielordner angibt, dann schreibt das Programm in den Ordner `AppDaten` im Projektordner.
- Wenn der Benutzer auch keinen BikeXperience-Ordner angibt, dann liest das Programm aus dem Ordner `Daten` im Projektordner.
- Wenn die Umwandlung fertig ist, dann enthält der Zielordner alle Trainingseinheiten, Räder, Wartungen, Aktivitäten, Kategorien, Schwellwerte und persönlichen Daten aus BikeXperience.
- Wenn die Umwandlung fertig ist, dann liegen alle Aufzeichnungen und Anhänge im Zielordner und lassen sich den Trainingseinheiten zuordnen.
- Wenn die Umwandlung fertig ist, dann liegt im Zielordner ein Bericht mit den übernommenen Mengen, den Jahressummen (Anzahl, km, hm, Fahrzeit), den Hinweisen, Warnungen und nicht reparierten Umlauten.
- Wenn die Umwandlung fertig ist, dann zeigt das Programm eine kurze Zusammenfassung (Mengen, Anzahl der Warnungen) und wo der Bericht liegt.
- Wenn die Jahressummen im Bericht mit der Statistik von BikeXperience verglichen werden, dann stimmen sie überein.
- Wenn Texte beschädigte Umlaute enthalten, dann werden sie repariert, soweit das eindeutig möglich ist, und die übrigen Stellen nennt der Bericht.
- Wenn ein Tourname „Stoisser B?ckeralm“ mit beschädigtem Umlaut lautet, dann heißt er nach der Umwandlung „Stoisser Bäckeralm“.
- Wenn ein Tourname die Abkürzung „(?PD)“ mit beschädigtem Umlaut enthält, dann lautet sie nach der Umwandlung „(ÜPD)“.
- Wenn zu einer Trainingseinheit die Aufzeichnung fehlt, dann wird die Einheit trotzdem übernommen und der Bericht nennt die fehlende Datei.
- Wenn eine Zeile der Altdaten fehlerhaft ist, dann wird der Rest trotzdem übernommen und der Bericht nennt die Zeile.
- Wenn die alte Konfiguration Zugangsdaten enthält (Dropbox, Web-Zugang), dann werden diese nicht übernommen.
- Wenn im angegebenen BikeXperience-Ordner keine BikeXperience-Daten liegen, dann meldet das Programm das verständlich und schreibt nichts.
- Wenn der Zielordner bereits AndyTours-Daten enthält, dann fragt das Programm, ob sie ersetzt werden sollen; ohne ausdrückliches Ja bleibt alles unverändert.
- Wenn der Benutzer das Ersetzen bestätigt, dann enthält der Zielordner danach nur die neu umgewandelten Daten.
- Wenn die Umwandlung mit einem Fehler abbricht, dann bleibt der Zielordner wie vorher.
- Wenn die Umwandlung läuft, dann bleibt der BikeXperience-Ordner unverändert.
