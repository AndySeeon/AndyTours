# Plan: 012 – Mehrfachauswahl zuordnen

## Vorgehen
Drei neue Befehle des Hauptfensters arbeiten auf den markierten Einheiten (`MarkierteEinheiten`). Sie stehen im Menü „Einheit“ und im Kontextmenü der Tabelle.

- **Core:**
  - Den Zustand jeder Kategorie über die markierten Einheiten ermitteln (alle / keine / teilweise).
  - Geänderte Kopien der Einheiten erzeugen (Kategorien setzen bzw. entfernen, Aktivität, Equipment), ohne den Bestand anzufassen.
  - Die Equipment-Auswahl für mehrere Aktivitäten zusammenstellen.
- **Daten:** `TrainingseinheitenTabelle` ändert mehrere Einheiten in einer Transaktion (alle oder keine).
- **App:**
  - Zwei kleine Fenster: „Kategorien zuordnen“ mit dreistufigen Häkchen und „Eintrag wählen“ (für Aktivität und Equipment).
  - Bei mehr als einer Einheit folgt eine Rückfrage mit der Anzahl.
  - Danach: zuerst schreiben, dann den Bestand ersetzen, Tabelle, Summen und Filter neu aufbauen, die geänderten Einheiten wieder markieren und die Statuszeile setzen.

## Dateien
- `src/AndyTours.Core/Berechnungen/ErmittleKategoriezustaende.cs` (neu) — je Kategorie (alphabetisch): `true` (alle markierten Einheiten), `false` (keine) oder `null` (teilweise)
- `src/AndyTours.Core/Berechnungen/OrdneEinheitenZu.cs` (neu)
  - `Kategorien(einheiten, setzen, entfernen)`, `Aktivitaet(einheiten, id)` und `Equipment(einheiten, id?)`
  - Jede Methode liefert geänderte Kopien (Nummer und Anhänge bleiben); die übrigen Werte bleiben unverändert.
- `src/AndyTours.Core/Berechnungen/ErmittleEquipmentauswahl.cs` (ändern) — Überladung für mehrere Aktivitäten: „(kein)“, die nicht ausgemusterten Equipments einer dieser Aktivitäten und die ohne Aktivität, alphabetisch
- `src/AndyTours.Daten/TrainingseinheitenTabelle.cs` (ändern) — `AendereMehrere(verbindung, einheiten)` in einer Transaktion; `Aendern` nutzt denselben inneren Schritt
- `src/AndyTours.App/ViewModels/Kategoriezustand.cs` (neu) — Zeile des Fensters: Id, Name, Häkchen (`bool?`), „teilweise“ nur wählbar, wenn es anfangs teilweise war
- `src/AndyTours.App/ViewModels/KategorienZuordnenViewModel.cs` (neu) — Zeilen, „OK“, Ergebnis: zu setzende und zu entfernende Kategorien
- `src/AndyTours.App/Views/KategorienZuordnenFenster.xaml` (neu) — Liste mit dreistufigen Häkchen, Erklärung, „OK“ und „Abbrechen“
- `src/AndyTours.App/Views/KategorienZuordnenFenster.xaml.cs` (neu) — schließt nach „OK“
- `src/AndyTours.App/ViewModels/EintragWaehlenViewModel.cs` (neu) — Titel, Erklärung, Einträge (`Auswahleintrag`), Vorauswahl, „OK“ nur mit Auswahl, Ergebnis
- `src/AndyTours.App/Views/EintragWaehlenFenster.xaml` (neu) — Liste, „OK“ und „Abbrechen“; Doppelklick wie „OK“
- `src/AndyTours.App/Views/EintragWaehlenFenster.xaml.cs` (neu) — schließt nach „OK“, Doppelklick
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Zuordnen.cs` (neu) — Befehle „Kategorien zuordnen…“, „Aktivität ändern…“, „Equipment ändern…“, Rückfrage, Speichern, Anzeige aktualisieren, Statusmeldung
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Einheiten.cs` (ändern)
  - `MarkierteEinheiten` meldet auch die drei neuen Befehle
  - Neues Ereignis `MarkiereEinheiten` für mehrere Nummern
