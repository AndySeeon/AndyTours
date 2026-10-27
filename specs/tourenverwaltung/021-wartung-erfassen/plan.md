# Plan: 021 – Wartung erfassen

## Vorgehen
Wartungen sind schon Teil des Datenbestands (Tabelle `Wartung`, aus BikeXperience übernommen); bisher werden sie nur geladen und beim Umwandeln gespeichert. Die Tabelle bekommt Einfügen, Ändern, Löschen und das Ersetzen einer Tätigkeit, wie bei den Einheiten.

Fachfunktionen im Core liefern:
- die Zeilen der Wartungstabelle: Datum, Rad, km-Stand, Tätigkeit, Kommentar, gefiltert nach Rad, die neueste oben; nur Einträge vom Typ „Wartung“ (Alarme kommen mit `wartungsalarme`),
- den km-Stand eines Rads an einem Tag,
- die Prüfung der Eingaben des Wartungsfensters (Pflichtfelder, gültiges Datum, kein Datum in der Zukunft) und die Wartung daraus,
- die Auswahl der Räder für das Wartungsfenster (nicht ausgemustert, beim Bearbeiten zusätzlich das bisherige Rad),
- für die Liste „Tätigkeit“: die anfängliche Liste aus den vorhandenen Wartungen, das Zählen und das Umbenennen einer Tätigkeit in den Wartungen.

Die Liste „Tätigkeit“ ist eine weitere Auswahlliste mit festen Werten, gespeichert wie die Trainingsbereiche. Beim Laden des Datenordners wird sie wie diese beim ersten Mal aus dem Bestand gebildet. Im Einstellungsfenster erscheint sie unter „Auswahllisten“ mit Hinzufügen, Umbenennen, Löschen und Verschieben. Die Rückfrage beim Umbenennen und die Löschsperre nennen Wartungen statt Einheiten.

Die Karteikarte „Wartung“ ersetzt den Platzhalter durch eine eigene Ansicht: oben die Radauswahl und die Schaltflächen „Neue Wartung…“, „Bearbeiten…“ und „Löschen“, darunter die Tabelle mit Kontextmenü, Doppelklick und Entf. Das Hauptfenster-ViewModel hält Zeilen, Radauswahl und Markierung und baut die Zeilen neu auf, wenn sich Wartungen, Einheiten oder Räder ändern; der km-Stand bleibt so aktuell. Das Wartungsfenster ist modal, wie das Bearbeitungsfenster einer Einheit.

