# 005 – Sicherung wiederherstellen

## Was und warum
Der Benutzer kann über „Extras → Sicherung wiederherstellen…“ eine der automatischen Sicherungen aus dem aktuellen Sicherungsordner auswählen und den Datenbestand nach einer Rückfrage auf deren Stand zurücksetzen. So lassen sich ein Defekt oder ein Bedienfehler ohne Handarbeit im Dateisystem beheben.

Vor dem Zurücksetzen sichert die App den aktuellen Stand als zusätzliche, ganz normale Sicherung; damit lässt sich auch das Wiederherstellen selbst rückgängig machen. Die Einstellungen der Datensicherung bleiben dabei unverändert.

## Kriterien
- Wenn der Benutzer „Extras → Sicherung wiederherstellen…“ wählt, dann zeigt die App alle Sicherungen des aktuellen Sicherungsordners mit Datum und Uhrzeit, die neueste zuerst.
- Wenn der Sicherungsordner keine Sicherungen enthält oder nicht erreichbar ist, dann erscheint ein Hinweis und der Datenbestand bleibt unverändert.
- Wenn der Benutzer die Auswahl abbricht, dann bleibt der Datenbestand unverändert.
- Wenn der Benutzer eine Sicherung auswählt, dann fragt die App nach; die Rückfrage nennt Datum und Uhrzeit der Sicherung und weist darauf hin, dass alle späteren Änderungen verloren gehen.
- Wenn der Benutzer die Rückfrage ablehnt, dann bleibt der Datenbestand unverändert.
- Wenn der Benutzer bestätigt, dann entspricht der Datenbestand dem Stand der Sicherung, und das Hauptfenster zeigt diesen Stand sofort an (Jahre, Aktivitäten, Summen), ohne Neustart.
- Wenn der Benutzer bestätigt, dann legt die App vorher eine Sicherung des aktuellen Stands im Sicherungsordner an; sie zählt wie jede andere Sicherung zur Höchstzahl.
- Wenn die Sicherung des aktuellen Stands fehlschlägt, dann erscheint eine verständliche Meldung, und es wird nichts wiederhergestellt.
- Wenn die gewählte Sicherung beschädigt ist oder keine AndyTours-Daten enthält, dann erscheint eine verständliche Meldung und der Datenbestand bleibt unverändert.
- Wenn das Wiederherstellen fehlschlägt (z. B. Ordner nicht erreichbar, Datenträger voll), dann erscheint eine verständliche Meldung und der bisherige Datenbestand bleibt vollständig erhalten.
- Wenn die Sicherung von einer älteren AndyTours-Version stammt, dann wird sie übernommen und ist danach normal nutzbar.
- Wenn die Sicherung von einer neueren AndyTours-Version stammt, dann lehnt die App sie mit einer Meldung ab und der Datenbestand bleibt unverändert.
- Wenn wiederhergestellt wurde, dann bleiben die Einstellungen der Datensicherung (Sicherungsordner, Intervall, Höchstzahl) so, wie sie vor dem Wiederherstellen waren.
