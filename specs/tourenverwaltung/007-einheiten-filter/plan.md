# Plan: 007 – Filter für die Trainingseinheiten

## Vorgehen
- **Filterzustand:** Ein eigenes `EinheitenfilterViewModel` hält die Eingaben: Kategorie, Equipment, Trainingsbereich (je Text, `(alle)` = kein Filter), Distanz von/bis, Höhenmeter von/bis und Name (je Text).
  - Ändert sich eine Eingabe, löst es das Ereignis `Geaendert` aus. Darauf baut das Hauptfenster die Tabelle sofort neu auf (kein „Filtern“-Knopf).
  - Es liefert `IstGesetzt` (mindestens ein Filter gesetzt), die Fehlermerker der vier Zahlenfelder und den Befehl `FilterZuruecksetzen`.
  - Der Zustand wird nirgends gespeichert. Nach einem Neustart sind die Filter also leer.
- **Zahlenfelder:** `LeseFilterbereich` (Core) wandelt die Texte von/bis in einen `Filterbereich` um.
  - Leer bedeutet keine Grenze. Komma und Punkt gelten beide als Dezimalzeichen.
  - Ein ungültiger Text markiert nur sein eigenes Feld als fehlerhaft. Ist „von“ größer als „bis“, sind beide Felder fehlerhaft.
  - Ein fehlerhafter Bereich wird nicht angewendet.
- **Filtern:** `FiltereEinheiten` (Core) wendet einen `Einheitenfilter` auf die Einheiten der Jahres- und Aktivitätsauswahl an. Alle gesetzten Bedingungen müssen gleichzeitig erfüllt sein.
  - **Kategorie und Equipment:** Verglichen wird über den Namen, aufgelöst über den Datenbestand. So bleibt die Wahl auch nach einem Neuladen sinnvoll. Eine Einheit mit mehreren Kategorien passt, wenn eine davon die gewählte ist.
  - **Trainingsbereich:** gleicher Text.
  - **Distanz:** Grenzen eingeschlossen. Eine Einheit mit 0 km hat keinen Wert und fällt bei aktivem Distanzfilter heraus.
  - **Höhenmeter:** Grenzen eingeschlossen. 0 hm ist wie in der Tabelle ein echter Wert.
  - **Name:** „enthält“, ohne Rücksicht auf Groß- und Kleinschreibung. Leerzeichen am Anfang und Ende der Eingabe werden ignoriert.
- **Auswahllisten:** `ErmittleFilterwerte` (Core) liefert die Kategorien, Equipments und Trainingsbereiche, die in der Jahres- und Aktivitätsauswahl vorkommen.
  - Die Listen sind alphabetisch sortiert, jeweils mit `(alle)` vorne.
  - Ein bereits gewählter Wert, der nicht mehr vorkommt, wird zusätzlich aufgenommen. So bleibt er sichtbar gewählt, und die Tabelle zeigt den Hinweis.
- **Hauptfenster:** `AktualisiereSummen` ermittelt wie bisher die Auswahl nach Jahr und Aktivität.
  - Hat sich Jahr, Aktivität oder der Datenbestand geändert, füllt es daraus die Auswahllisten des Filters neu; bei einer reinen Filteränderung bleiben die Listen, wie sie sind.
  - Danach wendet es `FiltereEinheiten` an.
  - Tabellenzeilen, Bestwerte (über `ErstelleEinheitszeilen`) und die Summenzeile entstehen aus dem gefilterten Ergebnis.
  - Die Summe in der Werkzeugleiste bleibt bei der ungefilterten Auswahl (Entscheidung A1).
  - Weil `AktualisiereSummen` auch beim Wechsel von Jahr oder Aktivität und beim Neuladen läuft, bleiben die Filter dabei erhalten und wirken auf die neue Auswahl.
- **Hinweis bei leerer Tabelle:** Der feste Text wird zur Eigenschaft `Hinweistext`.
  - Ist die Auswahl selbst leer, lautet er „Keine Einheiten in dieser Auswahl.“ (wie bisher).
  - Ist erst das Filterergebnis leer, lautet er „Keine Einheiten passen zum Filter.“.
- **Oberfläche:** Über der Tabelle liegt eine Filterleiste, die immer sichtbar ist, auch bei leerer Auswahl.
  - Sie enthält drei Auswahllisten, je zwei Felder für Distanz und Höhenmeter, das Namensfeld und die Schaltfläche „Filter zurücksetzen“.
  - Fehlerhafte Felder erhalten einen roten Rand mit Hinweis im Tooltip.
  - Die Schaltfläche ist hervorgehoben (fett, farbiger Hintergrund), solange `IstGesetzt` gilt.
  - Die Zahlenfelder aktualisieren bei jeder Eingabe (`UpdateSourceTrigger=PropertyChanged`).

