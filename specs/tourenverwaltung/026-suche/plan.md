# Plan: 026 – Suche in Name und Kommentar

## Vorgehen
Der vorhandene Namensfilter wird zur Suche ausgebaut. Im Filtermodell `Einheitenfilter` heißt das Feld künftig `Suchbegriff` (bisher `Name`), im `EinheitenfilterViewModel` ebenso; die Filterleiste beschriftet es mit „Suche:“. Eine neue Fachfunktion `PasstZumSuchbegriff` zerlegt den Suchbegriff an Leerzeichen in Wörter und prüft, ob jedes Wort in Name oder Kommentar der Einheit vorkommt (ohne Unterschied der Groß- und Kleinschreibung); ohne Wörter passt jede Einheit. `FiltereEinheiten` ruft sie an Stelle des bisherigen Namensvergleichs auf.

Das sofortige Filtern beim Tippen, das Zusammenwirken mit Jahr, Aktivität und den übrigen Filtern, der Hinweis „Keine Einheiten passen zum Filter.“ und das Leeren durch „Filter zurücksetzen“ sind schon vorhanden und bleiben unverändert.

Für die neue Spalte erhält `Einheitszeile` den Kommentar (`ErstelleEinheitszeilen` füllt ihn), `Einheitenspalte` den Wert `Kommentar` (am Ende, damit gespeicherte Spaltenanordnungen gültig bleiben; `VervollstaendigeSpaltenanordnung` hängt neue Spalten selbst an), und die Einheitentabelle eine schreibgeschützte Textspalte „Kommentar“ samt Bezeichnung im Kontextmenü. Nach dem Speichern aus dem Bearbeitungsfenster wird die Tabelle schon heute neu aufgebaut.

## Dateien
- `src/AndyTours.Core/Berechnungen/PasstZumSuchbegriff.cs` (neu) — prüft, ob jedes Wort des Suchbegriffs in Name oder Kommentar vorkommt
- `src/AndyTours.Core/Modell/Einheitenfilter.cs` (ändern) — Feld `Name` → `Suchbegriff`
- `src/AndyTours.Core/Berechnungen/FiltereEinheiten.cs` (ändern) — Suche über `PasstZumSuchbegriff`
- `src/AndyTours.Core/Modell/Einheitszeile.cs` (ändern) — Eigenschaft `Kommentar`
- `src/AndyTours.Core/Berechnungen/ErstelleEinheitszeilen.cs` (ändern) — Kommentar übernehmen
- `src/AndyTours.Core/Modell/Einheitenspalte.cs` (ändern) — Wert `Kommentar` am Ende
- `src/AndyTours.App/ViewModels/EinheitenfilterViewModel.cs` (ändern) — Eigenschaft `Name` → `Suchbegriff`
- `src/AndyTours.App/Views/EinheitenTabelle.xaml` (ändern) — Beschriftung „Suche:“, Bindung an `Suchbegriff`, Spalte „Kommentar“
- `src/AndyTours.App/Views/EinheitenTabelle.xaml.cs` (ändern) — Spalte „Kommentar“ in Bezeichnungen und Spaltenzuordnung
- `tests/AndyTours.Tests/Berechnungen/PasstZumSuchbegriffTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/FiltereEinheitenTests.cs` (ändern) — Suchbegriff statt Name, Treffer im Kommentar
- `tests/AndyTours.Tests/Berechnungen/ErstelleEinheitszeilenTests.cs` (ändern) — Kommentar in der Zeile
- `tests/AndyTours.Tests/Berechnungen/UebernimmZellwertTests.cs` (ändern) — Spalte „Kommentar“ ist nicht in der Zelle bearbeitbar
- `tests/AndyTours.Tests/Berechnungen/SchalteSpalteUmTests.cs`, `VervollstaendigeSpaltenanordnungTests.cs`, `tests/AndyTours.Tests/Daten/SpaltenanordnungTests.cs` (ändern) — 18 statt 17 Spalten, „Kommentar“ als letzte

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Suchen
- **Kriterium:** Wenn in der Filterleiste ein Suchbegriff eingegeben wird, dann zeigt die Tabelle nur die Einheiten, deren Name oder Kommentar den Begriff enthält, ohne Unterschied der Groß- und Kleinschreibung.
  - Umsetzung: `PasstZumSuchbegriff`, aufgerufen in `FiltereEinheiten`
  - Prüfung: `PasstZumSuchbegriffTests`, `FiltereEinheitenTests`
  - Manuell: „sabine“ eingeben → die Touren mit dem Kommentar „mit Sabine“.
