# Plan: 025 – Ausgemustertes Equipment kennzeichnen

## Vorgehen
Eine neue Klasse `EquipmentAnzeigename` im Core bündelt Hin- und Rückweg des angezeigten Namens: `Bilden` liefert den Namen, bei ausgemustertem Equipment mit dem Zusatz „(ausgemustert)“; `Passt` sagt, ob ein angezeigter (oder gespeicherter) Name zu einem Equipment gehört. Die Funktion `Anzeigename` samt Konstante aus `ErmittleWartungsraeder` (024) zieht dorthin um; die Karteikarte „Wartung“ und die Einstellungen verwenden sie weiter.

Alle Stellen, die Equipment-Namen für Tabellen und Auswahlen liefern, verwenden künftig `Bilden`:

- `ErstelleEinheitszeilen` (Spalte „Equipment“; die Sortierung folgt dem angezeigten Text),
- `ErmittleEquipmentauswahl` (Bearbeitungsfenster, Auswahl beim Bearbeiten einer Zelle, „Equipment ändern…“) – zusätzlich nach „ausgemustert“ sortiert, damit das bisherige ausgemusterte Equipment am Ende steht,
- `UebernimmZellwert.Rohtext` (damit die Zellauswahl den angezeigten Eintrag vorwählt),
- `ErmittleFilterwerte` (Equipment-Liste der Filterleiste: im Einsatz alphabetisch, danach ausgemusterte).

Wo ein Name zurück in ein Equipment übersetzt wird – `UebernimmZellwert.Ausfuehren` und `FiltereEinheiten` –, vergleicht künftig `Passt`. Der gespeicherte Name bleibt unverändert. Da Ausmustern und Wiedereinsetzen schon heute Tabelle, Filterleiste und Wartungsansicht neu aufbauen (`AktualisiereSummen`), erscheint und verschwindet der Zusatz ohne weiteren Code. Eine Regel in `docs/Konventionen.md` legt fest, dass künftige Anzeigen den Namen ebenfalls über `EquipmentAnzeigename.Bilden` bilden.

