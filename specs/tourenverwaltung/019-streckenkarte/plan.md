# Plan: 019 – Streckenkarte

## Vorgehen
Der Verlauf aus 018 wird um die Position je Messpunkt erweitert: `LeseFitVerlauf` liest dafür Breite und Länge aus der FIT-Datei. Garmin speichert sie in Semicircles, umgerechnet werden sie in Grad. Messpunkte ohne Position, z. B. im Tunnel, behalten eine leere Position.

Eine Fachfunktion im Core macht aus dem Verlauf die Strecke:
- die Punkte mit Position in ihrer Reihenfolge, sodass Lücken einfach überbrückt werden,
- dazu das umschließende Rechteck für „Ganze Strecke“.

Das Tourdetail bekommt die Karteikarte „Karte“. Ein eigenes ViewModel hält:
- die Strecke,
- die gewählte Kartenart (OpenStreetMap oder OpenTopoMap),
- den Lizenzhinweis zur Kartenart,
- den Hinweis ohne GPS-Daten,
- den Hinweis, wenn Kartenausschnitte nicht geladen werden konnten.

Die Kartendarstellung übernimmt Mapsui (ADR 0003). Die Bibliothek lädt die Kartenausschnitte, legt sie im Zwischenspeicher ab und zeichnet Linie und Start-/Zielzeichen. Der Zwischenspeicher liegt unter `%LOCALAPPDATA%\AndyTours\Karten`, nicht im Datenordner: Kartenausschnitte sind keine Tourdaten und sollen weder auf ein NAS noch in die Sicherung.

## Dateien
- `src/AndyTours.Core/Modell/Verlaufspunkt.cs` (ändern) — Breite und Länge in Grad, optional
- `src/AndyTours.Core/Modell/Geoposition.cs` (neu) — Breite und Länge in Grad
- `src/AndyTours.Core/Modell/Strecke.cs` (neu) — Punkte der Strecke sowie Süd-West- und Nord-Ost-Ecke des umschließenden Rechtecks
- `src/AndyTours.Core/Modell/Kartenart.cs` (neu) — OpenStreetMap, OpenTopoMap
- `src/AndyTours.Core/Berechnungen/ErmittleStrecke.cs` (neu) — Strecke aus dem Verlauf; `null` ohne GPS-Daten
- `src/AndyTours.Import/Garmin/LeseFitVerlauf.cs` (ändern) — Position je Messpunkt lesen und in Grad umrechnen
- `src/AndyTours.App/AndyTours.App.csproj` (ändern) — Paket `Mapsui.Wpf` 5.1.0; Ziel `net10.0-windows10.0.19041.0`, weil die mitgelieferte SkiaSharp-WPF-Ansicht nur dafür eine .NET-Fassung hat (sonst .NET-Framework-Fassung mit Warnung NU1701)
- `src/AndyTours.Daten/Datenordner/FindeProjektordner.cs` (ändern) — Beispielpfad im Kommentar an das neue Ziel angepasst
- `src/AndyTours.App/Funktionen/Kartenabruf.cs` (neu) — gemeinsame Internetverbindung der Kartenebenen: Kennung „AndyTours/Version“ und Meldung von Ladefehlern, weil Mapsui diese verschluckt
- `src/AndyTours.App/Funktionen/Programmpfade.cs` (ändern) — Ordner des Kartenspeichers `%LOCALAPPDATA%\AndyTours\Karten`
- `src/AndyTours.App/ViewModels/StreckenkarteViewModel.cs` (neu) — Strecke, Kartenart, Lizenzhinweis, Hinweise ohne GPS bzw. ohne geladene Karte, Befehl „Ganze Strecke“
- `src/AndyTours.App/ViewModels/TourdetailViewModel.cs` (ändern) — erzeugt das `StreckenkarteViewModel` aus dem Verlauf
- `src/AndyTours.App/Views/TourdetailFenster.xaml` (ändern) — Karteikarte „Karte“: Umschalter Kartenart, „Ganze Strecke“, Karte, Lizenzhinweis, Hinweise
- `src/AndyTours.App/Views/TourdetailFenster.xaml.cs` (ändern) — Mapsui-Karte aufbauen: Kartenebene je Kartenart mit Kennung „AndyTours“ und Zwischenspeicher, Ebenen für Linie und Start-/Zielzeichen, Ausschnitt „Ganze Strecke“, Ladefehler an das ViewModel melden; im Code-Behind, weil Mapsui seine Ebenen nicht über Bindungen bekommt
- `tests/AndyTours.Tests/Garmin/FitTestdatei.cs` (ändern) — Positionen an den Messpunkten: erster und dritter mit Position, zweiter ohne (Tunnel); Wahl „ohne GPS“
- `tests/AndyTours.Tests/Garmin/LeseFitVerlaufTests.cs` (ändern) — Positionen in Grad, Punkt ohne Position, Datei ohne GPS
- `tests/AndyTours.Tests/Berechnungen/ErmittleStreckeTests.cs` (neu)

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Anzeigen
- **Kriterium:** Wenn das Tourdetail einer Einheit geöffnet ist, dann gibt es neben „Höhenprofil“ die Karteikarte „Karte“.
  - Umsetzung: `TourdetailFenster.xaml`
  - Manuell: Tourdetail öffnen; zwei Karteikarten.
- **Kriterium:** Wenn der Benutzer die Karteikarte „Karte“ wählt, dann zeigt sie die Strecke der Aufzeichnung als einfarbige Linie auf einer Karte, so vergrößert, dass die ganze Strecke zu sehen ist.
  - Umsetzung: `LeseFitVerlauf`, `ErmittleStrecke`, `StreckenkarteViewModel`, `TourdetailFenster`
  - Prüfung: `LeseFitVerlaufTests`, `ErmittleStreckeTests` (Punkte und umschließendes Rechteck)
  - Manuell: Karteikarte wählen; die ganze Strecke ist zu sehen und stimmt mit der bekannten Tour überein.
