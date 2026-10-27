# Plan: 013 – Persönliche Daten

## Vorgehen
Neuer Befehl „Extras → Persönliche Daten…“ öffnet ein eigenes Fenster nach dem Muster des Fensters „Equipment“.

- **Core:**
  - Die Eingaben des Fensters sind reine Texte (`Fahrereingaben`).
  - `FahrerEingaben` wandelt einen Sportler in Eingaben um und zurück und prüft dabei alle Grenzen.
  - Unbekannte Werte: Zahlen werden als 0 gespeichert, der Geburtstag als „kein Datum“. Beide erscheinen als leeres Feld.
- **Daten:**
  - `FahrerTabelle.SpeicherePersoenlicheDaten` schreibt die Werte in einer Transaktion in den ersten Sportler.
  - Fehlt noch ein Sportler (leerer Datenordner), wird er dabei über `Standardfahrer` angelegt.
  - Werte, die AndyTours nicht pflegt (Trainingspuls, Koordinaten), bleiben unverändert.
- **App:**
  - Jede Eingabe wird sofort geprüft; ein roter Hinweis nennt den ersten Fehler, „OK“ ist dann gesperrt.
  - „OK“ schreibt zuerst in die Datenbank und ändert erst danach den Sportler im Datenbestand.
  - Schlägt das Schreiben fehl, erscheint die Fehlermeldung und das Fenster bleibt offen.

## Dateien
- `src/AndyTours.Core/Modell/Fahrereingaben.cs` (neu) — Texte der Felder: Vorname, Nachname, Geburtstag, Größe, Gewicht, Ruhepuls, Maximalpuls, Heimatort
- `src/AndyTours.Core/Berechnungen/FahrerEingaben.cs` (neu)
  - `AlsEingaben(fahrer)`: 0 bzw. kein Datum ergibt ein leeres Feld.
  - `AlsFahrer(eingaben, basis)` liefert den neuen Sportler oder den Hinweis zum ersten Fehler.
  - Geprüft wird in dieser Reihenfolge: Geburtstag, Größe, Gewicht, Ruhepuls, Maximalpuls, dann „Ruhepuls < Maximalpuls“.
  - Nummer, Trainingspuls und Koordinaten werden aus `basis` übernommen.
- `src/AndyTours.Daten/FahrerTabelle.cs` (ändern) — `SpeicherePersoenlicheDaten(verbindung, fahrer)`
  - eine Transaktion: `Standardfahrer` liefert bzw. legt den Sportler an, dann UPDATE der gepflegten Spalten
  - liefert die Nummer des Sportlers
