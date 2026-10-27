# 0007 · Skalierung der Oberfläche

## Entscheidung
Die Oberfläche wird zentral über eine gemeinsame Vergrößerung des Fensterinhalts skaliert (eine `ScaleTransform` als `LayoutTransform`, gesteuert von `Oberflaechenskalierung`). Verbindlich für alle Fenster, auch künftige:
- Jedes Fenster meldet sich nach `InitializeComponent` bei `Oberflaechenskalierung` an; feste Fenstergrößen werden dabei mit der Stufe umgerechnet, das Hauptfenster behält seine gemerkte Größe und Position.
- Jeder Dialog hat unten eine feste Schaltflächenleiste und darüber einen rollbaren Inhalt und wird höchstens so groß wie der Arbeitsbereich des Bildschirms.
- Kartensteuerelemente (Mapsui) werden mit `Oberflaechenskalierung.Ausnehmen` ausgenommen; das Kartenbild bleibt in Originalgröße, alles um die Karte herum wächst mit.

Die Stufe gehört zum Datenbestand und steht in der Tabelle `Einstellung`. Die Entscheidung ergänzt [0006](0006-grundlage-der-oberflaeche.md).

## Warum
- Eine einzige Stelle vergrößert alle Fenster gleichmäßig – Schrift, Felder, Abstände, Tabellen und feste Breiten – und wechselt die Stufe sofort, ohne Neustart.
- Gespeicherte Spaltenbreiten, Fensterpositionen und Kartenausschnitte bleiben in Einheiten von 100 % und stimmen deshalb in jeder Stufe.
- Die feste Schaltflächenleiste mit rollbarem Inhalt hält Dialoge auch bei 200 % auf kleineren Bildschirmen bedienbar.
- Karten werden ausgenommen, weil Mapsui ein Bitmap zeichnet, das vergrößert grob und unscharf würde und weniger Kartenausschnitt zeigte; die Kacheln bringen ihre Beschriftung ohnehin in fester Größe mit.

## Alternativen
- Nur die Schriftgröße erhöhen: einfach, aber Felder, Abstände und feste Breiten wachsen nicht mit, Texte würden abgeschnitten.
- Die Windows-Skalierung nutzen: vorhanden, wirkt aber auf alle Programme statt nur auf AndyTours und lässt sich nicht aus der App heraus einstellen.
- Skalierung in jedem Fenster selbst per Bindung im XAML: kein Anmelden nötig, aber feste Fenstergrößen, aufklappende Elemente und die Kartenausnahme müssten in jedem Fenster einzeln gelöst werden.
- Karten mitvergrößern: einheitlicher, aber das Kartenbild wird unscharf und zeigt weniger Ausschnitt.