- **Kriterium:** Wenn der Suchbegriff aus mehreren Wörtern besteht, dann muss jedes Wort in Name oder Kommentar vorkommen, in beliebiger Reihenfolge (z. B. findet „Sabine Alm“ eine Einheit „Bründlingalm“ mit dem Kommentar „mit Sabine“).
  - Umsetzung: `PasstZumSuchbegriff` (Zerlegen an Leerzeichen, jedes Wort einzeln)
  - Prüfung: `PasstZumSuchbegriffTests` (genau das Beispiel; ein fehlendes Wort → kein Treffer)
- **Kriterium:** Wenn der Suchbegriff geändert wird, dann wird die Tabelle sofort beim Tippen neu gefiltert.
  - Umsetzung: vorhandene Bindung mit `UpdateSourceTrigger=PropertyChanged` und `OnSuchbegriffChanged` → `Neuberechnen`
  - Manuell: Buchstabe für Buchstabe tippen.
- **Kriterium:** Wenn nur Leerzeichen eingegeben sind, dann wird nicht gefiltert.
  - Umsetzung: `PasstZumSuchbegriff` (ohne Wörter passt jede Einheit)
  - Prüfung: `PasstZumSuchbegriffTests`, `FiltereEinheitenTests`
- **Kriterium:** Wenn die Suche zusammen mit den übrigen Filtern und mit der Jahres- und Aktivitätsauswahl wirkt, dann zeigt die Tabelle nur Einheiten, die alles erfüllen; für eine Suche über alle Jahre wird oben „alle Jahre“ gewählt.
  - Umsetzung: `FiltereEinheiten` (alle Bedingungen gleichzeitig) auf der Jahres- und Aktivitätsauswahl, unverändert
  - Prüfung: `FiltereEinheitenTests` (vorhandener Test aller Filter zusammen, mit Suchbegriff)
- **Kriterium:** Wenn keine Einheit passt, dann erscheint der Hinweis „Keine Einheiten passen zum Filter.“.
  - Umsetzung: vorhandener Hinweistext in `HauptfensterViewModel.AktualisiereSummen`
  - Manuell: „xyzxyz“ eingeben.
- **Kriterium:** Wenn „Filter zurücksetzen“ gewählt wird, dann ist das Suchfeld leer.
  - Umsetzung: `EinheitenfilterViewModel` („Filter zurücksetzen“ leert `Suchbegriff`)
  - Manuell: Suchbegriff eingeben, „Filter zurücksetzen“.
- **Kriterium:** Wenn die Filterleiste angezeigt wird, dann heißt das Feld „Suche:“ (bisher „Name:“).
  - Umsetzung: `EinheitenTabelle.xaml`
  - Manuell: Sichtprüfung.

### Spalte „Kommentar“
- **Kriterium:** Wenn die Einheitentabelle angezeigt wird, dann zeigt die Spalte „Kommentar“ den Kommentar jeder Einheit; ohne Kommentar bleibt sie leer.
  - Umsetzung: `Einheitszeile.Kommentar`, `ErstelleEinheitszeilen`, Spalte in `EinheitenTabelle.xaml`/`.xaml.cs`, `Einheitenspalte.Kommentar`
  - Prüfung: `ErstelleEinheitszeilenTests`
  - Manuell: Spalte „Kommentar“ am Ende der Tabelle; per Kontextmenü aus- und einblendbar wie die übrigen.
- **Kriterium:** Wenn nach der Spalte „Kommentar“ sortiert wird, dann gilt die alphabetische Reihenfolge; ein zweiter Klick kehrt sie um.
  - Umsetzung: Textspalte mit `SortMemberPath="Kommentar"`
  - Manuell: zweimal auf die Überschrift klicken.
- **Kriterium:** Wenn ein Kommentar im Bearbeitungsfenster geändert wird, dann zeigt die Spalte sofort den neuen Text, und die Suche berücksichtigt ihn.
  - Umsetzung: vorhandener Neuaufbau der Tabelle nach dem Speichern (`ErstelleEinheitszeilen`, `FiltereEinheiten` auf dem aktuellen Bestand)
  - Manuell: Kommentar einer Einheit ändern, danach nach dem neuen Wort suchen.

## Entscheidungen
- Die Spalte „Kommentar“ ist schreibgeschützt; der Kommentar wird wie bisher im Bearbeitungsfenster geändert.
- Die neue Spalte steht am Ende der Tabelle; über die vorhandene Spaltenanordnung lässt sie sich verschieben und ausblenden.
- Das Filterfeld heißt im Code künftig `Suchbegriff` statt `Name`, weil es nicht mehr nur den Namen durchsucht.
