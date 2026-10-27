# Plan: 022 – Wartungsalarme

## Vorgehen
Alarme sind Einträge der vorhandenen Tabelle `Wartung` mit dem Typ „Alarm“: Rad, Tätigkeit, Intervall (`AlarmKm`), eingeschaltet (`Aktiv`) und als Datum der Tag des Anlegens. Diese Felder stammen aus BikeXperience; Einfügen, Ändern und Löschen in `WartungenTabelle` schreiben sie schon. Die Wartungstabelle aus 021 zeigt weiterhin nur den Typ „Wartung“.

Fachfunktionen im Core liefern:
- den Stand eines Alarms: Bezugstag (letzte Wartung mit derselben Tätigkeit an diesem Rad, sonst Tag des Anlegens), gefahrene km nach dem Bezugstag, verbleibende km und Zustand (normal, bald fällig, fällig, ausgeschaltet; ausgemusterte Räder nie fällig),
- die Zeilen der Alarmliste, gefiltert nach Rad und geordnet,
- die Prüfung der Eingaben des Alarmfensters (Rad, Tätigkeit, ganzzahliges Intervall > 0, kein zweiter Alarm für Rad und Tätigkeit) und den Alarm daraus,
- den Text des Hinweises beim Programmstart.

Die Karteikarte „Wartung“ bekommt über der Wartungstabelle die Alarmliste mit eigener Leiste („Neuer Alarm…“, „Bearbeiten…“, „Löschen“) und Kontextmenü (zusätzlich „Ein-/Ausschalten“). Radauswahl, Markierung und Neuaufbau folgen dem Muster der Wartungen: Die Alarmzeilen werden bei jedem Neuaufbau der Wartungen mit aufgebaut, also nach Änderungen an Einheiten, Rädern, Wartungen und Tätigkeiten. Der Titel der Karteikarte kommt aus dem ViewModel. Nach dem ersten Anzeigen des Hauptfensters meldet das ViewModel fällige Alarme.

Die Liste „Tätigkeit“ zählt künftig Wartungen und Alarme getrennt: Rückfrage beim Umbenennen und Löschsperre nennen beide.