- **Kriterium:** Wenn die Strecke angezeigt wird, dann sind Start und Ziel jeweils mit einem eigenen Zeichen markiert.
  - Umsetzung: `StreckenkarteViewModel` (erster und letzter Punkt), `TourdetailFenster`
  - Prüfung: `ErmittleStreckeTests` (Reihenfolge der Punkte)
  - Manuell: Start- und Zielzeichen liegen am Anfang bzw. Ende der Linie.
- **Kriterium:** Wenn der Benutzer zwischen „OpenStreetMap“ und „OpenTopoMap“ umschaltet, dann erscheint die gewählte Karte mit derselben Strecke und demselben Ausschnitt.
  - Umsetzung: `StreckenkarteViewModel` (Kartenart), `TourdetailFenster`
  - Manuell: vergrößern, dann umschalten; Ausschnitt und Strecke bleiben.
- **Kriterium:** Wenn der Benutzer das Mausrad dreht, dann wird die Karte an der Mausposition vergrößert oder verkleinert; Ziehen mit gedrückter linker Maustaste verschiebt sie.
  - Umsetzung: Mapsui (eingebaute Maussteuerung), `TourdetailFenster`
  - Manuell: Mausrad und Ziehen.
- **Kriterium:** Wenn der Benutzer auf „Ganze Strecke“ klickt, dann zeigt die Karte wieder die ganze Strecke.
  - Umsetzung: `StreckenkarteViewModel`, `TourdetailFenster`
  - Prüfung: `ErmittleStreckeTests` (umschließendes Rechteck)
  - Manuell: verschieben und vergrößern, dann „Ganze Strecke“.
- **Kriterium:** Wenn die Karte angezeigt wird, dann steht in einer Ecke der Hinweis „© OpenStreetMap-Mitwirkende“, bei OpenTopoMap ergänzt um „Kartendarstellung: © OpenTopoMap (CC-BY-SA)“.
  - Umsetzung: `StreckenkarteViewModel` (Lizenzhinweis je Kartenart), `TourdetailFenster`
  - Manuell: Hinweis bei beiden Kartenarten prüfen.

### Ohne Internet
- **Kriterium:** Wenn ein Kartenausschnitt einmal geladen wurde, dann erscheint er später auch ohne Internetverbindung.
  - Umsetzung: Zwischenspeicher von Mapsui im Ordner aus `Programmpfade`
  - Manuell: Tour mit Internet ansehen, Netzwerk trennen, App neu starten, dieselbe Tour öffnen; die Karte erscheint.
- **Kriterium:** Wenn keine Internetverbindung besteht und ein Ausschnitt noch nie geladen wurde, dann wird die Strecke trotzdem gezeichnet, und es erscheint der Hinweis „Die Karte konnte nicht geladen werden. Besteht eine Internetverbindung?“
  - Umsetzung: `StreckenkarteViewModel` (Hinweis), `Kartenabruf` (Ladefehler erkennen), `TourdetailFenster` (Hinweis einblenden)
  - Manuell: ohne Netzwerk eine noch nie angesehene Gegend öffnen.

### Grenz- und Fehlerfälle
- **Kriterium:** Wenn die Aufzeichnung keine GPS-Daten enthält (z. B. eine Fahrt auf der Rolle), dann erscheint in der Karteikarte „Die Aufzeichnung enthält keine GPS-Daten.“
  - Umsetzung: `ErmittleStrecke` (`null` ohne Positionen), `StreckenkarteViewModel`, `TourdetailFenster`
  - Prüfung: `ErmittleStreckeTests`, `LeseFitVerlaufTests` (Datei ohne GPS)
- **Kriterium:** Wenn einzelne Messpunkte keine Position haben (z. B. im Tunnel), dann verbindet die Linie die Punkte davor und danach.
  - Umsetzung: `LeseFitVerlauf` (Punkt ohne Position bleibt leer), `ErmittleStrecke` (lässt ihn aus)
  - Prüfung: `LeseFitVerlaufTests`, `ErmittleStreckeTests`

## Entscheidungen
- Die Positionen kommen aus dem Verlauf aus 018. Die Aufzeichnung wird beim Öffnen des Tourdetails nur einmal gelesen und für beide Karteikarten verwendet.
- Der Kartenspeicher liegt unter `%LOCALAPPDATA%\AndyTours\Karten` und nicht im Datenordner, damit Kartenausschnitte weder in die Sicherung noch auf ein NAS kommen.
- Der Lizenzhinweis steht im ViewModel je Kartenart, nicht in der Bibliothek. So ist er unabhängig davon, ob die Bibliothek selbst einen anzeigt.

## Offene Entscheidungen
- Welche Kartenbibliothek? (WPF-Karte mit OpenStreetMap- und OpenTopoMap-Kacheln, Zwischenspeicher auf dem Rechner, Linie und Zeichen auf der Karte, Meldung bei Ladefehlern) → decided in docs/adr/0003-kartenbibliothek.md

## Risiken
- Die Nutzungsregeln von OpenStreetMap und OpenTopoMap verlangen eine eindeutige Kennung der App bei jeder Anfrage und verbieten das massenhafte Herunterladen. Geladen werden deshalb nur die sichtbaren Ausschnitte, mit Zwischenspeicher und Kennung „AndyTours“.
- Mapsui meldet Ladefehler nicht (kein Ereignis); sie werden deshalb in `Kartenabruf` an der Internetverbindung erkannt.
- Lange Touren haben über 20.000 Punkte. Die Linie wird bei Bedarf für die Darstellung ausgedünnt; Start und Ziel bleiben exakt.
