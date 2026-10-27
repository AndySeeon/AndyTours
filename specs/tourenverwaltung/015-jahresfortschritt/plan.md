# Plan: 015 – Jahresfortschritt

## Vorgehen
Die Karteikarte „Aktuell“ ersetzt ihren Platzhalter durch eine eigene Ansicht: Kennzahlen des laufenden Jahres oben, darunter ein Liniendiagramm der Jahressummen.

- **Core:**
  - `ErmittleJahresfortschritt` berechnet aus dem Datenbestand alle Daten der Ansicht für einen gewählten Wert (km, Anzahl, hm, Fahrzeit):
    - die Jahressummen aller Jahre, mit einem Ziel je Jahr, falls vorhanden
    - die Kennzahlen des laufenden Jahres: Stand, Ziel, Prozent, Soll-Stand, Vorsprung bzw. Rückstand, Rest und Wochenleistung
  - Einbezogen werden Aktivität (Werkzeugleiste) und Kategorie (Filterleiste). Die Ziele kommen aus `ErmittleSaisonziel` und werden bei „Alle Aktivitäten“ addiert.
  - `BeschreibeJahresfortschritt` macht daraus die Texte über dem Diagramm.
- **App:**
  - `JahresfortschrittViewModel` hält den gezeigten Wert, die Diagrammdaten und die Texte; ein Befehl schaltet den Wert reihum weiter.
  - Das Hauptfenster-ViewModel aktualisiert es überall dort, wo heute schon die Tabelle neu aufgebaut wird (`AktualisiereSummen`), sowie nach dem Speichern der Saisonziele.
  - Das Diagramm (Linie mit Fläche, Punkte, gestrichelte Ziellinie, Soll-Markierung, Klick) entsteht mit einer Diagrammbibliothek, die noch festzulegen ist.

## Dateien
- `src/AndyTours.Core/Modell/Fortschrittswert.cs` (neu) — Aufzählung Kilometer, Anzahl, Hoehenmeter, Fahrzeit (in dieser Umschaltreihenfolge)
- `src/AndyTours.Core/Modell/Jahrespunkt.cs` (neu) — Jahr, Summe, Ziel (oder keines) für einen Wert
- `src/AndyTours.Core/Modell/Jahreskennzahlen.cs` (neu) — Stand, Ziel, Anteil, Soll-Stand, Differenz, Rest, Wochenleistung des laufenden Jahres
- `src/AndyTours.Core/Modell/Jahresfortschritt.cs` (neu) — Wert, Jahrespunkte, Kennzahlen
- `src/AndyTours.Core/Berechnungen/ErmittleJahresfortschritt.cs` (neu) — `Ausfuehren(bestand, aktivitaetId, kategorie, wert, heute)`
  - Auswahl: Aktivität (oder alle) über `FiltereAuswahl`, Kategorie über `FiltereEinheiten` mit sonst leerem Filter
  - Summen: Jahressummen über `BerechneJahressummen`; das laufende Jahr ist immer enthalten, sobald es Einheiten gibt
  - Ziele: je Jahr die Summe der wirksamen Ziele (`ErmittleSaisonziel`) der betroffenen Aktivitäten; 0 = kein Ziel; Fahrzeit nie mit Ziel
  - Soll-Stand: Ziel × Tag des Jahres / Tage des Jahres
  - Wochenleistung: Rest / ((Tage des Jahres − Tag des Jahres + 1) / 7)
