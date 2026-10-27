# Plan: 020 – Mehrtagestour

## Vorgehen
Das Tourdetail wird von einer Einheit auf eine Liste von Etappen verallgemeinert. Eine einzelne Einheit ist der Sonderfall mit genau einer Etappe; Kopfzeile, Profil und Karte bleiben dafür unverändert.

Ablauf beim Befehl „Tourdetail…“ mit mehreren markierten Einheiten:
1. Die Einheiten werden nach Datum und Startzeit geordnet.
2. Für jede Einheit mit Aufzeichnung wird der Verlauf gelesen wie bisher. Fehlt eine Datei oder ist sie nicht lesbar, erscheint der bekannte Hinweis (alle betroffenen Dateien in einer Meldung). Die Etappe wird danach wie eine Etappe ohne Aufzeichnung behandelt.
3. Das Fenster öffnet sich mit allen Etappen.

Fachfunktionen im Core liefern:
- die Reihenfolge der Etappen,
- die Kopfzeile mit Zeitraum, Anzahl der Etappen und den Summen aus den Werten der Einheiten (auch der Einheiten ohne Aufzeichnung),
- den durchgehenden Verlauf: Distanz und Fahrzeit jeder Etappe werden um das Ende der vorigen Etappe verschoben, sodass Nächte und Pausen dazwischen entfallen; dazu der Beginn jeder Etappe auf beiden Achsen,
- die Beschreibung von Etappen für die Hinweise („Mi, 16.07.2025 Brenner“),
- das umschließende Rechteck mehrerer Strecken.

Im Höhenprofil wird jede Etappe als eigene Fläche bzw. Linie gezeichnet, damit über eine Etappe ohne Höhendaten keine Verbindungslinie läuft. An jedem Etappenbeginn steht eine senkrechte Linie, und beim Überfahren steht zusätzlich die Etappe. Die Karte zeichnet jede Etappe als eigene Linie. Die Farben wechseln reihum; benachbarte Etappen unterscheiden sich so immer. Start und Ziel der Reise tragen die bekannten Zeichen, jeder Etappenwechsel einen kleinen Punkt.