- `src/AndyTours.App/Views/EinheitenTabelle.xaml` (ändern) — drei Einträge im Kontextmenü
- `src/AndyTours.App/Views/EinheitenTabelle.xaml.cs` (ändern) — `MarkiereEinheiten` behandeln: angezeigte Zeilen wieder markieren
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — drei Einträge im Menü „Einheit“
- `tests/AndyTours.Tests/Berechnungen/ErmittleKategoriezustaendeTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/OrdneEinheitenZuTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/EquipmentauswahlTests.cs` (ändern) — Auswahl für mehrere Aktivitäten
- `tests/AndyTours.Tests/Daten/EinheitSchreibenTests.cs` (ändern) — `AendereMehrere` mit allen oder keinen Änderungen

## Abdeckung der Kriterien
- Wenn der Benutzer eine oder mehrere Einheiten in der Tabelle markiert hat, dann kann er im Menü „Einheit“ und im Kontextmenü der Tabelle „Kategorien zuordnen…“, „Aktivität ändern…“ und „Equipment ändern…“ wählen.
  - Umgesetzt: `Hauptfenster.xaml`, `EinheitenTabelle.xaml`, Befehle in `HauptfensterViewModel.Zuordnen`
  - Geprüft: manuell – mehrere Zeilen markieren, beide Menüs öffnen.
- Wenn keine Einheit markiert ist, dann sind diese Befehle nicht wählbar.
  - Umgesetzt: CanExecute `MindestensEineMarkiert`; Benachrichtigung über `MarkierteEinheiten`
  - Geprüft: manuell – Auswahl der Werkzeugleiste ohne Einheiten, Menü grau.
- Wenn der Benutzer „Kategorien zuordnen…“ wählt, dann zeigt ein Fenster alle Kategorien: angehakt, wenn alle markierten Einheiten sie haben, leer, wenn keine sie hat, und teilweise angehakt, wenn nur manche sie haben.
  - Umgesetzt: `ErmittleKategoriezustaende`, `KategorienZuordnenViewModel`, `KategorienZuordnenFenster`
  - Geprüft: `ErmittleKategoriezustaendeTests`; manuell – zwei Einheiten mit verschiedenen Kategorien markieren.
- Wenn der Benutzer im Fenster eine Kategorie anhakt bzw. leert und „OK“ wählt, dann haben alle markierten Einheiten diese Kategorie bzw. keine von ihnen hat sie mehr; eine Kategorie, die auf „teilweise“ stehen bleibt, ändert sich bei keiner Einheit.
  - Umgesetzt: Ergebnis des ViewModels (nur Zeilen, deren Zustand sich gegenüber dem Anfang geändert hat), `OrdneEinheitenZu.Kategorien`, `AendereMehrere`
  - Geprüft: `OrdneEinheitenZuTests`, `EinheitSchreibenTests`; manuell – eine teilweise Kategorie anhaken, eine leeren, eine teilweise lassen.
- Wenn der Benutzer „Aktivität ändern…“ wählt, eine Aktivität auswählt und „OK“ wählt, dann gehören alle markierten Einheiten zu dieser Aktivität.
  - Umgesetzt: `EintragWaehlenFenster` mit den Aktivitäten, `OrdneEinheitenZu.Aktivitaet`
  - Geprüft: `OrdneEinheitenZuTests`; manuell – zwei Bike-Einheiten auf „Laufen“ ändern.
- Wenn die Aktivität geändert wird, dann behalten die Einheiten ihr Equipment, auch wenn es nicht zur neuen Aktivität passt.
  - Umgesetzt: `OrdneEinheitenZu.Aktivitaet` ändert nur die Aktivität
  - Geprüft: `OrdneEinheitenZuTests`; manuell – Equipment-Spalte nach dem Wechsel unverändert.
- Wenn der Benutzer „Equipment ändern…“ wählt, dann bietet das Fenster „(kein)“ und alle nicht ausgemusterten Equipments an, die zu einer der Aktivitäten der markierten Einheiten passen oder keine Aktivität haben.
  - Umgesetzt: Überladung von `ErmittleEquipmentauswahl`, `EintragWaehlenFenster`
  - Geprüft: `EquipmentauswahlTests`; manuell – Bike- und Laufen-Einheit markieren, Räder und Laufschuhe ohne ausgemusterte.