## Dateien
- `src/AndyTours.Core/Modell/Einheitenfilter.cs` (neu) — die gesetzten Filter: Kategorie-, Equipment- und Trainingsbereichsname (je `null` = alle), Distanz- und Höhenmeterbereich, Namenstext
- `src/AndyTours.Core/Modell/Filterbereich.cs` (neu) — Von, Bis (je `null` = offen) und die Fehlermerker `VonFehlerhaft`, `BisFehlerhaft`; `IstGueltig`
- `src/AndyTours.Core/Modell/Filterwerte.cs` (neu) — Listen der angebotenen Kategorien, Equipments und Trainingsbereiche
- `src/AndyTours.Core/Berechnungen/LeseFilterbereich.cs` (neu) — Texte von/bis → `Filterbereich`
- `src/AndyTours.Core/Berechnungen/FiltereEinheiten.cs` (neu) — Einheiten, Filter und Datenbestand → passende Einheiten in unveränderter Reihenfolge
- `src/AndyTours.Core/Berechnungen/ErmittleFilterwerte.cs` (neu) — Auswahl, Datenbestand und aktuelle Wahl → `Filterwerte`
- `src/AndyTours.App/ViewModels/EinheitenfilterViewModel.cs` (neu) — Eingaben, Listen, Fehlermerker, `IstGesetzt`, `Geaendert`, `FilterZuruecksetzenCommand`, `ErstelleFilter()`
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — Eigenschaft `Filter`, Abonnement von `Geaendert`, Filter in `AktualisiereSummen` anwenden, `Hinweistext`
- `src/AndyTours.App/Views/EinheitenTabelle.xaml` (ändern) — Filterleiste über der Tabelle, Fehler- und Hervorhebungsstil, Hinweis an `Hinweistext` gebunden, Kopfkommentar anpassen
- `tests/AndyTours.Tests/Berechnungen/LeseFilterbereichTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/FiltereEinheitenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErmittleFilterwerteTests.cs` (neu)

