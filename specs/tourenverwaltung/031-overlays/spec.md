# 031 – Overlays

## Was und warum
Overlays sind gespeicherte Strecken, die zusätzlich auf einer Karte eingeblendet werden – etwa geplante Touren oder fremde GPX-Strecken. So sieht der Benutzer beim Ansehen einer Tour, einer Mehrtagestour oder beim Planen, wo er schon gefahren ist oder was er sich vorgemerkt hat. Die Overlays liegen in Ordnern mit Unterordnern, damit sie auch bei vielen Strecken übersichtlich bleiben. Verwaltet werden sie über den Menüpunkt „Tourenplanung → Overlays…“.

Nicht enthalten: Teilstücke eines Overlays in die Tourenplanung übernehmen („Zauberstab“), eigene Einheiten direkt als Overlay übernehmen (dafür zuerst den GPX-Export nutzen) und das Übertragen auf den Garmin Edge (gehört zu `courses-uebertragen`).

## Kriterien

### Verwaltung öffnen
- Wenn der Benutzer „Tourenplanung → Overlays…“ wählt, dann öffnet sich die Overlay-Verwaltung mit dem Ordnerbaum und den Overlays, jeweils mit einem Kontrollkästchen zum Einblenden.
- Wenn noch keine Overlays vorhanden sind, dann zeigt die Verwaltung einen Hinweis, wie man Overlays hinzufügt.

### Overlays aufnehmen
- Wenn der Benutzer GPX-Dateien hinzufügt, dann erscheinen sie als Overlays im gewählten Ordner, benannt nach dem Dateinamen ohne Endung.
- Wenn ein Overlay hinzugefügt wurde, dann bleibt es erhalten, auch wenn die ursprüngliche Datei danach verschoben oder gelöscht wird.
- Wenn eine Planung aus der Tourenplanung gespeichert wurde, dann lässt sich diese Datei wie jede andere GPX-Datei als Overlay hinzufügen.
- Wenn eine Datei keine Strecke enthält oder nicht lesbar ist, dann wird sie nicht aufgenommen und ein Hinweis nennt die Datei.
- Wenn mehrere Dateien auf einmal hinzugefügt werden und einzelne davon fehlerhaft sind, dann werden die übrigen trotzdem aufgenommen.

### Ordner verwalten
- Wenn der Benutzer einen Ordner anlegt, dann erscheint er im gewählten Ordner als Unterordner oder, wenn keiner gewählt ist, auf oberster Ebene.
- Wenn der Benutzer einen Ordner umbenennt, dann erscheint er unter dem neuen Namen.
- Wenn ein Ordnername leer ist oder im selben übergeordneten Ordner schon vorkommt, dann wird er nicht übernommen und ein Hinweis erscheint.
- Wenn der Benutzer einen Ordner löscht, der Overlays oder Unterordner enthält, dann kommt eine Rückfrage; nach dem Bestätigen sind der Ordner und sein gesamter Inhalt gelöscht.
- Wenn der Benutzer ein Overlay oder einen Ordner in einen anderen Ordner verschiebt, dann steht es danach dort.
- Wenn der Benutzer einen Ordner in sich selbst oder in einen seiner Unterordner verschieben will, dann geschieht nichts.

### Overlays bearbeiten
- Wenn der Benutzer ein Overlay umbenennt, dann erscheint es unter dem neuen Namen.
- Wenn der Benutzer ein Overlay löscht, dann kommt eine Rückfrage; nach dem Bestätigen ist es entfernt und verschwindet von allen Karten.

### Einblenden
- Wenn der Benutzer ein Overlay einschaltet, dann erscheint seine Strecke als Linie auf den Karten im Tourdetail, in der Mehrtagestour und in der Tourenplanung.
- Wenn der Benutzer einen Ordner einschaltet, dann werden alle Overlays darin einschließlich der Unterordner eingeschaltet; ausschalten wirkt ebenso auf den ganzen Inhalt.
- Wenn der Benutzer ein Overlay ausschaltet, dann verschwindet es von den Karten.
- Wenn eine Karte bereits geöffnet ist, während Overlays ein- oder ausgeschaltet werden, dann zeigt sie die Änderung sofort.
- Wenn Overlays eingeblendet sind, dann liegen sie unter der eigenen Strecke, den Wegpunkten und Abbiegepunkten und verdecken diese nicht.
- Wenn der Benutzer in der Verwaltung ein Overlay doppelt anklickt, dann zeigen die geöffneten Karten dessen ganze Strecke.
- Wenn die App neu gestartet wird, dann sind dieselben Overlays wieder eingeschaltet.

### Farbe
- Wenn ein Overlay keine eigene Farbe hat, dann erscheint es in der gemeinsamen Overlay-Farbe, voreingestellt Violett.
- Wenn der Benutzer in den Einstellungen die gemeinsame Overlay-Farbe ändert, dann erscheinen alle Overlays ohne eigene Farbe in der neuen Farbe.
- Wenn der Benutzer einem Overlay eine eigene Farbe gibt, dann erscheint es in dieser Farbe, unabhängig von der gemeinsamen Farbe.
- Wenn der Benutzer die eigene Farbe eines Overlays zurücksetzt, dann erscheint es wieder in der gemeinsamen Farbe.
