# Plan: 016 – Bestleistungen

## Vorgehen
Die Ansicht „Aktuell“ bekommt ganz oben einen Bereich „Bestleistungen“ mit zwei Spalten: links die Jahresrekorde, rechts die Einzelrekorde. Darunter folgen wie bisher Überschrift, Kennzahlen und Diagramm des Jahresfortschritts.

- **Core:**
  - Die Auswahl der Übersicht (Aktivität der Werkzeugleiste, Kategorie der Filterleiste, alle Jahre) wird aus `ErmittleJahresfortschritt` in eine eigene Funktion `FiltereUebersicht` herausgelöst; Jahresfortschritt und Bestleistungen nutzen dieselbe.
  - `ErmittleBestleistungen` liefert die Rekorde als Daten:
    - Jahresrekorde aus `BerechneJahressummen`
    - Einzelrekorde aus den Einheiten
    - bei Gleichstand jeweils der frühere; fehlende Werte (0) zählen nicht
  - `BeschreibeBestleistungen` macht daraus Anzeigezeilen (Bezeichnung, Wert, Herkunft, „laufendes Jahr“).
  - Die Fahrzeit als „h:mm h“ wird in die Funktion `FormatiereStunden` verschoben, damit Jahresfortschritt und Bestleistungen sie gemeinsam nutzen.
- **App:**
  - `BestleistungenViewModel` hält die beiden Zeilenlisten und `HatEinheiten`.
  - Das Hauptfenster aktualisiert es am Ende von `AktualisiereSummen`, gleich neben dem Jahresfortschritt.
  - Die Ansicht „Aktuell“ erbt künftig das Hauptfenster-ViewModel und gibt ihren Teilen `Bestleistungen` bzw. `Jahresfortschritt` als Datenquelle.

## Dateien
- `src/AndyTours.Core/Modell/Jahresrekord.cs` (neu) — Wert (`Fortschrittswert`), Jahr, Summe
- `src/AndyTours.Core/Modell/Einzelrekordart.cs` (neu) — Aufzählung Distanz, Hoehenmeter, Fahrzeit, Durchschnittsgeschwindigkeit, Hoechstgeschwindigkeit, HoechsterPunkt (in dieser Anzeigereihenfolge)
- `src/AndyTours.Core/Modell/Einzelrekord.cs` (neu) — Art, Wert, Einheit
- `src/AndyTours.Core/Modell/Bestleistungen.cs` (neu) — Liste der Jahresrekorde, Liste der Einzelrekorde
- `src/AndyTours.Core/Modell/Bestleistungszeile.cs` (neu) — Bezeichnung, Wert, Herkunft, IstLaufendesJahr, EinheitId (nur bei Einzelrekorden)
- `src/AndyTours.Core/Berechnungen/FiltereUebersicht.cs` (neu) — `Ausfuehren(bestand, aktivitaetId, kategorie)`: `FiltereAuswahl` ohne Jahr, dann `FiltereEinheiten` nur mit der Kategorie
- `src/AndyTours.Core/Berechnungen/ErmittleBestleistungen.cs` (neu) — `Ausfuehren(bestand, aktivitaetId, kategorie)`
  - Jahresrekorde für Kilometer, Höhenmeter, Anzahl, Fahrzeit: höchste Jahressumme größer 0, bei Gleichstand das frühere Jahr
  - Einzelrekorde: höchster Wert größer 0, bei Gleichstand die frühere Einheit (Datum, dann Startzeit, dann Nummer)
  - Ø-Geschwindigkeit über `BerechneDurchschnittsgeschwindigkeit`, höchster Punkt aus der maximalen Höhe
  - Arten ohne Wert fehlen in der Liste