## Abdeckung der Kriterien
- Wenn das Hauptfenster geöffnet ist, dann stehen über der Tabelle Filter für Kategorie, Equipment, Trainingsbereich, Distanz (von–bis), Höhenmeter (von–bis) und Name, und keiner davon ist gesetzt. → `EinheitenTabelle.xaml` (Filterleiste), Startwerte in `EinheitenfilterViewModel`; manuelle Prüfung: App starten, Filterleiste mit allen Feldern leer bzw. auf „(alle)“.
- Wenn der Benutzer die App neu startet, dann sind alle Filter wieder leer. → `EinheitenfilterViewModel` speichert nichts; manuelle Prüfung: Filter setzen, App beenden und starten, alle Filter leer.
- Wenn der Benutzer eine der Auswahllisten Kategorie, Equipment oder Trainingsbereich öffnet, dann bietet sie nur die Werte an, die in der aktuellen Jahres- und Aktivitätsauswahl vorkommen, alphabetisch sortiert. → `ErmittleFilterwerte`; geprüft durch `ErmittleFilterwerteTests` (`dotnet test AndyTours.slnx`).
- Wenn der Benutzer eine Kategorie wählt, dann zeigt die Tabelle nur Einheiten, denen diese Kategorie zugeordnet ist, auch wenn sie weitere Kategorien haben. → `FiltereEinheiten`; geprüft durch `FiltereEinheitenTests`.
- Wenn der Benutzer ein Equipment wählt, dann zeigt die Tabelle nur Einheiten mit diesem Equipment. → `FiltereEinheiten`; geprüft durch `FiltereEinheitenTests`.
- Wenn der Benutzer einen Trainingsbereich wählt, dann zeigt die Tabelle nur Einheiten mit diesem Trainingsbereich. → `FiltereEinheiten`; geprüft durch `FiltereEinheitenTests`.
- Wenn der Benutzer bei der Distanz einen Von- und/oder Bis-Wert einträgt, dann zeigt die Tabelle nur Einheiten in diesem Bereich, die Grenzwerte eingeschlossen. → `LeseFilterbereich`, `FiltereEinheiten`; geprüft durch `LeseFilterbereichTests`, `FiltereEinheitenTests` (nur von, nur bis, beide, genau auf der Grenze).
- Wenn der Benutzer bei den Höhenmetern einen Von- und/oder Bis-Wert einträgt, dann zeigt die Tabelle nur Einheiten in diesem Bereich, die Grenzwerte eingeschlossen. → wie Distanz; geprüft durch `FiltereEinheitenTests`.
- Wenn bei aktivem Distanz- oder Höhenmeterfilter eine Einheit keinen solchen Wert hat, dann wird sie nicht angezeigt. → `FiltereEinheiten` (0 km = kein Wert; 0 hm ist ein Wert); geprüft durch `FiltereEinheitenTests`.
- Wenn der Benutzer Text in das Namensfeld schreibt, dann zeigt die Tabelle nur Einheiten, deren Name diesen Text enthält, ohne Rücksicht auf Groß- und Kleinschreibung. → `FiltereEinheiten`; geprüft durch `FiltereEinheitenTests` (Groß-/Kleinschreibung, Umlaute, Leerzeichen am Rand).
- Wenn der Benutzer einen Filter ändert, dann wird die Tabelle sofort angepasst, ohne dass er eine Schaltfläche drücken muss. → `EinheitenfilterViewModel.Geaendert` → `AktualisiereSummen`, `UpdateSourceTrigger=PropertyChanged`; manuelle Prüfung: Buchstaben ins Namensfeld tippen, Tabelle ändert sich bei jedem Zeichen.
- Wenn mehrere Filter gesetzt sind, dann zeigt die Tabelle nur Einheiten, die alle Filter gleichzeitig erfüllen. → `FiltereEinheiten`; geprüft durch `FiltereEinheitenTests`.
- Wenn ein Filter gesetzt ist, dann beziehen sich die Summenzeile und die roten Bestwerte nur auf die angezeigten Einheiten. → `AktualisiereSummen` übergibt das gefilterte Ergebnis an `ErstelleEinheitszeilen` und `BerechneAuswahlsummen` (Bestwerte bereits geprüft durch `ErstelleEinheitszeilenTests`); manuelle Prüfung: Filter auf ein Rad setzen, Summenzeile und rote Werte ändern sich, Werkzeugleiste bleibt.
- Wenn keine Einheit zu den Filtern passt, dann zeigt die Karteikarte den Hinweis „Keine Einheiten passen zum Filter.“ und keine Summenzeile. → `Hinweistext`, `HatEinheiten` in `HauptfensterViewModel`; manuelle Prüfung: Namen „xyzxyz“ eingeben.
- Wenn bei Distanz oder Höhenmetern keine gültige Zahl steht oder „von“ größer als „bis“ ist, dann wird das Feld rot markiert und dieser Filter nicht angewendet. → `LeseFilterbereich`, Fehlerstil in `EinheitenTabelle.xaml`; geprüft durch `LeseFilterbereichTests` (Buchstaben, von > bis, Komma und Punkt), manuelle Prüfung: „abc“ bei km von → roter Rand, Tabelle ungefiltert.
- Wenn der Benutzer „Filter zurücksetzen“ wählt, dann werden alle Filter geleert, und die Tabelle zeigt wieder die ganze Jahres- und Aktivitätsauswahl. → `FilterZuruecksetzenCommand`; manuelle Prüfung: mehrere Filter setzen, zurücksetzen, alle leer, Anzahl gleich der Werkzeugleiste.
- Wenn mindestens ein Filter gesetzt ist, dann ist die Schaltfläche „Filter zurücksetzen“ hervorgehoben, sodass erkennbar ist, dass die Tabelle gefiltert ist. → `IstGesetzt`, Stil in `EinheitenTabelle.xaml`; manuelle Prüfung: Filter setzen → Schaltfläche hervorgehoben, zurücksetzen → normal.
- Wenn der Benutzer Jahr oder Aktivität wechselt oder der Datenbestand neu geladen wird, dann bleiben die gesetzten Filter erhalten und wirken auf die neue Auswahl. → `AktualisiereSummen` liest den bestehenden Filter; manuelle Prüfung: Filter setzen, Jahr wechseln, Filter weiterhin gesetzt und wirksam.
- Wenn ein gewählter Wert einer Auswahlliste in der neuen Auswahl nicht mehr vorkommt, dann bleibt er gewählt, und die Tabelle zeigt den Hinweis „Keine Einheiten passen zum Filter.“ → `ErmittleFilterwerte` nimmt die aktuelle Wahl auf; geprüft durch `ErmittleFilterwerteTests`, manuelle Prüfung: Rad wählen, Jahr ohne dieses Rad wählen.

## Entscheidungen
- A1: Die Summe in der Werkzeugleiste zeigt weiterhin die ungefilterte Jahres- und Aktivitätsauswahl; nur Tabelle, Summenzeile und Bestwerte folgen dem Filter.
- B1: Der Eintrag für „kein Filter“ in den Auswahllisten heißt „(alle)“.
- Komma und Punkt gelten beide als Dezimalzeichen; ein ungültiges Feld wird einzeln rot, bei „von“ > „bis“ beide.
- Kategorie und Equipment werden über den Namen gefiltert, damit die Wahl ein Neuladen des Datenbestands übersteht.
- Der Filterzustand bekommt ein eigenes ViewModel, damit `HauptfensterViewModel` nicht weiter wächst.

## Risiken
- Bei sehr vielen Einheiten könnte das Neuaufbauen der Tabelle bei jedem Tastendruck im Namensfeld spürbar sein; falls ja, wird die Aktualisierung leicht verzögert (z. B. 200 ms).
- Das Neufüllen der Auswahllisten bei jeder Filteränderung darf die gewählte Eintragung nicht zurücksetzen; deshalb werden die Listen nur bei Wechsel von Jahr, Aktivität oder Datenbestand neu gefüllt.
