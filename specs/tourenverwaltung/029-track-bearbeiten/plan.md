# Plan: 029 – Track bearbeiten

## Vorgehen
Grundlage sind die Messpunkte der Aufzeichnung mit Position, also dieselben Punkte, aus denen `ErmittleStrecke` die Linie der Karte bildet. Die Schieberegler „Start“ und „Ziel“ wählen einen Punkt dieser Liste; daneben steht seine Distanz laut Aufzeichnung („12,4 km“). Das ViewModel hält „Start“ immer mindestens einen Punkt vor „Ziel“.

Zwei neue Fachfunktionen im Core:

- `BearbeiteTrack` liefert aus Verlauf, Start, Ziel, „Umkehren“ und Glättungsstufe die bearbeiteten Messpunkte. Sie schneidet das Stück zwischen Start und Ziel aus, glättet es gegebenenfalls und dreht zuletzt die Reihenfolge um, falls gewünscht.
- `GlaetteTrack` glättet die Positionen in zwei Schritten:
  1. Gleitender Mittelwert über benachbarte Punkte, „schwach“ über 5, „stark“ über 15. Damit verschwindet das Zittern.
  2. Vereinfachung nach Douglas-Peucker mit einer Toleranz von 3 m bzw. 10 m. Dabei fallen Punkte weg, die auf einer nahezu geraden Linie liegen.

  Start- und Endpunkt bleiben unverändert, die Höhe der verbleibenden Punkte bleibt erhalten.

Die Leiste kommt in einen neuen Teil `StreckenkarteViewModel.Bearbeiten.cs`. Sie ist nur bei einer einzelnen Etappe mit Strecke sichtbar (`KannBearbeiten`). Jede Änderung an Start, Ziel, „Umkehren“ oder „Glätten“ geht so durch:
- `BearbeiteTrack` berechnet die Strecke neu.
- Das ViewModel ersetzt `Strecken` und das umschließende Rechteck.
- Es aktualisiert „Länge: … km“.
- Es meldet die Änderung über das Ereignis `StreckeGeaendert`.

Das Code-Behind des Fensters zeichnet daraufhin Linie und Zeichen neu. Den Ausschnitt lässt es dabei unverändert, damit die Karte beim Ziehen nicht springt. Erst „Ganze Strecke“ zeigt wieder die ganze bearbeitete Strecke. „Zurücksetzen“ stellt alle vier Werte auf den Ausgangszustand.

Der GPX-Export nimmt bei einer bearbeiteten Strecke eine Etappe mit den bearbeiteten Messpunkten statt der Original-Etappe. `BildeGpxDateiname` erhält dafür den Schalter `bearbeitet` für den Zusatz „(bearbeitet)“. Einheit, Aufzeichnung und Höhenprofil kommen mit der Bearbeitung nicht in Berührung. Die Bearbeitung lebt nur im ViewModel des offenen Fensters.

## Dateien
- `src/AndyTours.Core/Modell/Glaettung.cs` (neu) — Stufen „aus“, „schwach“, „stark“
- `src/AndyTours.Core/Berechnungen/GlaetteTrack.cs` (neu) — Mittelwert und Vereinfachung der Positionen
- `src/AndyTours.Core/Berechnungen/BearbeiteTrack.cs` (neu) — kürzen, glätten, umkehren
- `src/AndyTours.Core/Berechnungen/BildeGpxDateiname.cs` (ändern) — Zusatz „(bearbeitet)“
- `src/AndyTours.App/ViewModels/StreckenkarteViewModel.cs` (ändern) — `Strecken` und Rechteck änderbar, Export der bearbeiteten Strecke
- `src/AndyTours.App/ViewModels/StreckenkarteViewModel.Bearbeiten.cs` (neu) — Leiste „Track bearbeiten“: Start, Ziel, Umkehren, Glätten, Länge, Zurücksetzen
- `src/AndyTours.App/Views/TourdetailFenster.xaml` (ändern) — Leiste unter der Karte
- `src/AndyTours.App/Views/TourdetailFenster.xaml.cs` (ändern) — Linie und Zeichen bei `StreckeGeaendert` neu zeichnen, Rechteck für „Ganze Strecke“ neu bestimmen
- `tests/AndyTours.Tests/Berechnungen/GlaetteTrackTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BearbeiteTrackTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BildeGpxDateinameTests.cs` (ändern) — Zusatz „(bearbeitet)“

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Bearbeiten im Tourdetail
- **Kriterium:** Wenn das Tourdetail einer einzelnen Einheit mit GPS-Daten geöffnet ist, dann gibt es auf der Karteikarte „Karte“ unter der Karte die Leiste „Track bearbeiten“ mit den Schiebereglern „Start“ und „Ziel“, „Umkehren“, „Glätten“ und „Zurücksetzen“.
  - Umsetzung: `TourdetailFenster.xaml`, `StreckenkarteViewModel.Bearbeiten` (`KannBearbeiten`)
  - Manuell: Tourdetail einer Tour mit GPS öffnen, Karteikarte „Karte“.