## Dateien
- `src/AndyTours.Core/Berechnungen/EquipmentAnzeigename.cs` (neu) — `Bilden` (Name mit Zusatz) und `Passt` (angezeigter oder gespeicherter Name gehört zum Equipment)
- `src/AndyTours.Core/Berechnungen/ErmittleWartungsraeder.cs` (ändern) — `Anzeigename` und `ZusatzAusgemustert` entfallen, stattdessen `EquipmentAnzeigename.Bilden`
- `src/AndyTours.Core/Berechnungen/ErstelleWartungszeilen.cs` (ändern) — `EquipmentAnzeigename.Bilden`
- `src/AndyTours.Core/Berechnungen/ErstelleAlarmzeilen.cs` (ändern) — `EquipmentAnzeigename.Bilden`
- `src/AndyTours.Core/Berechnungen/ErstelleEinheitszeilen.cs` (ändern) — Spalte „Equipment“ mit Zusatz
- `src/AndyTours.Core/Berechnungen/ErmittleEquipmentauswahl.cs` (ändern) — Name mit Zusatz, ausgemusterte am Ende
- `src/AndyTours.Core/Berechnungen/UebernimmZellwert.cs` (ändern) — `Rohtext` mit Zusatz, Rückübersetzung über `Passt`
- `src/AndyTours.Core/Berechnungen/ErmittleFilterwerte.cs` (ändern) — Equipment-Liste mit Zusatz, ausgemusterte am Ende
- `src/AndyTours.Core/Berechnungen/FiltereEinheiten.cs` (ändern) — Equipment-Filter über `Passt`
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Equipments.cs` (ändern) — Zusatz in den Einstellungen über `EquipmentAnzeigename` statt eigenem Text
- `docs/Konventionen.md` (ändern) — Regel: Equipment-Namen in Anzeigen immer über `EquipmentAnzeigename.Bilden`
- `tests/AndyTours.Tests/Berechnungen/EquipmentAnzeigenameTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErstelleEinheitszeilenTests.cs` (ändern)
- `tests/AndyTours.Tests/Berechnungen/EquipmentauswahlTests.cs` (ändern)
- `tests/AndyTours.Tests/Berechnungen/ErmittleEingabelistenTests.cs` (ändern)
- `tests/AndyTours.Tests/Berechnungen/UebernimmZellwertTests.cs` (ändern)
- `tests/AndyTours.Tests/Berechnungen/ErmittleFilterwerteTests.cs` (ändern)
- `tests/AndyTours.Tests/Berechnungen/FiltereEinheitenTests.cs` (ändern)

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Einheitentabelle
- **Kriterium:** Wenn eine Einheit mit ausgemustertem Equipment in der Einheitentabelle steht, dann zeigt die Spalte „Equipment“ den Namen mit dem Zusatz „(ausgemustert)“.
  - Umsetzung: `ErstelleEinheitszeilen`
  - Prüfung: `ErstelleEinheitszeilenTests`
  - Manuell: eine alte Tour mit „Namur“ in der Tabelle suchen.
- **Kriterium:** Wenn nach der Spalte „Equipment“ sortiert wird, dann gilt die Reihenfolge der angezeigten Namen.
  - Umsetzung: Spalte sortiert nach dem Text der Zeile (`SortMemberPath="Equipment"`, unverändert), der den Zusatz enthält
  - Manuell: nach „Equipment“ sortieren; „Namur (ausgemustert)“ steht zwischen „Genius/Bike“ und „SL Road SL“.
- **Kriterium:** Wenn die Equipment-Zelle einer solchen Einheit direkt in der Tabelle bearbeitet wird, dann steht das bisherige Equipment mit dem Zusatz „(ausgemustert)“ am Ende der Auswahl, nach den Equipments im Einsatz; wird es gewählt oder unverändert gelassen, bleibt die Zuordnung erhalten.
  - Umsetzung: `ErmittleEquipmentauswahl`, `UebernimmZellwert` (`Rohtext`, `Passt`)
  - Prüfung: `EquipmentauswahlTests`, `ErmittleEingabelistenTests`, `UebernimmZellwertTests`
  - Manuell: Equipment-Zelle einer Namur-Tour bearbeiten, Eintrag wählen, Enter → Zuordnung bleibt.

### Bearbeitungsfenster
- **Kriterium:** Wenn eine Einheit mit ausgemustertem Equipment im Bearbeitungsfenster geöffnet wird, dann steht das bisherige Equipment mit dem Zusatz „(ausgemustert)“ am Ende der Auswahl und ist gewählt; beim Speichern bleibt die Zuordnung erhalten.
  - Umsetzung: `ErmittleEquipmentauswahl` (Auswahleinträge mit Nummer; die Wahl hängt an der Nummer, nicht am Namen)
  - Prüfung: `EquipmentauswahlTests`
  - Manuell: Namur-Tour öffnen, Kommentar ändern, speichern.

### Filterleiste
- **Kriterium:** Wenn die Auswahl „Equipment“ der Filterleiste ein ausgemustertes Equipment enthält (weil Einheiten der Auswahl es haben), dann steht es mit dem Zusatz „(ausgemustert)“ am Ende, nach den Equipments im Einsatz.
  - Umsetzung: `ErmittleFilterwerte`
  - Prüfung: `ErmittleFilterwerteTests`
- **Kriterium:** Wenn in der Filterleiste ein ausgemustertes Equipment gewählt wird, dann zeigt die Tabelle genau die Einheiten mit diesem Equipment.
  - Umsetzung: `FiltereEinheiten` über `EquipmentAnzeigename.Passt`
  - Prüfung: `FiltereEinheitenTests`
  - Manuell: Filter „Namur (ausgemustert)“ wählen.

### Allgemein
- **Kriterium:** Wenn ein Equipment ausgemustert oder wieder in Einsatz genommen wird, dann erscheint bzw. verschwindet der Zusatz überall sofort; sein gespeicherter Name bleibt unverändert.
  - Umsetzung: Zusatz entsteht nur in `EquipmentAnzeigename.Bilden`; vorhandener Neuaufbau nach dem Bearbeiten eines Equipments (`AktualisiereSummen`)
  - Prüfung: `EquipmentAnzeigenameTests` (Name des Equipments unverändert)
  - Manuell: ein Rad ausmustern und wieder einsetzen; Tabelle, Filterleiste und Karteikarte „Wartung“ beobachten.
- **Kriterium:** Wenn eine weitere Liste, Tabelle oder Auswahl Equipment anzeigt, dann trägt ausgemustertes Equipment dort ebenso den Zusatz „(ausgemustert)“ – auch in späteren Funktionen wie der Auskunft „Equipment-Einsatz“.
  - Umsetzung: alle vorhandenen Anzeigen über `EquipmentAnzeigename.Bilden` (Einheitentabelle, Auswahlen, Filterleiste, Wartung, Alarme, Einstellungen); Regel in `docs/Konventionen.md` für künftige Funktionen
  - Prüfung: Durchsicht der Stellen bei `/sdd-verify`; Tests der einzelnen Funktionen

## Entscheidungen
- `Passt` akzeptiert den gespeicherten Namen und den Namen mit Zusatz, unabhängig vom aktuellen Zustand. So funktionieren Zellbearbeitung und Filter auch mit einem Filterwert, der vor dem Ausmustern bzw. Wiedereinsetzen gewählt wurde.
- „Equipment ändern…“ (Mehrfachauswahl) und der Garmin-Import bieten ausgemusterte Equipments weiterhin nicht an. „Equipment ändern…“ läuft über `ErmittleEquipmentauswahl` und erhält `Bilden` ohnehin mit; der Garmin-Import bleibt unverändert.
- Hinweis- und Fehlertexte (z. B. „Für ‚Namur‘ gibt es schon …“, Löschsperre) nennen weiterhin den gespeicherten Namen; sie sind keine Listen oder Auswahlen.

## Risiken
- Ist ein Equipment als Filter gewählt und wird dann ausgemustert, steht in der Filterleiste vorübergehend sein bisheriger Name (ohne Zusatz) neben dem neuen Eintrag; der Filter wirkt weiter richtig, bis eine andere Wahl getroffen wird.
- Trägt ein Equipment im Einsatz zufällig einen Namen, der auf „ (ausgemustert)“ endet und dem angezeigten Namen eines anderen entspricht, wäre `Passt` mehrdeutig; das ist praktisch ausgeschlossen und wird nicht abgefangen.
