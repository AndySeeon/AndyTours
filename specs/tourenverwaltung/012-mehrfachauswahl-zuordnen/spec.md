# 012 – Mehrfachauswahl zuordnen

## Was und warum
Der Benutzer markiert eine oder mehrere Einheiten in der Tabelle und ordnet ihnen auf einmal Kategorien zu, wechselt ihre Aktivität oder ändert ihr Equipment. Bisher ging das nur Einheit für Einheit im Bearbeitungsfenster oder in der Tabellenzelle. Viele alte Einheiten lassen sich so in einem Schritt bereinigen, etwa dem richtigen Rad zuordnen.

## Kriterien

### Aufruf
- Wenn der Benutzer eine oder mehrere Einheiten in der Tabelle markiert hat, dann kann er im Menü „Einheit“ und im Kontextmenü der Tabelle „Kategorien zuordnen…“, „Aktivität ändern…“ und „Equipment ändern…“ wählen.
- Wenn keine Einheit markiert ist, dann sind diese Befehle nicht wählbar.

### Kategorien
- Wenn der Benutzer „Kategorien zuordnen…“ wählt, dann zeigt ein Fenster alle Kategorien: angehakt, wenn alle markierten Einheiten sie haben, leer, wenn keine sie hat, und teilweise angehakt, wenn nur manche sie haben.
- Wenn der Benutzer im Fenster eine Kategorie anhakt bzw. leert und „OK“ wählt, dann haben alle markierten Einheiten diese Kategorie bzw. keine von ihnen hat sie mehr; eine Kategorie, die auf „teilweise“ stehen bleibt, ändert sich bei keiner Einheit.

### Aktivität
- Wenn der Benutzer „Aktivität ändern…“ wählt, eine Aktivität auswählt und „OK“ wählt, dann gehören alle markierten Einheiten zu dieser Aktivität.
- Wenn die Aktivität geändert wird, dann behalten die Einheiten ihr Equipment, auch wenn es nicht zur neuen Aktivität passt.

### Equipment
- Wenn der Benutzer „Equipment ändern…“ wählt, dann bietet das Fenster „(kein)“ und alle nicht ausgemusterten Equipments an, die zu einer der Aktivitäten der markierten Einheiten passen oder keine Aktivität haben.
- Wenn der Benutzer ein Equipment auswählt und „OK“ wählt, dann haben alle markierten Einheiten dieses Equipment bzw. keines; ihre Kategorien bleiben unverändert.

### Gemeinsam
- Wenn mehr als eine Einheit markiert ist und der Benutzer „OK“ wählt, dann fragt die App mit der Anzahl nach, z. B. „42 Einheiten ändern?“; bei „Nein“ bleibt alles unverändert.
- Wenn der Benutzer im Fenster „Abbrechen“ wählt, dann bleibt alles unverändert.
- Wenn zugeordnet wurde, dann zeigen Tabelle, Summen und Filter sofort den neuen Stand, und die Einheiten bleiben markiert, soweit sie noch angezeigt werden.
- Wenn geänderte Einheiten danach nicht mehr zur Auswahl der Werkzeugleiste oder zum Filter passen, dann nennt die Statuszeile ihre Anzahl, z. B. „3 geänderte Einheiten liegen außerhalb der aktuellen Auswahl.“

### Fehler
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und keine der markierten Einheiten ist geändert.