- Wenn der Benutzer ein Equipment auswählt und „OK“ wählt, dann haben alle markierten Einheiten dieses Equipment bzw. keines; ihre Kategorien bleiben unverändert.
  - Umgesetzt: `OrdneEinheitenZu.Equipment`
  - Geprüft: `OrdneEinheitenZuTests`; manuell – „SL Road SL“ zuordnen, „Arbeit“ wird nicht angehakt.
- Wenn mehr als eine Einheit markiert ist und der Benutzer „OK“ wählt, dann fragt die App mit der Anzahl nach, z. B. „42 Einheiten ändern?“; bei „Nein“ bleibt alles unverändert.
  - Umgesetzt: Rückfrage in `HauptfensterViewModel.Zuordnen` vor dem Schreiben, vorbelegt mit „Nein“
  - Geprüft: manuell – mit „Nein“ und „Ja“.
- Wenn der Benutzer im Fenster „Abbrechen“ wählt, dann bleibt alles unverändert.
  - Umgesetzt: Die Fenster liefern dann kein Ergebnis.
  - Geprüft: manuell – in allen drei Fenstern.
- Wenn zugeordnet wurde, dann zeigen Tabelle, Summen und Filter sofort den neuen Stand, und die Einheiten bleiben markiert, soweit sie noch angezeigt werden.
  - Umgesetzt: `AktualisiereSummen` und Jahre (wie `ZeigeAenderung`), danach Ereignis `MarkiereEinheiten`
  - Geprüft: manuell – Kategorie-Spalte und Filterliste sofort aktuell, Zeilen markiert.
- Wenn geänderte Einheiten danach nicht mehr zur Auswahl der Werkzeugleiste oder zum Filter passen, dann nennt die Statuszeile ihre Anzahl, z. B. „3 geänderte Einheiten liegen außerhalb der aktuellen Auswahl.“
  - Umgesetzt: geänderte Nummern, die nicht mehr in `Einheitszeilen` stehen, zählen; `Statusmeldung` (Einzahl: „1 geänderte Einheit liegt außerhalb der aktuellen Auswahl.“)
  - Geprüft: manuell – bei Werkzeugleiste „Bike“ Einheiten auf „Laufen“ ändern.
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und keine der markierten Einheiten ist geändert.
  - Umgesetzt: `AendereMehrere` in einer Transaktion über `Schreibe`; der Bestand wird erst nach Erfolg ersetzt (Kopien aus `OrdneEinheitenZu`).
  - Geprüft: `EinheitSchreibenTests` (eine fehlende Einheit lässt alle unverändert); der Fehlerweg (`Schreibe`) ist derselbe wie bei 008/009.

## Entscheidungen
- **Fenster für Aktivität und Equipment:** Ein gemeinsames Fenster „Eintrag wählen“ mit Liste statt zweier eigener Fenster; es ähnelt dem Fenster „Zusammenführen“, liefert aber eine Nummer statt eines Namens, weil „(kein)“ keine Nummer hat.
- **Vorauswahl:** Haben alle markierten Einheiten dieselbe Aktivität bzw. dasselbe Equipment, ist es vorgewählt, sonst nichts.
- **„Teilweise“ nur am Anfang:** Ein Häkchen lässt sich nur dann wieder auf „teilweise“ stellen, wenn es anfangs so stand; sonst wechselt es nur zwischen an und aus.
- **Keine Änderung, keine Rückfrage:** Ändert „OK“ im Kategorienfenster nichts, wird ohne Rückfrage und ohne Speichern geschlossen.
- **Gezählt wird, was markiert ist:** Die Rückfrage nennt die Zahl der markierten Einheiten, auch wenn einzelne den neuen Wert schon haben. Geschrieben werden alle markierten.

## Risiken
- Die Tabelle meldet Markierungsänderungen beim Neuaufbau selbst an das ViewModel. Das erneute Markieren mehrerer Zeilen muss nach dem Neuaufbau geschehen, sonst überschreibt die leere Auswahl die Liste.
