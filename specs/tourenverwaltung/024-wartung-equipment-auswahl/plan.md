# Plan: 024 – Equipment-Auswahl bei Wartung und Alarmen

## Vorgehen
Eine neue Klasse `Wartungsaktivitaet` im Core bündelt die beiden zusammengehörigen Regeln: `Bestimmen` liefert die Aktivität, die für die Karteikarte „Wartung“ gilt (die oben gewählte, bei „alle Aktivitäten“ die Hauptaktivität), `Passt` sagt, ob ein Equipment dazu gehört. Die drei vorhandenen Fachfunktionen erhalten diese Aktivität als zusätzlichen Parameter:

- `ErmittleWartungsraeder` liefert nur noch Equipments im Einsatz der Aktivität, alphabetisch; das bisherige Equipment einer bearbeiteten Wartung bzw. eines bearbeiteten Alarms bleibt (mit „(ausgemustert)“, falls ausgemustert) wählbar. Die Variante „alle Räder samt ausgemusterten“ für die Auswahl über den Listen entfällt.
- `ErstelleWartungszeilen` und `ErstelleAlarmzeilen` zeigen bei „Alle Equipments“ nur die Einträge der Equipments dieser Aktivität, ausgemusterte eingeschlossen. Ohne Aktivität (Parameter leer) bleibt alles wie bisher – so zählen Titel der Karteikarte und Hinweis beim Programmstart weiter alle fälligen Alarme.

Im Hauptfenster-ViewModel heißt der erste Eintrag der Auswahl „Alle Equipments“; Auswahl, Listen und Fenster verwenden die ermittelte Aktivität. Ein Wechsel der Aktivität und jede Änderung an Equipments rufen schon heute `AktualisiereSummen` und damit `AktualisiereWartungen` auf; dort fällt eine Wahl, die es in der neuen Auswahl nicht mehr gibt, bereits auf den ersten Eintrag zurück.

## Dateien
- `src/AndyTours.Core/Berechnungen/Wartungsaktivitaet.cs` (neu) — `Bestimmen` (gültige Aktivität) und `Passt` (gehört ein Equipment dazu)
- `src/AndyTours.Core/Berechnungen/ErmittleWartungsraeder.cs` (ändern) — nach Aktivität filtern, ausgemusterte nur noch als bisheriges Equipment, Parameter `alleRaeder` entfällt; `Anzeigename` mit Zusatz „(ausgemustert)“ für Auswahl und Listen
- `src/AndyTours.Core/Berechnungen/ErstelleWartungszeilen.cs` (ändern) — bei „Alle Equipments“ nur Wartungen der Aktivität; Name ausgemusterter Equipments mit Zusatz
- `src/AndyTours.Core/Berechnungen/ErstelleAlarmzeilen.cs` (ändern) — bei „Alle Equipments“ nur Alarme der Aktivität; ohne Aktivität alle; Name ausgemusterter Equipments mit Zusatz
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Wartungen.cs` (ändern) — „Alle Equipments“, Aktivität für Auswahl, Liste und Wartungsfenster
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Alarme.cs` (ändern) — Aktivität für Alarmliste und Alarmfenster; Titel weiter über alle Alarme
- `src/AndyTours.App/Views/WartungenAnsicht.xaml` (ändern) — Beschriftung „Equipment:“ statt „Rad:“, Kommentar zum Aufbau
- `tests/AndyTours.Tests/Berechnungen/WartungsaktivitaetTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErmittleWartungsraederTests.cs` (ändern) — neue Regeln statt „alle Räder“
- `tests/AndyTours.Tests/Berechnungen/ErstelleWartungszeilenTests.cs` (ändern) — Filter nach Aktivität
- `tests/AndyTours.Tests/Berechnungen/ErstelleAlarmzeilenTests.cs` (ändern) — Filter nach Aktivität

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Auswahl
- **Kriterium:** Wenn oben „alle Aktivitäten“ gewählt ist, dann gilt für die Karteikarte „Wartung“ und die Fenster für Wartungen und Alarme die Hauptaktivität (z. B. „Bike“).
  - Umsetzung: `Wartungsaktivitaet.Bestimmen`, aufgerufen in `HauptfensterViewModel.Wartungen`/`.Alarme`
  - Prüfung: `WartungsaktivitaetTests`
  - Manuell: „alle Aktivitäten“ wählen → Auswahl zeigt nur Räder.
- **Kriterium:** Wenn auf der Karteikarte „Wartung“ ein Equipment gewählt wird, dann bietet die Auswahl nach „Alle Equipments“ nur die Equipments im Einsatz an, die zur oben gewählten Aktivität gehören, alphabetisch; ausgemusterte erscheinen nicht.
  - Umsetzung: `ErmittleWartungsraeder`, `HauptfensterViewModel.AktualisiereWartungen`
  - Prüfung: `ErmittleWartungsraederTests`
  - Manuell: „Bike“ → nur Räder im Einsatz; „Laufen“ → nur der Schuh im Einsatz.
- **Kriterium:** Wenn die Auswahl auf der Karteikarte „Wartung“ angezeigt wird, dann heißt ihr erster Eintrag „Alle Equipments“ (bisher „Alle Räder“).
  - Umsetzung: `HauptfensterViewModel.Wartungen` (Eintrag `AlleEquipments`)
  - Manuell: Auswahl aufklappen.
