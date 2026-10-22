# 001 – Datenordner wählen

## Was und warum
Alle Daten von AndyTours (Datenbestand, Aufzeichnungen, Anhänge) liegen in einem Datenordner. Der Benutzer wählt diesen Ordner frei und kann ihn in den Einstellungen ändern, damit die Daten z. B. auf dem NAS liegen können. Während der Entwicklung arbeitet die App immer mit einem eigenen Ordner im Projekt, damit die echten Daten unberührt bleiben.

## Kriterien
- Wenn die fertige App zum ersten Mal startet, dann verwendet sie ohne Rückfrage den Ordner `Dokumente\AndyTours` als Datenordner.
- Wenn der Datenordner beim Start noch nicht existiert, dann legt die App ihn an.
- Wenn die App während der Entwicklung gestartet wird, dann verwendet sie immer den Ordner `AppDaten` im Projektordner, unabhängig von der Einstellung der fertigen App.
- Wenn der Benutzer im Menü „Extras → Einstellungen…“ wählt, dann öffnen sich die Einstellungen und zeigen den vollständigen Pfad des aktuellen Datenordners.
- Wenn die App während der Entwicklung läuft, dann lässt sich der Datenordner in den Einstellungen nicht ändern und ein Hinweis erklärt, warum.
- Wenn der Benutzer in der fertigen App einen anderen Ordner wählt und bestätigt, dann arbeitet die App sofort mit den Daten dieses Ordners und zeigt den Ordner in der Statusleiste an.
- Wenn der Benutzer die fertige App nach einem Ordnerwechsel neu startet, dann ist der zuletzt gewählte Ordner aktiv.
- Wenn der Benutzer die Ordnerauswahl abbricht, dann bleibt der bisherige Datenordner aktiv.
- Wenn der Benutzer einen Ordner wählt, in dem die App nicht schreiben darf, dann erscheint eine verständliche Meldung und der bisherige Ordner bleibt aktiv.
- Wenn der Benutzer einen Ordner wählt, in dem noch keine AndyTours-Daten liegen, dann fragt die App, ob die bisherigen Daten dorthin kopiert werden sollen.
- Wenn der Benutzer das Kopieren bestätigt, dann liegen danach alle bisherigen Daten im neuen Ordner und die App arbeitet mit ihnen.
- Wenn der Benutzer das Kopieren ablehnt, dann beginnt die App im neuen Ordner mit einem leeren Datenbestand.
- Wenn die App Daten kopiert, dann zeigt sie den Fortschritt (Anteil, Anzahl Dateien, Datenmenge) an und friert dabei nicht ein; andere Aktionen sind bis zum Ende des Kopierens gesperrt.
- Wenn das Kopieren fehlschlägt, dann erscheint eine verständliche Meldung und der bisherige Ordner bleibt aktiv.
- Wenn der Benutzer einen Ordner wählt, in dem bereits AndyTours-Daten liegen, dann erscheint ein Hinweis, dass die dort vorhandenen Daten verwendet werden, mit der Wahl „OK“ oder „Abbrechen“.
- Wenn der Benutzer diesen Hinweis mit „OK“ bestätigt, dann arbeitet die App mit den Daten des gewählten Ordners.
- Wenn der Benutzer diesen Hinweis mit „Abbrechen“ schließt, dann bleibt der bisherige Datenordner aktiv.
- Wenn der Benutzer den Datenordner wechselt, dann bleiben die Daten im bisherigen Ordner unverändert erhalten.
- Wenn der gespeicherte Datenordner beim Start nicht erreichbar ist (z. B. NAS ausgeschaltet), dann erscheint eine Meldung mit der Wahl „Erneut versuchen“, „Anderen Ordner wählen“ oder „Beenden“.
