# 0008 · Kontrastfarben der Oberfläche

## Entscheidung
Auf dem Fluent-Design (ADR 0006) setzt AndyTours app-weit in `App.xaml` eigene Farben für mehr Kontrast. Sie gelten für alle Fenster, auch künftige:
- Karteikarten: Die Seite ist hellgrau (`#F0F2F5`) und rundum grau gerahmt (`#8F98A3`). Der gewählte Reiter hat die Farbe der Seite, denselben Rahmen und eine halbfette Beschriftung und bildet mit der Seite eine Fläche. Nicht gewählte Reiter sind dunkler grau (`#DCE1E7`), beim Darüberfahren noch etwas dunkler (`#CDD4DC`).
- Hauptmenüpunkte („Datei“, „Einheit“ …) haben die Farben der nicht gewählten Reiter. Hervorgehobene Menüpunkte, auch in aufklappenden Menüs und Kontextmenüs, sind dunkler grau (`#CDD4DC`).
- Eingabe-, Auswahl- und Datumsfelder sind deckend weiß. Listen sind weiß mit feinem Rahmen.

Der Reiter bekommt dafür eine eigene, schlichte Vorlage in Fluent-Form (Rahmen oben und seitlich, oben abgerundet). Alles andere ändert nur Eigenschaften und Farbressourcen der Fluent-Stile.

## Warum
Im Fluent-Design sind Reiter und Hauptmenüpunkte durchsichtig und ohne Rahmen, sodass sie vor dem Hintergrund kaum erkennbar sind. Eingabefelder sind halb durchsichtig weiß und Listen grau, auf grauem Grund wirken sie selbst grau und blass. Der Benutzer wollte eine graue Seite mit weißen Feldern und Listen darin. Den gewählten Reiter färbt die Fluent-Vorlage über eine Zustandsanimation fest weiß; das lässt sich weder über Eigenschaften noch über Ressourcen ändern, deshalb die eigene Vorlage.

## Alternativen
- Fluent unverändert lassen: keine eigenen Farben, aber Reiter und Menüpunkte bleiben kaum erkennbar.
- Weiße Seite mit weißem Reiter: ausprobiert und verworfen, weil Seite, Reiter und Felder ineinander verschwimmen und die graue Liste blass wirkt.
- Gewählten Reiter in Fluent-Weiß auf grauer Seite lassen: ohne eigene Vorlage, aber Reiter und Seite bilden keine Fläche.