## Dateien
- `src/AndyTours.Core/Modell/Etappe.cs` (neu) — eine Etappe: die Einheit und der gelesene Verlauf ihrer Aufzeichnung (`null` ohne Aufzeichnung bzw. wenn sie fehlt oder nicht lesbar ist)
- `src/AndyTours.Core/Modell/Mehrtagesverlauf.cs` (neu) — durchgehende Messpunkte aller Etappen, die Etappen mit Verlauf und je Etappe die Stelle ihres ersten Messpunkts (Beginn in km und Fahrzeit ergibt sich daraus)
- `src/AndyTours.Core/Berechnungen/OrdneEtappen.cs` (neu) — Einheiten nach Datum und Startzeit (ohne Startzeit zuerst am Tag)
- `src/AndyTours.Core/Berechnungen/VerbindeVerlaeufe.cs` (neu) — macht aus den Verläufen der Etappen den durchgehenden Verlauf; Etappen ohne Verlauf entfallen
- `src/AndyTours.Core/Berechnungen/BeschreibeMehrtagestour.cs` (neu) — Kopfzeile, z. B. „Sa, 12.07.2025 – Fr, 18.07.2025 · 7 Etappen · 612,40 km · 8.230 hm · 31:12:05 h · Ø 19,6 km/h · höchster Punkt 2.504 m“
- `src/AndyTours.Core/Berechnungen/BeschreibeEtappen.cs` (neu) — Datum und Name von Etappen für die Hinweise, durch Strichpunkte getrennt (das Datum enthält schon ein Komma)
- `src/AndyTours.Core/Berechnungen/UmschliesseStrecken.cs` (neu) — Süd-West- und Nord-Ost-Ecke mehrerer Strecken
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Tourdetail.cs` (ändern) — Wählbarkeit bei mehreren Einheiten, Verläufe aller Etappen lesen, Meldung fehlender bzw. unlesbarer Dateien, Fenster mit allen Etappen öffnen
- `src/AndyTours.App/ViewModels/TourdetailViewModel.cs` (ändern) — nimmt die Etappen statt einer Einheit; Kopfzeile je nach Anzahl, Diagramm je Etappe, Etappenlinien, Etappe beim Überfahren, Hinweise ohne Aufzeichnung bzw. ohne Höhendaten
- `src/AndyTours.App/ViewModels/StreckenkarteViewModel.cs` (ändern) — Strecken je Etappe und gemeinsames Rechteck, Hinweis zu Etappen ohne GPS-Daten, Hinweis „Keine der Einheiten hat eine Aufzeichnung.“
- `src/AndyTours.App/Views/TourdetailFenster.xaml` (ändern) — Hinweis zu Einheiten ohne Aufzeichnung unter der Kopfzeile, Hinweise zu Etappen ohne Höhen- bzw. GPS-Daten in den Karteikarten, Zeichenerklärung „Etappenwechsel“ bei mehreren Etappen
- `src/AndyTours.App/Views/TourdetailFenster.xaml.cs` (ändern) — eine Linie je Etappe in wechselnden Farben, Punkte an den Etappenwechseln, Start der ersten und Ziel der letzten Etappe, „Ganze Strecke“ über alle Etappen
- `tests/AndyTours.Tests/Berechnungen/OrdneEtappenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/VerbindeVerlaeufeTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BeschreibeMehrtagestourTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BeschreibeEtappenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/UmschliesseStreckenTests.cs` (neu)
- `tests/AndyTours.Tests/Garmin/FitTestdatei.cs` (ändern) — Name der Testgruppe für FIT-Tests (nachträglich, siehe Entscheidungen)
- `tests/AndyTours.Tests/Garmin/LeseFitAufzeichnungTests.cs` (ändern) — in der Testgruppe „FIT-Dateien“
- `tests/AndyTours.Tests/Garmin/LeseFitVerlaufTests.cs` (ändern) — in der Testgruppe „FIT-Dateien“

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Anzeigen
- **Kriterium:** Wenn mehrere Einheiten markiert sind und der Benutzer „Einheit → Tourdetail…“ oder im Kontextmenü der Tabelle „Tourdetail…“ wählt, dann öffnet sich das Tourdetail für alle markierten Einheiten gemeinsam.
  - Umsetzung: `HauptfensterViewModel.Tourdetail`, `TourdetailViewModel`, `TourdetailFenster`
  - Manuell: die Etappen einer Reise markieren, beide Wege wählen.
- **Kriterium:** Wenn das Tourdetail mehrerer Einheiten geöffnet wird, dann sind die Etappen nach Datum und Startzeit geordnet, unabhängig von der Reihenfolge, in der sie markiert wurden.
  - Umsetzung: `OrdneEtappen`
  - Prüfung: `OrdneEtappenTests`
- **Kriterium:** Wenn das Tourdetail mehrerer Einheiten offen ist, dann stehen oben der Zeitraum vom ersten bis zum letzten Datum, die Anzahl der Etappen, die Summen von Distanz, Höhenmetern und Fahrzeit, die Durchschnittsgeschwindigkeit über alle Etappen und der höchste Punkt.
  - Umsetzung: `BeschreibeMehrtagestour` (Durchschnitt über `BerechneDurchschnittsgeschwindigkeit` aus den Summen), `TourdetailViewModel`
  - Prüfung: `BeschreibeMehrtagestourTests`
- **Kriterium:** Wenn das Höhenprofil angezeigt wird, dann folgen die Etappen hintereinander, die Distanz zählt über die Etappen weiter, und jeder Wechsel zur nächsten Etappe ist mit einer senkrechten Linie markiert.
  - Umsetzung: `VerbindeVerlaeufe`, `TourdetailViewModel` (senkrechte Linie je Etappenbeginn)
  - Prüfung: `VerbindeVerlaeufeTests` (Distanz und Beginn je Etappe)
  - Manuell: Profil einer Reise; die Linien stehen an den Etappenwechseln.
- **Kriterium:** Wenn der Benutzer die Achse auf „Fahrzeit“ umschaltet, dann zählt die Fahrzeit über die Etappen weiter; Nächte und Pausen zwischen den Etappen werden herausgelassen.
  - Umsetzung: `VerbindeVerlaeufe`, `TourdetailViewModel`
  - Prüfung: `VerbindeVerlaeufeTests` (Fahrzeit)
  - Manuell: Umschalten; das Ende der Achse entspricht der Summe der Fahrzeiten der Etappen mit Aufzeichnung.
- **Kriterium:** Wenn der Benutzer mit der Maus über das Profil fährt, dann erscheinen zusätzlich zu den bisherigen Werten Datum und Name der Etappe an dieser Stelle.
  - Umsetzung: `BeschreibeEtappen`, `TourdetailViewModel` (Hinweistext)
  - Prüfung: `BeschreibeEtappenTests`
  - Manuell: über zwei Etappen fahren.
- **Kriterium:** Wenn der Benutzer die Karteikarte „Karte“ wählt, dann zeigt sie alle Etappen, so vergrößert, dass die ganze Reise zu sehen ist.
  - Umsetzung: `UmschliesseStrecken`, `StreckenkarteViewModel`, `TourdetailFenster.xaml.cs`
  - Prüfung: `UmschliesseStreckenTests`
  - Manuell: Karteikarte „Karte“ einer Reise.
- **Kriterium:** Wenn die Karte mehrere Etappen zeigt, dann hat jede Etappe eine eigene Farbe, wobei benachbarte Etappen sich in der Farbe unterscheiden.
  - Umsetzung: `TourdetailFenster.xaml.cs` (Farben reihum)
  - Manuell: Reise mit mindestens drei Etappen.
- **Kriterium:** Wenn die Karte mehrere Etappen zeigt, dann sind der Start der ersten Etappe mit dem Startzeichen, das Ziel der letzten Etappe mit dem Zielzeichen und jeder Etappenwechsel mit einem kleinen Punkt markiert.
  - Umsetzung: `TourdetailFenster.xaml.cs`, `TourdetailFenster.xaml` (Zeichenerklärung)
  - Manuell: Zeichen an Start, Ziel und den Wechseln.
- **Kriterium:** Wenn der Benutzer im Tourdetail mehrerer Einheiten Häkchen setzt, vergrößert, die Kartenart wechselt, „Ganze Strecke“ wählt oder ohne Internet arbeitet, dann verhält es sich wie das Tourdetail einer einzelnen Einheit.
  - Umsetzung: unverändert in `TourdetailViewModel`, `StreckenkarteViewModel`, `TourdetailFenster`
  - Manuell: die Bedienung aus 018/019 an einer Reise durchgehen.

### Grenz- und Fehlerfälle
- **Kriterium:** Wenn genau eine Einheit markiert ist, dann öffnet „Tourdetail…“ wie bisher das Tourdetail dieser Einheit.
  - Umsetzung: `HauptfensterViewModel.Tourdetail` (eine Etappe), `TourdetailViewModel` (Kopfzeile über `BeschreibeTourdetail`)
  - Prüfung: `BeschreibeTourdetailTests`, `VerbindeVerlaeufeTests` (eine Etappe bleibt unverändert)
  - Manuell: Tourdetail einer einzelnen Einheit sieht aus wie bisher, ohne Etappenlinien.
- **Kriterium:** Wenn mehrere Einheiten markiert sind, dann ist „Tourdetail…“ wählbar, auch wenn keine davon eine Aufzeichnung hat.
  - Umsetzung: `HauptfensterViewModel.Tourdetail` (Wählbarkeit)
  - Manuell: zwei Einheiten ohne Aufzeichnung markieren.
- **Kriterium:** Wenn keine der markierten Einheiten eine Aufzeichnung hat, dann zeigt das Tourdetail oben die Summen, und in den Karteikarten „Höhenprofil“ und „Karte“ erscheint der Hinweis „Keine der Einheiten hat eine Aufzeichnung.“
  - Umsetzung: `TourdetailViewModel`, `StreckenkarteViewModel`, `TourdetailFenster.xaml`
  - Prüfung: `BeschreibeMehrtagestourTests` (Summen ohne Aufzeichnungen)
  - Manuell: wie oben; beide Karteikarten zeigen den Hinweis.
- **Kriterium:** Wenn unter den markierten Einheiten welche ohne Aufzeichnung sind, dann zählen sie zu den Summen oben, fehlen aber in Profil und Karte, und ein Hinweis nennt sie mit Datum und Name.
  - Umsetzung: `BeschreibeMehrtagestour`, `VerbindeVerlaeufe`, `BeschreibeEtappen`, `TourdetailViewModel`
  - Prüfung: `BeschreibeMehrtagestourTests`, `VerbindeVerlaeufeTests` (Etappe ohne Verlauf entfällt), `BeschreibeEtappenTests`
- **Kriterium:** Wenn die Aufzeichnung einer Etappe im Datenordner fehlt oder nicht gelesen werden kann, dann erscheint derselbe Hinweis wie beim Tourdetail einer einzelnen Einheit mit dem Namen der Datei.
  - Umsetzung: `HauptfensterViewModel.Tourdetail`
  - Manuell: eine Reise mit der Einheit vom 11.11.2019 markieren; die Meldung nennt die Datei, danach öffnet sich das Tourdetail ohne diese Etappe.
- **Kriterium:** Wenn nur einige Etappen Höhendaten bzw. GPS-Daten enthalten, dann zeigen Profil bzw. Karte die übrigen Etappen, und ein Hinweis nennt die Etappen ohne diese Daten.
  - Umsetzung: `ErmittleVerlaufswerte` und `ErmittleStrecke` je Etappe, `BeschreibeEtappen`, `TourdetailViewModel`, `StreckenkarteViewModel`
  - Prüfung: `ErmittleVerlaufswerteTests`, `ErmittleStreckeTests`, `BeschreibeEtappenTests`
  - Manuell: Reise mit einer Fahrt auf der Rolle.
- **Kriterium:** Wenn keine der Etappen Höhendaten bzw. GPS-Daten enthält, dann erscheint in der Karteikarte der Hinweis „Die Aufzeichnung enthält keine Höhendaten.“ bzw. „Die Aufzeichnung enthält keine GPS-Daten.“
  - Umsetzung: `TourdetailViewModel`, `StreckenkarteViewModel` (bestehende Hinweise)
  - Prüfung: `ErmittleVerlaufswerteTests`, `ErmittleStreckeTests`

## Entscheidungen
- Eine fehlende oder unlesbare Aufzeichnung bricht die Mehrtagestour nicht ab: Nach der Meldung öffnet sich das Tourdetail, und die Etappe zählt wie eine ohne Aufzeichnung (Summen ja, Profil und Karte nein, im Hinweis genannt). Mehrere betroffene Dateien stehen in einer gemeinsamen Meldung.
- Der Verlauf wird aus den Aufzeichnungen verbunden, nicht aus den Werten der Einheiten: Die Distanz der nächsten Etappe beginnt bei der letzten Distanz der vorigen Aufzeichnung. Die Kopfzeile dagegen summiert die Werte der Einheiten, wie es die Spec verlangt.
- Eine Etappe mit Aufzeichnung, aber ohne Höhendaten, nimmt im Profil trotzdem ihren Platz auf der Achse ein. So passen Distanz und Fahrzeit zur Karte und zur Summe der übrigen Etappen.
- Ohne Startzeit steht eine Einheit am Anfang ihres Tages; bei gleichem Datum und gleicher Startzeit entscheidet die Nummer der Einheit (Reihenfolge der Anlage), weil die Markierung keine verlässliche Reihenfolge liefert.
- Die Kopfzeile der Mehrtagestour hat ein eigenes Format; die einer einzelnen Einheit bleibt unverändert.
- Das Fenster heißt weiter „Tourdetail“. Das Kriterium aus 018 „Wenn keine oder mehrere Einheiten markiert sind, dann ist ‚Tourdetail…‘ nicht wählbar.“ gilt nur noch für keine markierte Einheit; 020 ersetzt es für mehrere.

- Nachträglich: Die Testklassen, die FIT-Dateien schreiben und lesen, laufen in einer gemeinsamen xUnit-Testgruppe nacheinander. Laufen sie gleichzeitig, ist gelegentlich eine Testdatei unlesbar (schon vor 020 in etwa jedem dritten Lauf). Die App liest Aufzeichnungen nacheinander und ist nicht betroffen.

## Risiken
- Eine Reise mit vielen langen Etappen hat leicht über 100.000 Messpunkte. Werden Profil oder Karte träge, werden die Punkte für die Anzeige ausgedünnt; die Werte beim Überfahren bleiben genau.
- Das Lesen mehrerer Aufzeichnungen dauert länger als bei einer; die Sanduhr bleibt bis zum Öffnen stehen.