## Dateien
- `src/AndyTours.Core/Modell/Auswahlliste.cs` (ändern) — neuer Wert `Taetigkeit`
- `src/AndyTours.Core/Modell/Wartungszeile.cs` (neu) — eine Zeile der Wartungstabelle: Id, Datum, Rad, km-Stand, Tätigkeit, Kommentar
- `src/AndyTours.Core/Modell/Wartungseingaben.cs` (neu) — die Eingaben des Wartungsfensters als Text bzw. Auswahl
- `src/AndyTours.Core/Modell/Wartungsfeld.cs` (neu) — die prüfbaren Felder: Datum, Rad, Tätigkeit
- `src/AndyTours.Core/Modell/Wartungsergebnis.cs` (neu) — Ergebnis der Prüfung: Wartung oder fehlerhafte Felder samt Hinweis
- `src/AndyTours.Core/Berechnungen/BerechneKmStand.cs` (neu) — Anfangskilometer des Rads plus Summe der Distanzen aller seiner Einheiten bis einschließlich zu einem Tag
- `src/AndyTours.Core/Berechnungen/ErstelleWartungszeilen.cs` (neu) — Zeilen der Tabelle, gefiltert nach Rad, die neueste oben
- `src/AndyTours.Core/Berechnungen/WartungEingaben.cs` (neu) — Eingaben aus einer Wartung und Wartung aus den Eingaben (mit Prüfung)
- `src/AndyTours.Core/Berechnungen/ErmittleWartungsraeder.cs` (neu) — Räder für das Wartungsfenster
- `src/AndyTours.Core/Berechnungen/ErmittleTaetigkeiten.cs` (neu) — anfängliche Liste „Tätigkeit“ aus den Wartungen, alphabetisch
- `src/AndyTours.Core/Berechnungen/ZaehleTaetigkeit.cs` (neu) — Anzahl der Wartungen mit einer Tätigkeit
- `src/AndyTours.Core/Berechnungen/BenenneTaetigkeitUm.cs` (neu) — ersetzt eine Tätigkeit in den Wartungen des Bestands
- `src/AndyTours.Daten/WartungenTabelle.cs` (ändern) — Einfügen, Ändern, Löschen mehrerer, Tätigkeit ersetzen
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Wartungen.cs` (neu) — Zeilen, Radauswahl, Markierung, Befehle „Neue Wartung…“, „Bearbeiten…“, „Löschen“, Speichern mit Fehlermeldung
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — Liste „Tätigkeit“ beim Laden lesen bzw. bilden; Wartungszeilen beim Laden und nach jeder Änderung neu aufbauen
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Auswahllisten.cs` (ändern) — Liste „Tätigkeit“: Lesen, Zählen, Löschsperre, Hinzufügen, Umbenennen samt Wartungen, Löschen, Verschieben
- `src/AndyTours.App/ViewModels/AuswahllistenViewModel.cs` (ändern) — Liste „Tätigkeit“ in der Auswahl, verschiebbar, Rückfrage beim Umbenennen mit „Wartungen“
- `src/AndyTours.App/ViewModels/IAuswahllistenSpeicher.cs` (ändern) — Beschreibung von `ZaehleVerwendung`: bei Tätigkeiten zählt der erste Wert die Wartungen (bei der Umsetzung ergänzt)
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Equipments.cs` (ändern) — nach dem Löschen eines Rads die Radauswahl der Karteikarte „Wartung“ neu aufbauen (bei der Umsetzung ergänzt)
- `src/AndyTours.App/ViewModels/WartungBearbeitenViewModel.cs` (neu) — Felder, Auswahl von Rad und Tätigkeit, Fehlermarkierung, Hinweise, „OK“ nur ohne Fehler
- `src/AndyTours.App/Views/WartungBearbeitenFenster.xaml` (neu) — Fenster mit Datum, Rad, Tätigkeit, Kommentar, Hinweis, „OK“ und „Abbrechen“
- `src/AndyTours.App/Views/WartungBearbeitenFenster.xaml.cs` (neu) — Code-Behind
- `src/AndyTours.App/Views/WartungenAnsicht.xaml` (neu) — Radauswahl, Schaltflächen, Tabelle mit Kontextmenü und Hinweis ohne Wartungen
- `src/AndyTours.App/Views/WartungenAnsicht.xaml.cs` (neu) — Markierung an das ViewModel, Doppelklick, Entf
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Karteikarte „Wartung“ zeigt die neue Ansicht statt des Platzhalters
- `tests/AndyTours.Tests/Berechnungen/BerechneKmStandTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErstelleWartungszeilenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/WartungEingabenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErmittleWartungsraederTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ZaehleUndBenenneTaetigkeitTests.cs` (neu) — auch `ErmittleTaetigkeiten`
- `tests/AndyTours.Tests/Daten/WartungenTests.cs` (neu) — Einfügen, Ändern, Löschen, Tätigkeit ersetzen, Liste „Tätigkeit“ speichern und laden

## Abdeckung der Kriterien
Testbefehl: `dotnet test AndyTours.slnx`

### Anzeigen
- **Kriterium:** Wenn der Benutzer die Karteikarte „Wartung“ wählt, dann zeigt sie eine Tabelle aller Wartungen mit Datum, Rad, km-Stand, Tätigkeit und Kommentar, die neueste oben.
  - Umsetzung: `ErstelleWartungszeilen`, `HauptfensterViewModel.Wartungen`, `WartungenAnsicht`, `Hauptfenster.xaml`
  - Prüfung: `ErstelleWartungszeilenTests` (Reihenfolge, Spalten)
  - Manuell: Karteikarte „Wartung“ öffnen; die 86 übernommenen Wartungen erscheinen, die vom 06.09.2026 oben.
- **Kriterium:** Wenn eine Wartung angezeigt wird, dann steht unter „km-Stand“ die Summe der Distanzen aller Einheiten mit diesem Rad bis einschließlich zum Tag der Wartung, zuzüglich der Anfangskilometer des Rads.
  - Umsetzung: `BerechneKmStand`, `ErstelleWartungszeilen`
  - Prüfung: `BerechneKmStandTests` (Anfangskilometer und Einheiten am selben Tag zählen mit, spätere und andere Räder nicht)
- **Kriterium:** Wenn der Benutzer oben ein Rad wählt, dann zeigt die Tabelle nur dessen Wartungen; mit „Alle Räder“ erscheinen wieder alle.
  - Umsetzung: `ErstelleWartungszeilen` (Filter), `HauptfensterViewModel.Wartungen`, `WartungenAnsicht`
  - Prüfung: `ErstelleWartungszeilenTests`
  - Manuell: Rad wählen, dann „Alle Räder“.
- **Kriterium:** Wenn der Benutzer auf einen Spaltenkopf klickt, dann wird nach dieser Spalte sortiert; ein zweiter Klick kehrt die Reihenfolge um.
  - Umsetzung: `WartungenAnsicht` (Sortierung der Tabelle nach Datum, Zahl bzw. Text)
  - Manuell: nach „km-Stand“ und „Tätigkeit“ sortieren, je zweimal klicken.
- **Kriterium:** Wenn es noch keine Wartung gibt, dann steht statt der Tabelle „Noch keine Wartungen erfasst.“
  - Umsetzung: `HauptfensterViewModel.Wartungen`, `WartungenAnsicht`
  - Manuell: in einem leeren Datenordner die Karteikarte öffnen.

### Erfassen, ändern, löschen
- **Kriterium:** Wenn der Benutzer über der Tabelle oder im Kontextmenü der Tabelle „Neue Wartung…“ wählt, dann öffnet sich ein Fenster mit Datum (vorbelegt mit heute), Rad, Tätigkeit und Kommentar.
  - Umsetzung: `HauptfensterViewModel.Wartungen`, `WartungBearbeitenViewModel`, `WartungBearbeitenFenster`, `WartungenAnsicht`
  - Manuell: beide Wege; das Datum ist heute.
- **Kriterium:** Wenn oben ein Rad gewählt ist und der Benutzer eine neue Wartung anlegt, dann ist dieses Rad vorgewählt.
  - Umsetzung: `HauptfensterViewModel.Wartungen`, `WartungBearbeitenViewModel`
  - Manuell: Rad wählen, „Neue Wartung…“.
- **Kriterium:** Wenn der Benutzer im Wartungsfenster die Tätigkeit wählt, dann bietet die Auswahl die Einträge der Liste „Tätigkeit“ in deren Reihenfolge an; andere Tätigkeiten lassen sich dort nicht eingeben.
  - Umsetzung: `WartungBearbeitenViewModel`, `WartungBearbeitenFenster` (Auswahl ohne freie Eingabe)
  - Manuell: Auswahl öffnen; Reihenfolge wie in den Einstellungen, Tippen ergibt keinen neuen Eintrag.
- **Kriterium:** Wenn der Benutzer mit „OK“ bestätigt, dann erscheint die Wartung sofort in der Tabelle und bleibt nach einem Neustart erhalten.
  - Umsetzung: `WartungEingaben`, `WartungenTabelle.Einfuegen`/`Aendern`, `HauptfensterViewModel.Wartungen`
  - Prüfung: `WartungEingabenTests`, `WartungenTests` (Einfügen und wieder laden)
  - Manuell: Wartung anlegen, App neu starten.
- **Kriterium:** Wenn der Benutzer doppelt auf eine Wartung klickt oder „Bearbeiten…“ wählt, dann öffnet sich dasselbe Fenster mit ihren Werten; Änderungen erscheinen nach „OK“ in der Tabelle, „Abbrechen“ verwirft sie.
  - Umsetzung: `WartungEingaben.AlsEingaben`, `WartungBearbeitenViewModel`, `WartungenAnsicht`, `HauptfensterViewModel.Wartungen`
  - Prüfung: `WartungEingabenTests` (Hin- und Rückweg), `WartungenTests` (Ändern)
  - Manuell: Doppelklick, ändern, „OK“; erneut ändern, „Abbrechen“.
- **Kriterium:** Wenn der Benutzer eine oder mehrere Wartungen markiert und „Löschen“ wählt oder Entf drückt, dann erscheint eine Rückfrage mit der Anzahl, und erst nach „Ja“ werden sie gelöscht.
  - Umsetzung: `HauptfensterViewModel.Wartungen`, `WartungenTabelle.Loeschen`, `WartungenAnsicht` (Entf)
  - Prüfung: `WartungenTests` (Löschen mehrerer)
  - Manuell: zwei Wartungen markieren, Entf, erst „Nein“, dann „Ja“.

### Liste der Tätigkeiten
- **Kriterium:** Wenn der Benutzer in den Einstellungen unter „Auswahllisten“ die Liste „Tätigkeit“ wählt, dann sieht er deren Einträge in der festgelegten Reihenfolge.
  - Umsetzung: `Auswahlliste`, `AuswahllistenViewModel`, `HauptfensterViewModel.Auswahllisten`
  - Manuell: Einstellungen → Auswahllisten → „Tätigkeit“.
- **Kriterium:** Wenn die App zum ersten Mal mit dieser Funktion gestartet wird, dann enthält die Liste „Tätigkeit“ alle Tätigkeiten der vorhandenen Wartungen, alphabetisch geordnet.
  - Umsetzung: `ErmittleTaetigkeiten`, `HauptfensterViewModel` (Laden)
  - Prüfung: `ZaehleUndBenenneTaetigkeitTests`, `WartungenTests` (Liste speichern und laden)
  - Manuell: erster Start; die 21 Tätigkeiten stehen alphabetisch in der Liste.
- **Kriterium:** Wenn der Benutzer in der Liste „Tätigkeit“ einen Eintrag hinzufügt, verschiebt oder einen leeren oder doppelten Eintrag erfassen will, dann verhält sie sich wie die Liste „Trainingsbereich“.
  - Umsetzung: `AuswahllistenViewModel` (gemeinsame Prüfung und Befehle), `HauptfensterViewModel.Auswahllisten`
  - Prüfung: vorhandene Tests der gemeinsamen Prüfung (`PruefeAuswahleintragTests`, `VerschiebeEintragTests`)
  - Manuell: Eintrag hinzufügen, verschieben, leeren und doppelten Eintrag versuchen.
- **Kriterium:** Wenn der Benutzer eine Tätigkeit umbenennt, die in Wartungen vorkommt, dann fragt die App mit der Anzahl („In 32 Wartungen ‚Kettenwechsel‘ durch ‚Kette gewechselt‘ ersetzen?“) und ändert nach „Ja“ die Liste und alle diese Wartungen; bei „Nein“ bleibt alles unverändert.
  - Umsetzung: `ZaehleTaetigkeit`, `BenenneTaetigkeitUm`, `WartungenTabelle.ErsetzeTaetigkeit`, `AuswahllistenViewModel`, `HauptfensterViewModel.Auswahllisten`
  - Prüfung: `ZaehleUndBenenneTaetigkeitTests`, `WartungenTests` (Tätigkeit ersetzen)
  - Manuell: „Kettenwechsel“ umbenennen, erst „Nein“, dann „Ja“.
- **Kriterium:** Wenn der Benutzer eine Tätigkeit löschen will, die in Wartungen vorkommt, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl dieser Wartungen.
  - Umsetzung: `ZaehleTaetigkeit`, `HauptfensterViewModel.Auswahllisten` (Löschsperre)
  - Prüfung: `ZaehleUndBenenneTaetigkeitTests`
  - Manuell: „Kettenwechsel“ löschen.
- **Kriterium:** Wenn die Liste „Tätigkeit“ geändert wurde, dann bieten danach geöffnete Wartungsfenster den neuen Stand an, und umbenannte Tätigkeiten erscheinen sofort in der Tabelle.
  - Umsetzung: `HauptfensterViewModel.Auswahllisten` (Zeilen neu aufbauen), `HauptfensterViewModel.Wartungen` (Fenster bekommt die aktuelle Liste)
  - Manuell: umbenennen, Tabelle und neues Wartungsfenster ansehen.

### Grenz- und Fehlerfälle
- **Kriterium:** Wenn im Wartungsfenster kein Rad oder keine Tätigkeit gewählt ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
  - Umsetzung: `WartungEingaben`, `WartungBearbeitenViewModel`, `WartungBearbeitenFenster`
  - Prüfung: `WartungEingabenTests`
- **Kriterium:** Wenn das Datum ungültig ist, dann ist „OK“ nicht möglich, und das Feld ist markiert.
  - Umsetzung: `WartungEingaben` (über `Eingabetext.LeseDatum`), `WartungBearbeitenViewModel`
  - Prüfung: `WartungEingabenTests`
- **Kriterium:** Wenn das Datum in der Zukunft liegt, dann ist „OK“ nicht möglich, und es erscheint „Das Datum liegt in der Zukunft.“
  - Umsetzung: `WartungEingaben` (Vergleich mit dem übergebenen heutigen Tag), `WartungBearbeitenViewModel`
  - Prüfung: `WartungEingabenTests` (heute erlaubt, morgen nicht)
- **Kriterium:** Wenn die Liste „Tätigkeit“ leer ist, dann weist das Wartungsfenster darauf hin, dass zuerst in den Einstellungen Tätigkeiten angelegt werden müssen.
  - Umsetzung: `WartungBearbeitenViewModel`, `WartungBearbeitenFenster`
  - Manuell: in einem leeren Datenordner „Neue Wartung…“.
- **Kriterium:** Wenn ein Rad ausgemustert ist, dann wird es für neue Wartungen nicht angeboten; seine Wartungen bleiben sichtbar und lassen sich weiter bearbeiten.
  - Umsetzung: `ErmittleWartungsraeder`, `ErstelleWartungszeilen` (zeigt alle Räder)
  - Prüfung: `ErmittleWartungsraederTests`, `ErstelleWartungszeilenTests`
- **Kriterium:** Wenn keine oder mehrere Wartungen markiert sind, dann ist „Bearbeiten…“ nicht wählbar; ohne Markierung ist auch „Löschen“ nicht wählbar.
  - Umsetzung: `HauptfensterViewModel.Wartungen` (Wählbarkeit), `WartungenAnsicht`
  - Manuell: keine, eine und zwei Wartungen markieren.
- **Kriterium:** Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Tabelle und Liste bleiben beim vorherigen Stand.
  - Umsetzung: `HauptfensterViewModel.Wartungen` und `HauptfensterViewModel.Auswahllisten` (erst schreiben, dann den Bestand ändern, über das vorhandene `Schreibe`)
  - Manuell: nicht erneut nötig; dieselbe Fehlerbehandlung ist bei den Einheiten und Auswahllisten geprüft.

## Entscheidungen
- Die Tabelle zeigt nur Einträge vom Typ „Wartung“; Alarme bleiben unberührt, bis `wartungsalarme` sie anzeigt.
- Die Zeilen werden nach jeder Änderung an Einheiten, Rädern, Wartungen oder der Liste „Tätigkeit“ neu aufgebaut. Bei rund 100 Wartungen und 1.000 Einheiten ist das unmerklich und hält den km-Stand immer aktuell.
- „Neueste oben“: nach Datum absteigend, bei gleichem Datum die zuletzt erfasste oben.
- Das Datum wird wie bei den Einheiten als Text eingegeben (z. B. „06.09.2026“).
- Die Schaltflächen stehen über der Tabelle; das Menü „Einheit“ bleibt unverändert (W1a).
- Bei der Umsetzung ergänzt: Hat das oben gewählte Rad keine Wartungen, es gibt aber andere, steht „Keine Wartungen für dieses Rad.“ statt der Tabelle.
- Bei der Umsetzung ergänzt: Wird eine Wartung für ein anderes Rad gespeichert als das oben gewählte, wechselt die Auswahl auf „Alle Räder“, damit die Wartung sofort sichtbar ist; sie wird danach markiert.
- Bei der Umsetzung ergänzt: Steht in einer vorhandenen Wartung eine Tätigkeit nur in anderer Groß-/Kleinschreibung in der Liste, wird der Listeneintrag gewählt; fehlt sie ganz, bleibt sie im Wartungsfenster als erster Eintrag wählbar (wie ein ausgemustertes Rad).
- Auf Wunsch des Benutzers ergänzt: Solange die Karteikarte „Wartung“ gewählt ist, nennt die Statusleiste die Anzahl der Wartungen in der Datenbank (z. B. „86 Wartungen in der Datenbank“) statt der Einheiten.
- Auf Wunsch des Benutzers ergänzt: In der Radauswahl über der Tabelle und im Wartungsfenster stehen ausgemusterte Räder am Ende der Liste, mit dem Zusatz „(ausgemustert)“; die Spalte „Rad“ der Tabelle zeigt weiter nur den Namen.
- Nach 034 auf Wunsch des Benutzers geändert: Die Auswahl „Tätigkeit“ bietet die Einträge der Liste „Tätigkeit“ nach ihrer bisherigen Häufigkeit in den durchgeführten Wartungen aller Räder an, die häufigste zuerst, bei Gleichstand (auch bei nie verwendeten) in der Reihenfolge der Liste; freie Eingabe gibt es nicht. Umgesetzt in `Core/Berechnungen/OrdneTaetigkeitenNachHaeufigkeit.cs` (Tests `OrdneTaetigkeitenNachHaeufigkeitTests`), gezählt wie `ZaehleTaetigkeit` (ohne Groß-/Kleinschreibung, ohne Randleerzeichen). Antworten: A1 „Beide nach Häufigkeit“ (Wartungs- und Alarmfenster gleich), B1 „Über alle Räder“ (nicht je Rad).

## Risiken
- Das Einstellungsfenster zeigt die Rückfrage beim Umbenennen bisher nur für Trainingsbereiche mit „Einheiten“. Die Erweiterung auf „Wartungen“ darf die vorhandenen Listen nicht verändern; die vorhandenen Tests der Auswahllisten laufen weiter mit.