- **Kriterium:** Wenn ein Schieberegler „Start“ oder „Ziel“ verschoben wird, dann steht daneben seine Stelle in km ab dem Start der Aufzeichnung, die Karte zeigt nur noch das Stück dazwischen, und die Zeichen für Start und Ziel wandern mit.
  - Umsetzung: `BearbeiteTrack` (Ausschnitt), `StreckenkarteViewModel.Bearbeiten` (km-Text, `StreckeGeaendert`), `TourdetailFenster.xaml.cs` (neu zeichnen)
  - Prüfung: `BearbeiteTrackTests` (Ausschnitt zwischen Start und Ziel)
  - Manuell: beide Regler ziehen; Linie und Zeichen folgen, die km-Angaben ändern sich.
- **Kriterium:** Wenn „Umkehren“ eingeschaltet wird, dann tauschen Start und Ziel ihre Rollen; die Strecke läuft rückwärts.
  - Umsetzung: `BearbeiteTrack`
  - Prüfung: `BearbeiteTrackTests`
  - Manuell: „Umkehren“ einschalten; grüner Punkt und rotes Quadrat tauschen die Enden.
- **Kriterium:** Wenn bei „Glätten“ „schwach“ oder „stark“ gewählt wird, dann zeigt die Karte die entsprechend geglättete Linie – das GPS-Zittern verschwindet, „stark“ glättet mehr als „schwach“ – und die Strecke hat weniger Punkte; „aus“ zeigt die ungeglättete Linie.
  - Umsetzung: `GlaetteTrack`, aufgerufen in `BearbeiteTrack`
  - Prüfung: `GlaetteTrackTests` (Zickzack wird ruhiger, „stark“ ruhiger und mit weniger Punkten als „schwach“, „aus“ unverändert, Start und Ende fest)
  - Manuell: an einer Stelle mit Zittern stark vergrößern und zwischen den Stufen wechseln.
- **Kriterium:** Wenn die Strecke angezeigt wird, dann steht in der Leiste die Länge der bearbeiteten Strecke, z. B. „Länge: 42,3 km“.
  - Umsetzung: `StreckenkarteViewModel.Bearbeiten` (Distanz des Ziel- minus Startpunkts laut Aufzeichnung)
  - Manuell: ungekürzt entspricht die Länge der Distanz der Einheit; nach dem Kürzen der Differenz der beiden km-Angaben.
- **Kriterium:** Wenn „Zurücksetzen“ gewählt wird, dann stehen Start und Ziel wieder an Anfang und Ende, „Umkehren“ ist aus, „Glätten“ steht auf „aus“, und die Karte zeigt die ganze, unbearbeitete Strecke.
  - Umsetzung: `StreckenkarteViewModel.Bearbeiten` (Befehl „Zurücksetzen“)
  - Manuell: bearbeiten, dann „Zurücksetzen“.

### Speichern
- **Kriterium:** Wenn nach dem Bearbeiten „Als GPX exportieren…“ gewählt wird, dann enthält die Datei die bearbeitete Strecke – gekürzt, ggf. umgekehrt und geglättet –, weiter mit der Höhe je Punkt.
  - Umsetzung: `StreckenkarteViewModel` (Export mit den Messpunkten aus `BearbeiteTrack`), `ErstelleGpx` unverändert
  - Prüfung: `BearbeiteTrackTests` (Höhe bleibt je Punkt erhalten)
  - Manuell: gekürzt und umgekehrt exportieren, im Viewer prüfen.