- **Kriterium:** Wenn eine Wartung oder ein Alarm neu angelegt wird, dann bietet das Fenster nur die Equipments im Einsatz an, die zur oben gewählten Aktivität gehören.
  - Umsetzung: `ErmittleWartungsraeder`, `OeffneWartungsfenster`, `OeffneAlarmfenster`
  - Prüfung: `ErmittleWartungsraederTests`
  - Manuell: „Neue Wartung…“ und „Neuer Alarm…“ bei „Bike“ und bei „Laufen“.
- **Kriterium:** Wenn eine vorhandene Wartung oder ein vorhandener Alarm bearbeitet wird, dessen Equipment inzwischen ausgemustert ist, dann bleibt dieses Equipment im Fenster mit dem Zusatz „(ausgemustert)“ wählbar, damit sich der Eintrag ohne Umhängen speichern lässt.
  - Umsetzung: `ErmittleWartungsraeder` (bisheriges Equipment)
  - Prüfung: `ErmittleWartungsraederTests`
  - Manuell: eine Wartung von „Namur“ öffnen und speichern.

### Listen
- **Kriterium:** Wenn „Alle Equipments“ gewählt ist, dann zeigen die Listen „Wartungen“ und „Alarme“ nur die Einträge der Equipments, die zur oben gewählten Aktivität gehören.
  - Umsetzung: `ErstelleWartungszeilen`, `ErstelleAlarmzeilen` mit Aktivität
  - Prüfung: `ErstelleWartungszeilenTests`, `ErstelleAlarmzeilenTests`
- **Kriterium:** Wenn ein Equipment ausgemustert ist, dann bleiben seine Wartungen und Alarme unter „Alle Equipments“ sichtbar und lassen sich bearbeiten und löschen.
  - Umsetzung: `ErstelleWartungszeilen`, `ErstelleAlarmzeilen` (ausgemusterte nicht ausgefiltert)
  - Prüfung: `ErstelleWartungszeilenTests`, `ErstelleAlarmzeilenTests`
  - Manuell: Wartungen von „Namur“ unter „Bike“ sichtbar, Bearbeiten und Löschen möglich.
- **Kriterium:** Wenn ein Equipment ausgemustert ist, dann steht in den Listen „Wartungen“ und „Alarme“ hinter seinem Namen der Zusatz „(ausgemustert)“.
  - Umsetzung: `ErmittleWartungsraeder.Anzeigename`, verwendet in `ErstelleWartungszeilen` und `ErstelleAlarmzeilen`
  - Prüfung: `ErstelleWartungszeilenTests`, `ErstelleAlarmzeilenTests`
  - Manuell: unter „Bike“ steht in beiden Listen „Namur (ausgemustert)“.
- **Kriterium:** Wenn Alarme fällig sind, dann zählen der Titel der Karteikarte „Wartung“ und der Hinweis beim Programmstart alle fälligen Alarme, unabhängig von der gewählten Aktivität.
  - Umsetzung: `FuelleAlarmzeilen` (Titel über `ErstelleAlarmzeilen` ohne Rad und ohne Aktivität), `BeschreibeFaelligeAlarme` (unverändert, ohne Aktivität)
  - Prüfung: `ErstelleAlarmzeilenTests` (ohne Aktivität alle Alarme), vorhandene `BeschreibeFaelligeAlarmeTests`

### Aktualität
- **Kriterium:** Wenn oben eine andere Aktivität gewählt wird, dann passen sich Auswahl und Listen sofort an; gehört das gewählte Equipment nicht zur neuen Aktivität, steht die Auswahl danach auf „Alle Equipments“.
  - Umsetzung: `OnAusgewaehlteAktivitaetChanged` → `AktualisiereSummen` → `AktualisiereWartungen` (Rückfall auf den ersten Eintrag)
  - Manuell: Rad wählen, dann „Laufen“ → „Alle Equipments“, Listen zeigen Laufschuh-Einträge.
- **Kriterium:** Wenn ein Equipment ausgemustert, wieder in Einsatz genommen oder einer anderen Aktivität zugeordnet wird, dann zeigen Auswahl und Listen sofort den neuen Stand.
  - Umsetzung: vorhandener Neuaufbau nach dem Bearbeiten eines Equipments (`AktualisiereSummen`)
  - Manuell: ein Rad in den Einstellungen ausmustern und wieder einsetzen.

## Entscheidungen
- Bei „alle Aktivitäten“ gilt die Hauptaktivität (B1b); gibt es keine Hauptaktivität, wird nicht nach Aktivität gefiltert.
- Ein Equipment ohne zugeordnete Aktivität gilt als passend zu jeder Aktivität, damit es erreichbar bleibt; eine Wartung ohne Equipment erscheint ebenfalls immer.
- Beim Bearbeiten bleibt das bisherige Equipment auch dann wählbar, wenn es zu einer anderen Aktivität gehört (nachträglich umgehängtes Equipment).
- Die Beschriftung vor der Auswahl heißt „Equipment:“ statt „Rad:“, passend zu „Alle Equipments“.
- Titel der Karteikarte und Hinweis beim Programmstart rufen die Fachfunktionen ohne Aktivität auf und zählen so alle fälligen Alarme (A4a).

## Risiken
- Der Hinweis einer leeren Liste lautet weiter „Keine Wartungen für dieses Rad.“ bzw. „Keine Alarme für dieses Rad.“, auch wenn unter „Laufen“ nur „Alle Equipments“ gewählt ist; der Text bleibt verständlich und wird nicht geändert.