- `src/AndyTours.Core/Berechnungen/BeschreibeBestleistungen.cs` (neu) — `Ausfuehren(bestleistungen, heute)` liefert zwei Zeilenlisten
  - Jahresrekorde, z. B. „Kilometer: 8.123,4 km (2019)“, als Bezeichnung „Kilometer“, Wert „8.123,4 km“, Herkunft „2019“
  - Einzelrekorde, z. B. „Längste Tour: 212,35 km – 14.06.2021 Alpenrunde“
  - Formate wie in der Tabelle: Distanz N2, Fahrzeit `FormatiereDauer`, km/h N1, hm und Höhe N0; Jahressummen wie im Jahresfortschritt
- `src/AndyTours.Core/Berechnungen/FormatiereStunden.cs` (neu) — Stunden als „h:mm h“ (bisher privat in `BeschreibeJahresfortschritt`)
- `src/AndyTours.Core/Berechnungen/ErmittleJahresfortschritt.cs` (ändern) — Auswahl über `FiltereUebersicht`
- `src/AndyTours.Core/Berechnungen/BeschreibeJahresfortschritt.cs` (ändern) — `FormatiereStunden` statt der privaten Methode
- `src/AndyTours.App/ViewModels/BestleistungenViewModel.cs` (neu) — `Jahresrekorde`, `Einzelrekorde` (Zeilenlisten), `HatEinheiten`, `Aktualisiere(bestand, aktivitaetId, kategorie, heute)`
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Einheiten.cs` (ändern) — Befehl `EinheitOeffnen(id)`: öffnet das Bearbeitungsfenster der Einheit mit dieser Nummer
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — Eigenschaft `Bestleistungen`; am Ende von `AktualisiereSummen` aktualisieren
- `src/AndyTours.App/Views/AktuellAnsicht.xaml` (ändern) — oben der Bereich „Bestleistungen“ (zwei Spalten, Zeilen des laufenden Jahres fett und farbig, Doppelklick auf einen Einzelrekord → `EinheitOeffnen`), darunter der bisherige Inhalt mit der Datenquelle `Jahresfortschritt`
- `src/AndyTours.App/Views/AktuellAnsicht.xaml.cs` (ändern) — der Klick holt das ViewModel aus der Datenquelle des Diagramms
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — `AktuellAnsicht` ohne eigene Datenquelle (erbt das Hauptfenster-ViewModel)
- `tests/AndyTours.Tests/Berechnungen/ErmittleBestleistungenTests.cs` (neu) — Jahres- und Einzelrekorde, Gleichstand, fehlende Werte, Aktivität/Kategorie, leere Auswahl
- `tests/AndyTours.Tests/Berechnungen/BeschreibeBestleistungenTests.cs` (neu) — Texte, Hervorhebung des laufenden Jahres
- `tests/AndyTours.Tests/Berechnungen/FormatiereStundenTests.cs` (neu) — „h:mm h“, Rundung auf Minuten, Tausenderpunkt

## Abdeckung der Kriterien
- Wenn der Benutzer die Karteikarte „Aktuell“ öffnet, dann steht über dem Diagramm eine Liste der Bestleistungen.
  - Umgesetzt: `AktuellAnsicht.xaml` (Bereich oben)
  - Geprüft: manuell – Karteikarte öffnen, Liste steht über dem Diagramm.
- Wenn es Jahresrekorde gibt, dann nennt die Liste für Kilometer, Höhenmeter, Anzahl Einheiten und Fahrzeit jeweils den höchsten Jahreswert mit seinem Jahr, z. B. „Kilometer: 8.123,4 km (2019)“.
  - Umgesetzt: `ErmittleBestleistungen`, `BeschreibeBestleistungen`
  - Geprüft: `ErmittleBestleistungenTests`, `BeschreibeBestleistungenTests`
- Wenn es Einzelrekorde gibt, dann nennt die Liste die längste Tour, die meisten Höhenmeter, die längste Fahrzeit, die höchste Ø-Geschwindigkeit, die höchste Höchstgeschwindigkeit und den höchsten Punkt, jeweils mit Wert, Datum und Name der Einheit.
  - Umgesetzt: `ErmittleBestleistungen`, `BeschreibeBestleistungen`
  - Geprüft: `ErmittleBestleistungenTests`, `BeschreibeBestleistungenTests`; manuell – Werte mit den roten Bestwerten der Tabelle bei „alle Jahre“ vergleichen.
- Wenn das laufende Jahr einen Rekord hält, dann ist dieser hervorgehoben.
  - Umgesetzt: `BeschreibeBestleistungen` (`IstLaufendesJahr`), `AktuellAnsicht.xaml` (Darstellung)
  - Geprüft: `BeschreibeBestleistungenTests`; manuell – Rekord aus 2026 ist fett und farbig.
- Wenn der Benutzer auf einen Einzelrekord doppelklickt, dann öffnet sich die zugehörige Einheit zum Bearbeiten.
  - Umgesetzt: `BeschreibeBestleistungen` (`EinheitId`), `HauptfensterViewModel.EinheitOeffnen`, `AktuellAnsicht.xaml` (Doppelklick)
  - Geprüft: `BeschreibeBestleistungenTests` (Nummer der Einheit); manuell – Doppelklick auf „Längste Tour“ öffnet diese Einheit, Ändern und Speichern aktualisiert die Liste.
- Wenn in der Werkzeugleiste eine Aktivität oder in der Filterleiste eine Kategorie gewählt ist, dann gelten die Bestleistungen nur für diese Einheiten, wie beim Diagramm; die Jahresauswahl und die übrigen Filter ändern die Bestleistungen nicht.
  - Umgesetzt: `FiltereUebersicht`, Aufruf in `AktualisiereSummen`
  - Geprüft: `ErmittleBestleistungenTests`; manuell – Aktivität, Kategorie, Jahr und Equipment wechseln.
- Wenn mehrere Jahre oder Einheiten denselben Rekordwert haben, dann hält der frühere den Rekord.
  - Umgesetzt: `ErmittleBestleistungen`
  - Geprüft: `ErmittleBestleistungenTests`
- Wenn bei einer Einheit ein Wert fehlt (z. B. keine Höchstgeschwindigkeit erfasst), dann zählt er nicht als Rekord; hat keine Einheit diesen Wert, entfällt die Zeile.
  - Umgesetzt: `ErmittleBestleistungen`
  - Geprüft: `ErmittleBestleistungenTests`
- Wenn die Auswahl keine Einheiten enthält, dann entfällt die Liste.
  - Umgesetzt: `BestleistungenViewModel.HatEinheiten`, `AktuellAnsicht.xaml`
  - Geprüft: `ErmittleBestleistungenTests` (leere Auswahl); manuell – Aktivität ohne Einheiten wählen.
- Wenn Einheiten angelegt, geändert oder gelöscht werden oder sich die Aktivität der Werkzeugleiste oder die Kategorie der Filterleiste ändert, dann zeigt die Liste sofort den neuen Stand.
  - Umgesetzt: Aufruf am Ende von `AktualisiereSummen`
  - Geprüft: manuell – längere Tour anlegen, wieder löschen.

## Entscheidungen
- **Anordnung:** Die Bestleistungen stehen ganz oben; Überschrift und Kennzahlen bleiben direkt beim Diagramm, weil sie sich mit dem Klick ins Diagramm ändern.
- **Hervorhebung:** Zeilen mit einem Rekord des laufenden Jahres werden fett in der Linienfarbe des Diagramms gezeigt. Rot ist in der Tabelle schon für die Bestwerte der Auswahl vergeben.
- **Einzelrekorde:** Die Formate entsprechen der Tabelle, damit die Werte wiedererkennbar sind.
- **Gemeinsame Auswahl:** Die Auswahl steht in `FiltereUebersicht`, damit Diagramm und Bestleistungen sicher dieselben Einheiten verwenden.