- `src/AndyTours.App/ViewModels/PersoenlicheDatenViewModel.cs` (neu) — Textfelder, Hinweis, „OK“ (nur ohne Hinweis), Ereignis `Gespeichert`; Prüfen und Speichern als übergebene Funktionen wie bei `EquipmentBearbeitenViewModel`
- `src/AndyTours.App/Views/PersoenlicheDatenFenster.xaml` (neu) — Felder mit Beschriftung und Einheit (cm, kg, Schläge/min), roter Hinweis, „OK“ und „Abbrechen“
- `src/AndyTours.App/Views/PersoenlicheDatenFenster.xaml.cs` (neu) — schließt nach `Gespeichert`, Fokus ins erste Feld
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.PersoenlicheDaten.cs` (neu)
  - Befehl `PersoenlicheDatenOeffnen`; Grundlage ist der erste Sportler des Bestands oder ein leerer
  - Speichern über `Schreibe`, danach den Sportler im Bestand ersetzen bzw. hinzufügen
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Eintrag „_Persönliche Daten…“ im Menü „Extras“ vor „Einstellungen…“
- `tests/AndyTours.Tests/Berechnungen/FahrerEingabenTests.cs` (neu) — Umwandlung in beide Richtungen, leere Felder, alle Grenzen und Hinweise
- `tests/AndyTours.Tests/Daten/FahrerTests.cs` (neu) — Speichern in vorhandenen Sportler (nicht gepflegte Werte bleiben), Anlegen in leerer Datenbank, erneutes Laden

## Abdeckung der Kriterien
- Wenn der Benutzer „Extras → Persönliche Daten…“ wählt, dann zeigt ein Fenster die gespeicherten Werte: Vorname, Nachname, Geburtstag, Größe in cm, Gewicht in kg, Ruhepuls, Maximalpuls und Heimatort (nur der Name des Orts).
  - Umgesetzt: `Hauptfenster.xaml`, `HauptfensterViewModel.PersoenlicheDaten`, `PersoenlicheDatenFenster`, `FahrerEingaben.AlsEingaben`
  - Geprüft: `FahrerEingabenTests`; manuell – Menü wählen, alle acht Felder sichtbar.
- Wenn das Fenster zum ersten Mal geöffnet wird, dann zeigt es die aus BikeXperience übernommenen Werte.
  - Umgesetzt: Grundlage ist der erste Sportler des Bestands (von der Altdatenübernahme angelegt).
  - Geprüft: manuell – Werte mit der BikeXperience-Anzeige vergleichen.
- Wenn noch keine persönlichen Daten vorhanden sind (neuer, leerer Datenordner), dann sind die Felder leer.
  - Umgesetzt: leerer `Fahrer` als Grundlage, `AlsEingaben` liefert leere Felder.
  - Geprüft: `FahrerEingabenTests`; manuell – neuen Datenordner wählen und Fenster öffnen.
- Wenn der Benutzer Werte ändert und „OK“ wählt, dann sind sie gespeichert und erscheinen auch nach einem Neustart.
  - Umgesetzt: `FahrerTabelle.SpeicherePersoenlicheDaten`, Bestand nach Erfolg ersetzen
  - Geprüft: `FahrerTests`; manuell – ändern, App neu starten, Fenster öffnen.
- Wenn der Benutzer „Abbrechen“ wählt, dann bleibt alles unverändert.
  - Umgesetzt: Das Fenster arbeitet nur auf Texten; geschrieben wird nur bei „OK“.
  - Geprüft: manuell – ändern, „Abbrechen“, erneut öffnen.
- Wenn ein Feld leer gelassen wird, dann gilt der Wert als unbekannt und wird so gespeichert.
  - Umgesetzt: `FahrerEingaben.AlsFahrer` (leer ergibt 0, leerer Text bzw. kein Datum)
  - Geprüft: `FahrerEingabenTests`, `FahrerTests`
- Wenn der Geburtstag kein gültiges Datum im Format TT.MM.JJJJ ist oder in der Zukunft liegt, dann erscheint ein Hinweis, und „OK“ ist nicht wählbar.
  - Umgesetzt: `FahrerEingaben.AlsFahrer` mit `Eingabetext.LeseDatum`, Vergleich mit dem heutigen Tag; „OK“ gesperrt im ViewModel
  - Geprüft: `FahrerEingabenTests` (ungültig, morgen, heute)
- Wenn die Größe keine ganze Zahl ist oder außerhalb von 100 bis 250 cm liegt, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
  - Umgesetzt: `FahrerEingaben.AlsFahrer`
  - Geprüft: `FahrerEingabenTests` (99, 100, 250, 251, „1,5“)
- Wenn das Gewicht keine Zahl mit höchstens einer Nachkommastelle ist (z. B. „78,5“) oder außerhalb von 30 bis 250 kg liegt, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
  - Umgesetzt: `FahrerEingaben.AlsFahrer` (Zahl über `Eingabetext.LeseZahl`, Nachkommastellen geprüft)
  - Geprüft: `FahrerEingabenTests` (29,9, 30, 78,5, 78,55, 250, 250,1, „abc“)
- Wenn der Ruhepuls außerhalb von 30 bis 120 oder der Maximalpuls außerhalb von 100 bis 230 liegt oder keine ganze Zahl ist, dann erscheint ein Hinweis mit dem erlaubten Bereich, und „OK“ ist nicht wählbar.
  - Umgesetzt: `FahrerEingaben.AlsFahrer`
  - Geprüft: `FahrerEingabenTests` (jeweils beide Grenzen und eine Kommazahl)
- Wenn der Ruhepuls nicht kleiner als der Maximalpuls ist, dann erscheint der Hinweis „Der Ruhepuls muss kleiner als der Maximalpuls sein.“, und „OK“ ist nicht wählbar.
  - Umgesetzt: `FahrerEingaben.AlsFahrer` (nur wenn beide angegeben sind)
  - Geprüft: `FahrerEingabenTests` (gleich, größer, einer leer)
- Wenn das Speichern fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, das Fenster bleibt offen, und die gespeicherten Werte bleiben unverändert.
  - Umgesetzt: `Schreibe` meldet den Fehler und liefert `false`. Das ViewModel schließt dann nicht, und der Bestand wird nicht geändert. Das Schreiben läuft in einer Transaktion.
  - Geprüft: Der Fehlerweg (`Schreibe`) ist derselbe wie bei 008/009.

## Entscheidungen
- **Ein Hinweis für alle Felder:** Wie im Fenster „Equipment“ nennt eine rote Zeile unter den Feldern den ersten Fehler. Das ist einheitlich und reicht bei acht Feldern.
- **Unbekannt bleibt 0:** Die Datenbank kennt bei Zahlen schon „0 = unbekannt“ (übernommene Altdaten, leerer Standardsportler). Es gibt daher keine Schemaänderung.
- **Geburtstag großzügig lesen:** Wie in allen Datumsfeldern wird auch „28.5.63“ angenommen; angezeigt wird immer TT.MM.JJJJ.
- **Koordinaten bleiben:** Ändert sich der Name des Heimatorts, bleiben die gespeicherten Koordinaten unverändert. Ihre Pflege gehört zu `streckenkarte`.

## Risiken
- Die übernommenen Altdaten könnten Werte außerhalb der neuen Grenzen enthalten. Das Fenster zeigt sie dann mit Hinweis, und „OK“ ist gesperrt, bis der Wert korrigiert ist. So ist es gewollt, wird aber bei der manuellen Prüfung sichtbar.