- `src/AndyTours.Core/Berechnungen/BeschreibeJahresfortschritt.cs` (neu) — Überschrift („Kilometer (Summe Entfernung)“ usw.) und Kennzahltexte („noch 2.345 km – 112,4 km pro Woche“, „Ziel erreicht – 120 km darüber“, „kein Saisonziel – unter Extras → Saisonziele… festlegen“); Fahrzeit als Stunden:Minuten
- `src/AndyTours.App/ViewModels/JahresfortschrittViewModel.cs` (neu) — Wert, Überschrift, Kennzahltexte, Diagrammdaten, `HatEinheiten`, Befehl `WertWechseln`, `Aktualisiere(bestand, aktivitaetId, kategorie, heute)`
- `src/AndyTours.App/Views/AktuellAnsicht.xaml` (neu) — Kennzahlen oben, Diagramm darunter, Hinweis „Keine Einheiten in dieser Auswahl.“ statt des Diagramms (hängt ab von: Welche Diagrammbibliothek?)
- `src/AndyTours.App/Views/AktuellAnsicht.xaml.cs` (neu) — überträgt die Diagrammdaten in das Diagramm, Klick → `WertWechseln` (hängt ab von: Welche Diagrammbibliothek?)
- `src/AndyTours.App/AndyTours.App.csproj` (ändern) — Paketverweis der Diagrammbibliothek (hängt ab von: Welche Diagrammbibliothek?)
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — Eigenschaft `Jahresfortschritt`; am Ende von `AktualisiereSummen` aktualisieren (Aktivität aus der Werkzeugleiste, Kategorie aus `Filter`)
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Saisonziele.cs` (ändern) — nach dem Speichern `Jahresfortschritt` aktualisieren
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Karteikarte „Aktuell“: Platzhalter durch `AktuellAnsicht` ersetzen
- `tests/AndyTours.Tests/Berechnungen/ErmittleJahresfortschrittTests.cs` (neu) — Jahre, Summen je Wert, Aktivität/Kategorie, Ziele (einzeln, addiert, keines, Fahrzeit), Soll-Stand, Wochenleistung, Ziel erreicht, laufendes Jahr ohne Einheiten
- `tests/AndyTours.Tests/Berechnungen/BeschreibeJahresfortschrittTests.cs` (neu) — Überschriften, Kennzahltexte

## Abdeckung der Kriterien
- Wenn der Benutzer die Karteikarte „Aktuell“ öffnet, dann zeigt sie ein Liniendiagramm mit ausgefüllter Fläche unter der Linie: waagrecht alle Jahre mit erfassten Einheiten (das laufende Jahr ganz rechts), senkrecht die Jahressumme des gezeigten Werts, mit einem Punkt je Jahr.
  - Umgesetzt: `ErmittleJahresfortschritt` (Jahrespunkte), `AktuellAnsicht`
  - Geprüft: `ErmittleJahresfortschrittTests`; manuell – Karteikarte öffnen, alle Jahre 2016 bis 2026 mit Fläche.
- Wenn der Benutzer in das Diagramm klickt, dann wechselt es von Kilometern (Summe Entfernung) zu Anzahl Einheiten, dann zu Höhenmetern (Summe Höhenmeter), dann zu Fahrzeit und wieder zu Kilometern; die Überschrift nennt den gezeigten Wert.
  - Umgesetzt: `JahresfortschrittViewModel.WertWechseln`, `BeschreibeJahresfortschritt` (Überschrift), Klick in `AktuellAnsicht`
  - Geprüft: `BeschreibeJahresfortschrittTests`; manuell – viermal klicken.
- Wenn Jahre ein Saisonziel für den gezeigten Wert haben, dann zeigt das Diagramm es als gestrichelte Ziellinie über diesen Jahren, und im laufenden Jahr ist zusätzlich der Soll-Stand zum heutigen Tag markiert.
  - Umgesetzt: Ziele je Jahrespunkt und Soll-Stand in `ErmittleJahresfortschritt`; Darstellung in `AktuellAnsicht` (hängt ab von: Welche Diagrammbibliothek?)
  - Geprüft: `ErmittleJahresfortschrittTests`; manuell – gestrichelte Linie, Soll-Markierung 2026.
- Wenn in der Werkzeugleiste eine Aktivität gewählt ist, dann enthält das Diagramm nur deren Einheiten und Ziele; bei „Alle Aktivitäten“ werden Einheiten und Ziele aller Aktivitäten zusammengezählt.
  - Umgesetzt: `ErmittleJahresfortschritt`
  - Geprüft: `ErmittleJahresfortschrittTests`; manuell – „Bike“ und „Alle Aktivitäten“ vergleichen.
- Wenn in der Filterleiste eine Kategorie gewählt ist, dann enthält das Diagramm nur Einheiten dieser Kategorie; die übrigen Filter und die Jahresauswahl der Werkzeugleiste ändern das Diagramm nicht.
  - Umgesetzt: `ErmittleJahresfortschritt` (nur Kategorie aus dem Filter, Jahr nicht verwendet)
  - Geprüft: `ErmittleJahresfortschrittTests`; manuell – Kategorie, Equipment und Jahr wechseln.
- Wenn für den gezeigten Wert ein Ziel im laufenden Jahr besteht, dann stehen über dem Diagramm Stand, Ziel, Anteil in Prozent, Soll-Stand zum heutigen Tag (Ziel anteilig zu den verstrichenen Tagen des Jahres) und Vorsprung bzw. Rückstand.
  - Umgesetzt: `Jahreskennzahlen`, `BeschreibeJahresfortschritt`
  - Geprüft: `ErmittleJahresfortschrittTests` (fester Tag, z. B. 24.10.2026), `BeschreibeJahresfortschrittTests`
- Wenn das Ziel noch nicht erreicht ist, dann nennt die Karteikarte die nötige Wochenleistung bis zum 31. Dezember, z. B. „noch 2.345 km – 112,4 km pro Woche“; die verbleibenden Wochen sind die restlichen Tage einschließlich heute geteilt durch 7.
  - Umgesetzt: `ErmittleJahresfortschritt`, `BeschreibeJahresfortschritt`
  - Geprüft: `ErmittleJahresfortschrittTests` (auch 31.12. mit 1/7 Woche), `BeschreibeJahresfortschrittTests`
- Wenn das Ziel erreicht ist, dann steht dort „Ziel erreicht“ mit dem Überschuss.
  - Umgesetzt: `BeschreibeJahresfortschritt`
  - Geprüft: `BeschreibeJahresfortschrittTests`
- Wenn für Kilometer, Anzahl oder Höhenmeter kein Ziel besteht, dann stehen nur der Stand und der Hinweis „kein Saisonziel – unter Extras → Saisonziele… festlegen“; für die Fahrzeit gibt es kein Saisonziel, dort steht nur der Stand.
  - Umgesetzt: `BeschreibeJahresfortschritt`
  - Geprüft: `BeschreibeJahresfortschrittTests`
- Wenn Einheiten angelegt, geändert oder gelöscht werden oder sich die Saisonziele, die Aktivität der Werkzeugleiste oder die Kategorie der Filterleiste ändern, dann zeigt die Karteikarte sofort den neuen Stand.
  - Umgesetzt: Aufruf am Ende von `AktualisiereSummen`, das bei Änderungen an Einheiten, Werkzeugleiste und Filter läuft; Aufruf nach dem Speichern der Saisonziele
  - Geprüft: manuell – Einheit anlegen, Ziel ändern, Aktivität und Kategorie wechseln.
- Wenn die Auswahl keine Einheiten enthält, dann erscheint statt des Diagramms der Hinweis „Keine Einheiten in dieser Auswahl.“
  - Umgesetzt: `JahresfortschrittViewModel.HatEinheiten`, `AktuellAnsicht`
  - Geprüft: `ErmittleJahresfortschrittTests` (leere Auswahl); manuell – Aktivität ohne Einheiten wählen.

## Entscheidungen
- **Laufendes Jahr immer im Diagramm:** Gibt es überhaupt Einheiten in der Auswahl, steht das laufende Jahr ganz rechts, auch mit Summe 0 (z. B. am 1. Januar). Sonst fehlten die Kennzahlen und der Soll-Stand.
- **Gezeigter Wert bleibt bis zum Programmende:** Nach einer Änderung der Auswahl bleibt der gewählte Wert (z. B. hm) stehen. Er wird nicht gespeichert; beim Start ist es Kilometer.
- **Kein Hinweis am Punkt:** Auf Wunsch des Benutzers entfallen; die Werte des laufenden Jahres stehen in den Kennzahlen.
- **Rundung:** Kilometer und Wochenleistung werden mit einer Nachkommastelle angezeigt, Höhenmeter und Anzahl ganzzahlig, Prozent ganzzahlig, Fahrzeit als Stunden:Minuten.

## Offene Entscheidungen
- Welche Diagrammbibliothek (für dieses und alle weiteren Diagramme der App, z. B. Höhenprofil)? → decided in docs/adr/0001-diagrammbibliothek.md

## Risiken
- Mit `AktualisiereSummen` wird die Ansicht häufiger neu berechnet, z. B. bei jeder Filteränderung. Bei rund tausend Einheiten ist das unkritisch, das Diagramm sollte aber nur die Daten tauschen und nicht jedes Mal neu entstehen.
