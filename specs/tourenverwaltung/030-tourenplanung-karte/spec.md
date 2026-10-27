# 030 – Tourenplanung auf der Karte

## Was und warum
Der Benutzer plant eine neue Radtour, indem er Start und Ziel in eine Karte klickt und die Streckenführung mit
Zwischenpunkten anpasst. Die Strecke folgt dabei automatisch den Wegen, die zum gewählten Fahrprofil passen.
Länge und Höhenmeter erscheinen sofort. An jeder Abzweigung, an der abgebogen werden muss, entsteht
automatisch ein Abbiegepunkt mit der Richtung. Die fertige Planung wird als
GPX-Datei gespeichert und lässt sich später wieder öffnen und weiterbearbeiten. Damit ersetzt AndyTours
die Tourenplanung von BikeXperience und schafft die Grundlage, um geplante Strecken später auf den Garmin
Edge 1040 zu übertragen.

## Kriterien

### Planen
- Wenn der Benutzer im Menü „Tourenplanung → Tour planen…“ wählt, dann öffnet sich ein Fenster mit einer Karte.
- Wenn das Planungsfenster öffnet, dann zeigt die Karte den zuletzt in der Planung gezeigten Ausschnitt; beim ersten Mal zeigt sie das Gebiet der zuletzt gefahrenen Einheit.
- Wenn der Benutzer in die Karte neben die Strecke klickt, dann wird dort ein Wegpunkt gesetzt und als neuer letzter Punkt an die Planung angehängt.
- Wenn die Planung mindestens zwei Wegpunkte hat, dann verbindet eine Linie die Wegpunkte in ihrer Reihenfolge und folgt dabei den Wegen, die das gewählte Fahrprofil vorsieht.

### Fahrprofil
- Wenn der Benutzer das Fahrprofil wählt, dann stehen „Rennrad (nur Straße)“, „Trekking/Tourenrad“ und „Mountainbike (bevorzugt offroad)“ zur Auswahl.
- Wenn das Profil „Rennrad (nur Straße)“ gewählt ist, dann führt die Strecke nur über asphaltierte Straßen und Radwege, soweit möglich.
- Wenn das Profil „Trekking/Tourenrad“ gewählt ist, dann führt die Strecke über Straßen, Radwege und gut befahrbare Schotterwege.
- Wenn das Profil „Mountainbike (bevorzugt offroad)“ gewählt ist, dann bevorzugt die Strecke Feld-, Wald- und Schotterwege.
- Wenn der Benutzer das Fahrprofil während der Planung wechselt, dann wird die ganze Strecke mit dem neuen Profil neu ermittelt; die Wegpunkte bleiben an ihrer Stelle.
- Wenn das Planungsfenster öffnet, dann ist das zuletzt gewählte Fahrprofil eingestellt, beim ersten Mal „Trekking/Tourenrad“.
- Wenn der Benutzer eine gespeicherte Planung öffnet, dann ist das Fahrprofil eingestellt, mit dem sie gespeichert wurde.

### Strecke
- Wenn der Benutzer die Karte mit gedrückter Maustaste verschiebt, dann wird kein Wegpunkt gesetzt.
- Wenn die Verbindung über Straßen für ein Teilstück nicht ermittelt werden kann (z. B. ohne Internet), dann wird dieses Teilstück als gerade Linie gezeichnet und ein Hinweis erscheint.
- Wenn sich die Strecke ändert, dann zeigt das Fenster sofort die Länge in km sowie die Höhenmeter bergauf und bergab an.

### Wegpunkte ändern
- Wenn der Benutzer die Linie zwischen zwei Wegpunkten an einer Stelle mit der Maus wegzieht, dann entsteht dort ein neuer Wegpunkt zwischen diesen beiden, und die Strecke wird über ihn neu ermittelt.
- Wenn der Benutzer einen Wegpunkt mit der Maus zieht, dann wandert der Punkt mit und die angrenzenden Teilstücke werden neu ermittelt.
- Wenn der Benutzer mit der rechten Maustaste auf einen Wegpunkt klickt, dann wird dieser Punkt entfernt und seine Nachbarn werden mit dem gewählten Fahrprofil neu verbunden.
- Wenn der Benutzer „Letzten Punkt entfernen“ wählt, dann verschwindet der zuletzt gesetzte Wegpunkt samt seinem Teilstück.
- Wenn der Benutzer „Alles löschen“ wählt und die Rückfrage bestätigt, dann ist die Planung leer.

### Abbiegepunkte
- Wenn die Strecke an einer Abzweigung abbiegt, dann entsteht dort automatisch ein Abbiegepunkt mit der Richtung (z. B. „links“, „rechts“, „leicht rechts“, „Kreisverkehr 2. Ausfahrt“); Straßennamen werden nicht angegeben.
- Wenn die Strecke an einer Abzweigung geradeaus weitergeht, dann entsteht dort kein Abbiegepunkt.
- Wenn sich die Strecke ändert, dann werden die Abbiegepunkte sofort neu ermittelt und als kleine Markierungen auf der Karte angezeigt.
- Wenn ein Teilstück als gerade Linie gezeichnet ist, dann enthält es keine Abbiegepunkte.
- Wenn der Benutzer die Planung speichert, dann enthält die Datei auch alle Abbiegepunkte mit ihrer Richtung.

### Speichern und öffnen
- Wenn der Benutzer „Speichern…“ wählt, dann kann er Ordner und Dateinamen wählen; vorgeschlagen werden der zuletzt verwendete Ordner und der Name „Planung JJJJ-MM-TT“.
- Wenn der Benutzer die Datei speichert, dann ist sie eine GPX-Datei mit der geplanten Strecke, die andere Programme öffnen können.
- Wenn die Planung weniger als zwei Wegpunkte hat, dann lässt sie sich nicht speichern.
- Wenn die gewählte Datei schon existiert, dann fragt die App, ob sie überschrieben werden soll.
- Wenn die Datei nicht geschrieben werden kann, dann erscheint eine Fehlermeldung und die Planung bleibt erhalten.
- Wenn der Benutzer „Öffnen…“ wählt und eine von AndyTours gespeicherte Planung auswählt, dann erscheinen ihre Wegpunkte und ihre Strecke wieder und lassen sich weiterbearbeiten.
- Wenn die gewählte Datei keine von AndyTours gespeicherte Planung ist oder nicht gelesen werden kann, dann erscheint ein Hinweis und die aktuelle Planung bleibt unverändert.
- Wenn der Benutzer eine geöffnete Planung erneut speichert, dann werden Ordner und Dateiname der geöffneten Datei vorgeschlagen.
- Wenn der Benutzer das Fenster schließt oder eine andere Planung öffnet und es ungespeicherte Änderungen gibt, dann fragt die App, ob die Änderungen verworfen werden sollen.
