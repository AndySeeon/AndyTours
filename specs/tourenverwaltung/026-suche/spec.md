# 026 – Suche in Name und Kommentar

## Was und warum
Das Feld „Name:“ der Filterleiste wird zum Feld „Suche:“ und findet Einheiten über einen Suchbegriff in Name und Kommentar – z. B. alle Touren „mit Sabine“. Damit erkennbar ist, warum eine Einheit gefunden wurde, zeigt die Einheitentabelle den Kommentar in einer eigenen Spalte. Anhänge werden nicht durchsucht, weil es nur Aufzeichnungsdateien mit Zeitstempel als Namen sind.

## Kriterien

### Suchen
- Wenn in der Filterleiste ein Suchbegriff eingegeben wird, dann zeigt die Tabelle nur die Einheiten, deren Name oder Kommentar den Begriff enthält, ohne Unterschied der Groß- und Kleinschreibung.
- Wenn der Suchbegriff aus mehreren Wörtern besteht, dann muss jedes Wort in Name oder Kommentar vorkommen, in beliebiger Reihenfolge (z. B. findet „Sabine Alm“ eine Einheit „Bründlingalm“ mit dem Kommentar „mit Sabine“).
- Wenn der Suchbegriff geändert wird, dann wird die Tabelle sofort beim Tippen neu gefiltert.
- Wenn nur Leerzeichen eingegeben sind, dann wird nicht gefiltert.
- Wenn die Suche zusammen mit den übrigen Filtern und mit der Jahres- und Aktivitätsauswahl wirkt, dann zeigt die Tabelle nur Einheiten, die alles erfüllen; für eine Suche über alle Jahre wird oben „alle Jahre“ gewählt.
- Wenn keine Einheit passt, dann erscheint der Hinweis „Keine Einheiten passen zum Filter.“.
- Wenn „Filter zurücksetzen“ gewählt wird, dann ist das Suchfeld leer.
- Wenn die Filterleiste angezeigt wird, dann heißt das Feld „Suche:“ (bisher „Name:“).

### Spalte „Kommentar“
- Wenn die Einheitentabelle angezeigt wird, dann zeigt die Spalte „Kommentar“ den Kommentar jeder Einheit; ohne Kommentar bleibt sie leer.
- Wenn nach der Spalte „Kommentar“ sortiert wird, dann gilt die alphabetische Reihenfolge; ein zweiter Klick kehrt sie um.
- Wenn ein Kommentar im Bearbeitungsfenster geändert wird, dann zeigt die Spalte sofort den neuen Text, und die Suche berücksichtigt ihn.