## Dateien
- `src/AndyTours.Core/Modell/Alarmzustand.cs` (neu) — Normal, BaldFaellig, Faellig, Ausgeschaltet
- `src/AndyTours.Core/Modell/Alarmstand.cs` (neu) — Bezugstag, gefahrene km, verbleibende km, Zustand eines Alarms
- `src/AndyTours.Core/Modell/Alarmzeile.cs` (neu) — eine Zeile der Alarmliste: Id, Rad, Tätigkeit, Intervall, gefahren, verbleibend, Zustand
- `src/AndyTours.Core/Modell/Alarmeingaben.cs` (neu) — Eingaben des Alarmfensters (Rad, Tätigkeit, Intervall als Text, eingeschaltet)
- `src/AndyTours.Core/Modell/Alarmfeld.cs` (neu) — prüfbare Felder: Rad, Tätigkeit, Intervall
- `src/AndyTours.Core/Modell/Alarmergebnis.cs` (neu) — Ergebnis der Prüfung: Alarm oder fehlerhafte Felder samt Hinweis
- `src/AndyTours.Core/Berechnungen/BerechneAlarmstand.cs` (neu) — Stand eines Alarms an einem Tag
- `src/AndyTours.Core/Berechnungen/ErstelleAlarmzeilen.cs` (neu) — Zeilen der Alarmliste, gefiltert nach Rad, fällige zuerst
- `src/AndyTours.Core/Berechnungen/AlarmEingaben.cs` (neu) — Eingaben aus einem Alarm und Alarm aus den Eingaben (mit Prüfung samt Doppelprüfung)
- `src/AndyTours.Core/Berechnungen/BeschreibeFaelligeAlarme.cs` (neu) — Text des Hinweises beim Start; `null` ohne fällige Alarme
- `src/AndyTours.Core/Berechnungen/ZaehleTaetigkeit.cs` (ändern) — zählt nur Einträge eines Typs (Wartung oder Alarm)
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Alarme.cs` (neu) — Alarmzeilen, Markierung, Befehle „Neuer Alarm…“, „Bearbeiten…“, „Löschen“, „Ein-/Ausschalten“, Titel der Karteikarte, Hinweis beim Start
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Wartungen.cs` (ändern) — beim Neuaufbau und nach dem Speichern/Löschen von Wartungen auch die Alarme neu aufbauen; Radwahl gilt für beide Listen
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Auswahllisten.cs` (ändern) — Tätigkeit: Wartungen und Alarme getrennt zählen, Löschsperre nennt beide, nach Umbenennen auch Alarme neu aufbauen
- `src/AndyTours.App/ViewModels/AuswahllistenViewModel.cs` (ändern) — Rückfrage beim Umbenennen einer Tätigkeit mit Wartungen und Alarmen
- `src/AndyTours.App/ViewModels/IAuswahllistenSpeicher.cs` (ändern) — Beschreibung: bei Tätigkeiten zweiter Wert = Alarme
- `src/AndyTours.App/ViewModels/AlarmBearbeitenViewModel.cs` (neu) — Felder, Auswahl von Rad und Tätigkeit, Fehlermarkierung, Hinweis, „OK“ nur ohne Fehler
- `src/AndyTours.App/Views/AlarmBearbeitenFenster.xaml` (neu) — Rad, Tätigkeit, Intervall (km), „Alarm eingeschaltet“, Hinweis, „OK“, „Abbrechen“
- `src/AndyTours.App/Views/AlarmBearbeitenFenster.xaml.cs` (neu) — Code-Behind
- `src/AndyTours.App/Views/WartungenAnsicht.xaml` (ändern) — Alarmliste mit Leiste, Farben je Zustand, Kontextmenü und Hinweis ohne Alarme über der Wartungstabelle
- `src/AndyTours.App/Views/WartungenAnsicht.xaml.cs` (ändern) — Markierung, Doppelklick und Entf der Alarmliste
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Titel der Karteikarte „Wartung“ aus dem ViewModel
- `src/AndyTours.App/Views/Hauptfenster.xaml.cs` (ändern) — nach dem ersten Anzeigen fällige Alarme melden lassen
- `tests/AndyTours.Tests/Berechnungen/BerechneAlarmstandTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErstelleAlarmzeilenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/AlarmEingabenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BeschreibeFaelligeAlarmeTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ZaehleUndBenenneTaetigkeitTests.cs` (ändern) — Zählen je Typ; Umbenennen erfasst auch Alarme
- `tests/AndyTours.Tests/Daten/WartungenTests.cs` (ändern) — Alarm mit Intervall und Ein/Aus speichern, ändern und laden

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Anzeigen
- **Kriterium:** Wenn der Benutzer die Karteikarte „Wartung“ öffnet, dann sieht er über der Wartungstabelle eine eigene Liste der Alarme mit Rad, Tätigkeit, Intervall (km), gefahrenen km seit dem Bezugspunkt und verbleibenden km.
  - Umsetzung: `ErstelleAlarmzeilen`, `HauptfensterViewModel.Alarme`, `WartungenAnsicht`
  - Prüfung: `ErstelleAlarmzeilenTests`
  - Manuell: Alarm anlegen, Liste über der Wartungstabelle ansehen.
- **Kriterium:** Wenn ein Alarm angezeigt wird, dann sind die gefahrenen km die Distanzen aller Einheiten mit diesem Rad nach dem Bezugspunkt; Bezugspunkt ist der Tag der letzten Wartung mit derselben Tätigkeit an diesem Rad, ohne eine solche der Tag, an dem der Alarm angelegt wurde.
  - Umsetzung: `BerechneAlarmstand`
  - Prüfung: `BerechneAlarmstandTests` (letzte von mehreren Wartungen, Einheiten am Bezugstag zählen nicht, andere Räder und andere Tätigkeiten zählen nicht, ohne Wartung ab dem Tag des Anlegens, Schreibweise egal)
- **Kriterium:** Wenn ein Rad seit dem Bezugspunkt mindestens die Intervall-km gefahren ist, dann ist der Alarm fällig: Die Zeile ist rot, und die verbleibenden km erscheinen als Überschreitung (z. B. „−120“).
  - Umsetzung: `BerechneAlarmstand`, `WartungenAnsicht` (Zeilenfarbe je Zustand)
  - Prüfung: `BerechneAlarmstandTests` (genau erreicht ist fällig, verbleibend negativ)
  - Manuell: Alarm mit kleinem Intervall anlegen; Zeile rot, „−…“.
- **Kriterium:** Wenn bei einem Alarm nur noch höchstens 10 % des Intervalls übrig sind, er aber noch nicht fällig ist, dann ist er bald fällig, und die Zeile ist orange.
  - Umsetzung: `BerechneAlarmstand`, `WartungenAnsicht`
  - Prüfung: `BerechneAlarmstandTests` (genau 10 % übrig ist bald fällig, etwas mehr nicht)
  - Manuell: Intervall knapp über den gefahrenen km wählen; Zeile orange.
- **Kriterium:** Wenn mindestens ein Alarm fällig ist, dann trägt die Karteikarte die Anzahl der fälligen Alarme im Titel, z. B. „Wartung (2)“; ohne fällige Alarme heißt sie „Wartung“.
  - Umsetzung: `HauptfensterViewModel.Alarme`, `Hauptfenster.xaml`
  - Manuell: Alarm fällig machen und wieder ausschalten; Titel wechselt.
- **Kriterium:** Wenn beim Programmstart Alarme fällig sind, dann erscheint ein Hinweis, der jeden fälligen Alarm mit Rad, Tätigkeit und Überschreitung nennt.
  - Umsetzung: `BeschreibeFaelligeAlarme`, `HauptfensterViewModel.Alarme`, `Hauptfenster.xaml.cs`
  - Prüfung: `BeschreibeFaelligeAlarmeTests`
  - Manuell: mit fälligem Alarm neu starten.
- **Kriterium:** Wenn oben ein Rad gewählt ist, dann zeigt die Alarmliste nur dessen Alarme; mit „Alle Räder“ wieder alle.
  - Umsetzung: `ErstelleAlarmzeilen` (Filter), `HauptfensterViewModel.Wartungen`/`.Alarme`
  - Prüfung: `ErstelleAlarmzeilenTests`
- **Kriterium:** Wenn es keine Alarme gibt, dann steht statt der Alarmliste „Keine Alarme eingerichtet.“
  - Umsetzung: `HauptfensterViewModel.Alarme`, `WartungenAnsicht`
  - Manuell: ohne Alarme die Karteikarte öffnen.

### Anlegen, ändern, löschen
- **Kriterium:** Wenn der Benutzer „Neuer Alarm…“ wählt, dann öffnet sich ein Fenster mit Rad, Tätigkeit (aus der Liste „Tätigkeit“, ohne freie Eingabe) und Intervall in km; ist oben ein Rad gewählt, ist es vorgewählt.
  - Umsetzung: `HauptfensterViewModel.Alarme`, `AlarmBearbeitenViewModel`, `AlarmBearbeitenFenster`
  - Manuell: Rad wählen, „Neuer Alarm…“, Tätigkeitsauswahl öffnen.
- **Kriterium:** Wenn der Benutzer mit „OK“ bestätigt, dann erscheint der Alarm sofort in der Liste und bleibt nach einem Neustart erhalten.
  - Umsetzung: `AlarmEingaben`, `WartungenTabelle.Einfuegen`/`Aendern`, `HauptfensterViewModel.Alarme`
  - Prüfung: `AlarmEingabenTests`, `WartungenTests` (Alarm speichern und laden)
  - Manuell: Alarm anlegen, App neu starten.
- **Kriterium:** Wenn der Benutzer einen Alarm doppelt anklickt oder „Bearbeiten…“ wählt, dann öffnet sich dasselbe Fenster mit seinen Werten; Änderungen erscheinen nach „OK“, „Abbrechen“ verwirft sie.
  - Umsetzung: `AlarmEingaben.AlsEingaben`, `AlarmBearbeitenViewModel`, `WartungenAnsicht`
  - Prüfung: `AlarmEingabenTests` (Hin- und Rückweg, Tag des Anlegens bleibt)
  - Manuell: Doppelklick, ändern, „OK“; erneut, „Abbrechen“.
- **Kriterium:** Wenn der Benutzer einen oder mehrere Alarme markiert und „Löschen“ wählt oder Entf drückt, dann erscheint eine Rückfrage mit der Anzahl, und erst nach „Ja“ werden sie gelöscht.
  - Umsetzung: `HauptfensterViewModel.Alarme`, `WartungenTabelle.Loeschen`, `WartungenAnsicht` (Entf)
  - Manuell: zwei Alarme markieren, Entf, erst „Nein“, dann „Ja“.
- **Kriterium:** Wenn der Benutzer einen Alarm ausschaltet, dann wird er grau dargestellt, nicht fällig oder bald fällig und nicht beim Start gemeldet, bis er ihn wieder einschaltet.
  - Umsetzung: `BerechneAlarmstand` (Zustand „ausgeschaltet“), `BeschreibeFaelligeAlarme`, `HauptfensterViewModel.Alarme` (Ein-/Ausschalten), `AlarmBearbeitenFenster`
  - Prüfung: `BerechneAlarmstandTests`, `BeschreibeFaelligeAlarmeTests`
  - Manuell: fälligen Alarm über das Kontextmenü aus- und wieder einschalten.

### Zurücksetzen
- **Kriterium:** Wenn der Benutzer für ein Rad eine Wartung mit der Tätigkeit eines Alarms erfasst, ändert oder löscht, dann zählt der Alarm sofort ab dem Tag der nun letzten solchen Wartung.
  - Umsetzung: `BerechneAlarmstand`, `HauptfensterViewModel.Wartungen` (Alarme nach jeder Wartungsänderung neu aufbauen)
  - Prüfung: `BerechneAlarmstandTests`
  - Manuell: fälligen Alarm durch Erfassen der passenden Wartung zurücksetzen, die Wartung wieder löschen.
- **Kriterium:** Wenn eine Tätigkeit in der Liste „Tätigkeit“ umbenannt wird, dann tragen auch die Alarme mit dieser Tätigkeit den neuen Namen, und eine verwendete Tätigkeit lässt sich nicht löschen, solange ein Alarm sie nutzt.
  - Umsetzung: `WartungenTabelle.ErsetzeTaetigkeit` und `BenenneTaetigkeitUm` (erfassen schon alle Einträge), `ZaehleTaetigkeit`, `HauptfensterViewModel.Auswahllisten`, `AuswahllistenViewModel`
  - Prüfung: `ZaehleUndBenenneTaetigkeitTests`
  - Manuell: Tätigkeit eines Alarms umbenennen und löschen versuchen.

### Grenz- und Fehlerfälle
- **Kriterium:** Wenn das Intervall leer, keine ganze Zahl oder nicht größer als 0 ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
  - Umsetzung: `AlarmEingaben` (über `Eingabetext.LeseGanzzahl`), `AlarmBearbeitenViewModel`
  - Prüfung: `AlarmEingabenTests`
- **Kriterium:** Wenn kein Rad oder keine Tätigkeit gewählt ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
  - Umsetzung: `AlarmEingaben`, `AlarmBearbeitenViewModel`, `AlarmBearbeitenFenster`
  - Prüfung: `AlarmEingabenTests`
- **Kriterium:** Wenn für dasselbe Rad und dieselbe Tätigkeit schon ein Alarm besteht, dann ist „OK“ nicht möglich, und ein Hinweis nennt den vorhandenen Alarm.
  - Umsetzung: `AlarmEingaben` (gegen die übrigen Alarme, Schreibweise egal, der bearbeitete selbst zählt nicht)
  - Prüfung: `AlarmEingabenTests`
- **Kriterium:** Wenn ein Rad ausgemustert ist, dann wird es für neue Alarme nicht angeboten, und seine Alarme werden weder fällig noch bald fällig noch beim Start gemeldet.
  - Umsetzung: `ErmittleWartungsraeder` (vorhanden), `BerechneAlarmstand`, `BeschreibeFaelligeAlarme`
  - Prüfung: `BerechneAlarmstandTests`, `BeschreibeFaelligeAlarmeTests`
- **Kriterium:** Wenn keine oder mehrere Alarme markiert sind, dann ist „Bearbeiten…“ nicht wählbar; ohne Markierung ist auch „Löschen“ nicht wählbar.
  - Umsetzung: `HauptfensterViewModel.Alarme` (Wählbarkeit), `WartungenAnsicht`
  - Manuell: keine, einen und zwei Alarme markieren.
- **Kriterium:** Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und die Liste bleibt beim vorherigen Stand.
  - Umsetzung: `HauptfensterViewModel.Alarme` (erst schreiben, dann den Bestand ändern, über das vorhandene `Schreibe`)
  - Manuell: nicht erneut nötig; dieselbe Fehlerbehandlung ist bei Einheiten, Wartungen und Auswahllisten geprüft.

## Entscheidungen
- Ein-/Ausschalten (B1a): Kontrollkästchen „Alarm eingeschaltet“ im Alarmfenster und „Ein-/Ausschalten“ im Kontextmenü für alle markierten Alarme.
- Alarme liegen als Typ „Alarm“ in der vorhandenen Tabelle `Wartung`; ihr Datum ist der Tag des Anlegens und bleibt beim Bearbeiten erhalten.
- Gezählt werden die Einheiten nach dem Bezugstag; eine Fahrt am Tag der Wartung zählt zum alten Teil (passend zum km-Stand „einschließlich des Tages“).
- Reihenfolge der Alarmliste: fällig, bald fällig, dann nach verbleibenden km aufsteigend; ausgeschaltete und die ausgemusterter Räder am Ende. Die Spalten lassen sich wie die Wartungstabelle sortieren.
- Der Hinweis beim Start erscheint nur beim Programmstart, nicht nach einem Datenordnerwechsel oder dem Wiederherstellen einer Sicherung.
- Rückfrage beim Umbenennen und Löschsperre einer Tätigkeit nennen Wartungen und Alarme getrennt (z. B. „In 32 Wartungen und 1 Alarm ‚Kettenwechsel‘ durch ‚Kette gewechselt‘ ersetzen?“); bisher zählten Alarme als Wartungen.
- Die Löschsperre eines Equipments zählt Alarme weiter als Wartungseinträge mit („hat 1 Wartung“); eine eigene Formulierung lohnt bei dieser Grenzlage nicht.
- Bei der Umsetzung ergänzt: Die Karteikarte ist in die Bereiche „Alarme“ und „Wartungen“ gegliedert, jeder mit eigener Leiste; die Radauswahl steht darüber und gilt für beide. Die Schaltflächen der Wartungen aus 021 stehen dadurch in ihrem Bereich statt neben der Radauswahl.
- Bei der Umsetzung ergänzt: „Ein-/Ausschalten“ schaltet alle markierten Alarme aus, sobald einer davon eingeschaltet ist; sind alle aus, werden alle eingeschaltet. Die Markierung bleibt danach erhalten.
- Bei der Umsetzung ergänzt: Hat das gewählte Rad keine Alarme, es gibt aber andere, steht „Keine Alarme für dieses Rad.“; der Titel der Karteikarte zählt die fälligen Alarme aller Räder, unabhängig von der Radauswahl.
- Nach Prüfung 2 ergänzt: Markierte fällige und bald fällige Alarme zeigen eine kräftigere Fassung ihrer Farbe statt der Markierungsfarbe des Systems, damit die Farbe auch bei einem einzigen (nach dem Speichern markierten) Alarm sichtbar bleibt.
- Bei der Umsetzung ergänzt: Das Intervall wird ohne Tausenderpunkt eingegeben („3000“), wie die übrigen ganzen Zahlen der App; angezeigt wird es mit Tausenderpunkt.
- Bei der Umsetzung ergänzt: Der Text „32 Wartungen und 1 Alarm“ für Rückfrage und Löschsperre entsteht an einer Stelle (`AuswahllistenViewModel.BeschreibeTaetigkeitsverwendung`).
- Nach 034 auf Wunsch des Benutzers geändert: Die Auswahl „Tätigkeit“ im Alarmfenster ist wie im Wartungsfenster nach der bisherigen Häufigkeit in den Wartungen aller Räder geordnet (`OrdneTaetigkeitenNachHaeufigkeit`, siehe Plan 021); Alarme zählen dabei nicht mit. Antworten A1 „Beide nach Häufigkeit“, B1 „Über alle Räder“.

## Risiken
- Die Karteikarte „Wartung“ teilt sich künftig in zwei Listen; die Alarmliste braucht eine begrenzte Höhe, damit die Wartungstabelle sichtbar bleibt.
- `ZaehleTaetigkeit` ändert seine Signatur; alle Aufrufer und Tests aus 021 müssen mitgezogen werden.