- **Kriterium:** Wenn die Strecke bearbeitet wurde, dann schlägt der Speichern-Dialog den Dateinamen mit dem Zusatz „(bearbeitet)“ vor, z. B. „2026-05-01 Bründlingalm (bearbeitet).gpx“; unbearbeitet bleibt es beim bisherigen Namen.
  - Umsetzung: `BildeGpxDateiname` (Schalter `bearbeitet`), `StreckenkarteViewModel`
  - Prüfung: `BildeGpxDateinameTests`
- **Kriterium:** Wenn die Strecke bearbeitet wurde, dann bleiben Einheit, Aufzeichnung, Distanz, Höhenmeter und Höhenprofil unverändert.
  - Umsetzung: Bearbeitung nur im `StreckenkarteViewModel`; `BearbeiteTrack` erzeugt neue Messpunkte und verändert den Verlauf nicht
  - Prüfung: `BearbeiteTrackTests` (Eingabeverlauf unverändert)
  - Manuell: nach dem Bearbeiten Höhenprofil und Einheitentabelle ansehen.
- **Kriterium:** Wenn das Tourdetail geschlossen und wieder geöffnet wird, dann ist die Bearbeitung verworfen.
  - Umsetzung: jedes Öffnen erzeugt ein neues ViewModel
  - Manuell: bearbeiten, schließen, wieder öffnen.

### Grenzfälle
- **Kriterium:** Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann fehlt die Leiste „Track bearbeiten“.
  - Umsetzung: `KannBearbeiten` (nur eine Etappe)
  - Manuell: Mehrtagestour öffnen.
- **Kriterium:** Wenn die Aufzeichnung keine GPS-Daten enthält, dann fehlt die Leiste.
  - Umsetzung: `KannBearbeiten` (nur mit Strecke); die Leiste liegt im Bereich, der ohne Strecke ausgeblendet ist
  - Manuell: Rollenfahrt öffnen.
- **Kriterium:** Wenn „Start“ und „Ziel“ zusammengeschoben werden, dann bleibt immer ein Stück von mindestens zwei Punkten übrig; „Start“ kann nicht hinter „Ziel“ gezogen werden und umgekehrt.
  - Umsetzung: `StreckenkarteViewModel.Bearbeiten` (Start höchstens Ziel − 1, Ziel mindestens Start + 1); `BearbeiteTrack` prüft die Grenzen
  - Prüfung: `BearbeiteTrackTests` (zwei benachbarte Punkte ergeben zwei Punkte; ungültige Grenzen werfen eine Ausnahme)
  - Manuell: beide Regler gegeneinander ziehen.
- **Kriterium:** Wenn „Ganze Strecke“ gewählt wird, dann zeigt die Karte die ganze bearbeitete Strecke.
  - Umsetzung: `TourdetailFenster.xaml.cs` (Rechteck bei `StreckeGeaendert` neu bestimmen)
  - Manuell: stark kürzen, dann „Ganze Strecke“.

## Entscheidungen
- Die Länge ist die Distanz laut Aufzeichnung zwischen Start- und Zielpunkt, nicht die Länge der geglätteten Linie. So passt sie zu den km-Angaben der Regler und ungekürzt zur Distanz der Einheit; Glätten ändert sie nicht.
- Die Regler arbeiten mit den Punkten der Aufzeichnung, die eine Position haben. Ein Regelschritt ist ein Punkt, bei einer Aufzeichnung je Sekunde also wenige Meter.
- Beim Ziehen bleibt der Kartenausschnitt stehen. Erst „Ganze Strecke“ passt ihn an.
- Geglättet wird das bereits gekürzte Stück, umgekehrt wird zuletzt. Mittelwert und Vereinfachung rechnen in Metern auf einer örtlichen Ebene. Für Radtouren ist das genau genug.

## Risiken
- Lange Aufzeichnungen haben viele Punkte, z. B. 30 000 bei acht Stunden. Wenn das Neuzeichnen beim Ziehen ruckelt, wird erst beim Loslassen des Reglers neu berechnet.
