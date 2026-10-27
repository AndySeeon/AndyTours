# Plan: 009 – Auswahllisten verwalten

## Vorgehen
- **Speicherung:** Die Liste „Trainingsbereich“ steht in der vorhandenen Tabelle `Einstellung` unter dem Schlüssel `Auswahlliste.Trainingsbereich`. Gespeichert wird ein Wert je Zeile, in der Reihenfolge des Benutzers. Das Schema ändert sich nicht.
  - Fehlt der Schlüssel, wurde die Liste noch nie angelegt.
  - Ein leerer Wert bedeutet, dass der Benutzer alle Einträge gelöscht hat.
  - Weitere Listen mit festen Werten erhalten später eigene Schlüssel `Auswahlliste.<Name>`.
- **Anfangsbestand:** `LadeDatenbank` liest die Liste. Fehlt sie, wird sie aus den vorkommenden Kürzeln gebildet (`ErmittleTrainingsbereiche`) und sofort gespeichert. Das greift beim ersten Start mit dieser Funktion, nach dem Wiederherstellen einer älteren Sicherung und bei einem anderen Datenordner.
- **Fachfunktionen (Core):** Sie arbeiten mit Listen von Texten und den Einheiten.
  - Prüfen eines Eintrags: leer oder doppelt, ohne Unterschied zwischen Groß- und Kleinschreibung, nach dem Entfernen der Leerzeichen am Rand.
  - Verschieben eines Eintrags.
  - Zählen der Einheiten, die einen Eintrag verwenden (ohne Unterschied zwischen Groß- und Kleinschreibung).
  - Umbenennen in den Einheiten.
- **Pflegen (App):** Neu ist ein `AuswahllistenViewModel`. Das Einstellungsfenster bekommt dafür die Gruppe „Auswahllisten“:
  - eine Auswahl der Liste, zunächst nur „Trainingsbereich“;
  - die Einträge als Liste, rechts davon „Nach oben“, „Nach unten“ und „Löschen“;
  - darunter das Eingabefeld „Eintrag“ mit „Hinzufügen“ und „Umbenennen“ und eine rote Hinweiszeile.

  Ein Klick auf einen Eintrag übernimmt ihn ins Eingabefeld.
- **Speichern einer Änderung:** Das `AuswahllistenViewModel` rechnet die neue Liste aus. Gespeichert wird über eine Funktion, die ihm das `HauptfensterViewModel` übergibt, weil dieses den Datenbestand hält.
  - Die Funktion schreibt in einer Transaktion die Liste und, beim Umbenennen, den Trainingsbereich der betroffenen Einheiten.
  - Erst bei Erfolg ändert sie den Datenbestand im Speicher, setzt einen auf den alten Namen gesetzten Filter um und baut Tabelle, Filterlisten und Summen neu auf (`AktualisiereSummen`).
  - Bei einem Fehler erscheint die Fehlermeldung über das vorhandene `Schreibe`, und das `AuswahllistenViewModel` behält seinen bisherigen Stand.
- **Rückfragen:** Das `AuswahllistenViewModel` stellt sie selbst, wie das `HauptfensterViewModel` beim Löschen.
  - Umbenennen eines verwendeten Eintrags: „In {n} Einheiten ‚{alt}‘ durch ‚{neu}‘ ersetzen?“ (Ja/Nein, Vorgabe Nein).
  - Löschen eines verwendeten Eintrags: Hinweis „‚{Eintrag}‘ wird in {n} Einheiten verwendet und kann nicht gelöscht werden.“
  - Löschen eines unbenutzten Eintrags: ohne Rückfrage.
- **Verwenden:** `ErmittleEingabelisten` erhält die gepflegte Liste als Parameter. Der Trainingsbereich wird angeboten als leerer Eintrag, dann die Liste in ihrer Reihenfolge. Hat die bearbeitete Einheit einen Wert, der nicht in der Liste steht, kommt er am Ende dazu, damit er erhalten bleibt. Die Filterleiste (`ErmittleFilterwerte`) bleibt unverändert und zeigt die vorkommenden Werte.

## Dateien
- `src/AndyTours.Core/Modell/Auswahlliste.cs` (neu) — Aufzählung der pflegbaren Listen (zunächst `Trainingsbereich`)
- `src/AndyTours.Core/Berechnungen/ErmittleTrainingsbereiche.cs` (neu) — Anfangsliste aus den vorkommenden Kürzeln: getrimmt, ohne leere Werte, ohne Dubletten (ohne Unterschied zwischen Groß- und Kleinschreibung), alphabetisch
- `src/AndyTours.Core/Berechnungen/PruefeAuswahleintrag.cs` (neu) — prüft einen neuen oder umbenannten Eintrag (leer, schon vorhanden; der umbenannte Eintrag selbst zählt nicht) und liefert den Hinweistext oder `null`
- `src/AndyTours.Core/Berechnungen/VerschiebeEintrag.cs` (neu) — neue Reihenfolge nach „Nach oben“/„Nach unten“; am Rand unverändert
- `src/AndyTours.Core/Berechnungen/ZaehleTrainingsbereich.cs` (neu) — Anzahl der Einheiten mit diesem Trainingsbereich
- `src/AndyTours.Core/Berechnungen/BenenneTrainingsbereichUm.cs` (neu) — ersetzt den Trainingsbereich in den Einheiten im Speicher
- `src/AndyTours.Core/Berechnungen/ErmittleEingabelisten.cs` (ändern) — Trainingsbereiche aus der gepflegten Liste statt aus dem Datenbestand
- `src/AndyTours.Daten/Einstellungsschluessel.cs` (ändern) — Schlüssel `Auswahlliste.Trainingsbereich`
- `src/AndyTours.Daten/AuswahllisteEinstellung.cs` (neu) — `Laden` (liefert `null`, wenn der Schlüssel fehlt) und `Speichern` einer Liste, auch innerhalb einer Transaktion
- `src/AndyTours.Daten/TrainingseinheitenTabelle.cs` (ändern) — `ErsetzeTrainingsbereich(verbindung, transaktion, alt, neu)` ersetzt den Wert in allen Einheiten (ohne Unterschied zwischen Groß- und Kleinschreibung)
- `src/AndyTours.App/ViewModels/AuswahllistenViewModel.cs` (neu) — Listenauswahl, Einträge, Eingabefeld, Hinweis, Befehle Hinzufügen, Umbenennen, Löschen, Nach oben, Nach unten
- `src/AndyTours.App/ViewModels/EinstellungenViewModel.cs` (ändern) — nimmt das `AuswahllistenViewModel` entgegen und stellt es bereit
- `src/AndyTours.App/Views/EinstellungenFenster.xaml` (ändern) — Gruppe „Auswahllisten“, Kopfkommentar
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Auswahllisten.cs` (neu) — gepflegte Liste im Speicher, Laden mit Anfangsbestand, Zählen, Speichern einer Änderung samt Aktualisierung der Anzeige
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — `LadeDatenbank` lädt die Liste, `EinstellungenOeffnen` erzeugt das `AuswahllistenViewModel`
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Einheiten.cs` (ändern) — `ErmittleEingabelisten` mit der gepflegten Liste aufrufen
- `tests/AndyTours.Tests/Berechnungen/ErmittleTrainingsbereicheTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/PruefeAuswahleintragTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/VerschiebeEintragTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ZaehleUndBenenneTrainingsbereichTests.cs` (neu) — Tests zu `ZaehleTrainingsbereich` und `BenenneTrainingsbereichUm`
- `tests/AndyTours.Tests/Berechnungen/ErmittleEingabelistenTests.cs` (ändern) — Trainingsbereiche aus der Liste, fremder Wert der Einheit am Ende
- `tests/AndyTours.Tests/Daten/AuswahllisteTests.cs` (neu) — Laden/Speichern, fehlender Schlüssel, leere Liste, Reihenfolge, `ErsetzeTrainingsbereich`, Rücknahme bei abgebrochener Transaktion

## Abdeckung der Kriterien
- Wenn der Benutzer die Einstellungen öffnet, dann findet er einen Bereich „Auswahllisten“ mit einer Auswahl der Liste (zunächst nur „Trainingsbereich“) und darunter deren Einträge in der festgelegten Reihenfolge. → `EinstellungenFenster.xaml`, `AuswahllistenViewModel`; manuelle Prüfung: Extras → Einstellungen.
- Wenn die App zum ersten Mal mit dieser Funktion gestartet wird, dann enthält die Liste „Trainingsbereich“ die Kürzel, die im Datenbestand vorkommen, alphabetisch geordnet. → `ErmittleTrainingsbereiche`, Anfangsbestand in `HauptfensterViewModel.Auswahllisten`, `AuswahllisteEinstellung.Laden` (`null` bei fehlendem Schlüssel); geprüft durch `ErmittleTrainingsbereicheTests`, `AuswahllisteTests` (`dotnet test AndyTours.slnx`); manuelle Prüfung: erster Start zeigt GA1, GA2, K0, SK, WSA.
- Wenn der Benutzer einen Eintrag hinzufügt, dann erscheint er am Ende der Liste und ist sofort dauerhaft gespeichert. → `AuswahllistenViewModel.Hinzufuegen`, `AuswahllisteEinstellung.Speichern`; geprüft durch `AuswahllisteTests`; manuelle Prüfung: hinzufügen, App neu starten, Eintrag ist noch da.
- Wenn ein neuer oder umbenannter Eintrag leer ist oder es ihn in der Liste schon gibt (ohne Unterschied zwischen Groß- und Kleinschreibung; Leerzeichen am Anfang und Ende werden entfernt), dann wird er abgewiesen, und ein Hinweis nennt den Grund. → `PruefeAuswahleintrag`; geprüft durch `PruefeAuswahleintragTests` (leer, nur Leerzeichen, „ga1“ bei „GA1“, Umbenennen nur in der Schreibweise); manuelle Prüfung: Hinweis erscheint.
- Wenn der Benutzer einen Eintrag nach oben oder unten verschiebt, dann gilt die neue Reihenfolge sofort und bleibt beim nächsten Start erhalten. → `VerschiebeEintrag`, `AuswahllisteEinstellung.Speichern`; geprüft durch `VerschiebeEintragTests`, `AuswahllisteTests` (Reihenfolge); manuelle Prüfung: verschieben, neu starten.
- Wenn der Benutzer einen Eintrag umbenennt, der in keiner Einheit vorkommt, dann ändert sich nur die Liste, ohne Rückfrage. → `AuswahllistenViewModel.Umbenennen` (Zählung 0 → keine Rückfrage); geprüft durch `ZaehleUndBenenneTrainingsbereichTests`; manuelle Prüfung.
- Wenn der Benutzer einen Eintrag umbenennt, der in Einheiten vorkommt, dann fragt die App mit der Anzahl („In 714 Einheiten ‚GA2‘ durch ‚GA 2‘ ersetzen?“) und ändert nach „Ja“ die Liste und alle diese Einheiten; bei „Nein“ bleibt alles unverändert. → `AuswahllistenViewModel.Umbenennen`, `ErsetzeTrainingsbereich` und `AuswahllisteEinstellung.Speichern` in einer Transaktion, `BenenneTrainingsbereichUm`; geprüft durch `ZaehleUndBenenneTrainingsbereichTests`, `AuswahllisteTests`; manuelle Prüfung: Rückfrage mit Anzahl, Ja/Nein.
- Wenn der Benutzer einen Eintrag löschen will, der in Einheiten vorkommt, dann ist das nicht möglich, und ein Hinweis nennt die Anzahl dieser Einheiten. → `AuswahllistenViewModel.Loeschen`, `ZaehleTrainingsbereich`; geprüft durch `ZaehleUndBenenneTrainingsbereichTests`; manuelle Prüfung: „GA2“ löschen → Hinweis mit 714.
- Wenn der Benutzer einen Eintrag, der in keiner Einheit vorkommt, ausdrücklich mit „Löschen“ entfernt, dann verschwindet er ohne weitere Rückfrage aus der Liste; Einträge, die er nicht löscht, bleiben erhalten, auch wenn sie noch keiner Einheit zugeordnet sind. → `AuswahllistenViewModel.Loeschen`; Einträge werden nur durch diesen Befehl entfernt; geprüft durch `AuswahllisteTests` (gespeicherte Liste unverändert geladen); manuelle Prüfung: neuen Eintrag anlegen, neu starten, er ist noch da; dann löschen.
- Wenn das Speichern einer Änderung fehlschlägt (z. B. Datenordner nicht erreichbar), dann erscheint eine Fehlermeldung, und Liste und Einheiten bleiben beim vorherigen Stand. → Speichern über `Schreibe` in einer Transaktion, Speicher und Anzeige erst nach Erfolg ändern; geprüft durch `AuswahllisteTests` (abgebrochene Transaktion ändert nichts); manuelle Prüfung: Datenbank wie bei 008 von außen sperren, umbenennen → Fehlermeldung, Liste und Tabelle unverändert.
- Wenn der Benutzer im Bearbeitungsfenster oder in einer Tabellenzelle den Trainingsbereich wählt, dann bietet die Auswahlliste einen leeren Eintrag und die Einträge der Liste in deren Reihenfolge an. → `ErmittleEingabelisten`; geprüft durch `ErmittleEingabelistenTests`; manuelle Prüfung in Fenster und Zelle.
- Wenn die Liste geändert wurde, dann bieten danach geöffnete Bearbeitungsfenster und Tabellenzellen den neuen Stand an. → Liste im Speicher des `HauptfensterViewModel`, bei jedem Öffnen an `ErmittleEingabelisten` übergeben; manuelle Prüfung.
- Wenn ein Eintrag umbenannt wurde, dann zeigen Tabelle und Filterleiste sofort den neuen Namen. → `BenenneTrainingsbereichUm`, gesetzten Filter umstellen, `AktualisiereSummen`; manuelle Prüfung: umbenennen, Einstellungen schließen, Tabelle und Filterliste zeigen den neuen Namen.
- Wenn der Benutzer in der Filterleiste nach Trainingsbereich filtert, dann bietet die Liste weiterhin die Werte an, die in den Einheiten vorkommen. → `ErmittleFilterwerte` unverändert; geprüft durch die bestehenden `ErmittleFilterwerteTests`; manuelle Prüfung: neuer, unbenutzter Eintrag erscheint nicht im Filter.

## Entscheidungen
- H1: Eingabefeld unter der Liste statt eigener Eingabefenster. Ein Klick auf einen Eintrag übernimmt ihn ins Feld; „Hinzufügen“ fügt den Feldinhalt an, „Umbenennen“ ersetzt den markierten Eintrag. Hinweise erscheinen rot unter dem Feld wie bei der Datensicherung.
- Die Liste steht in der Tabelle `Einstellung`, ohne Schemaänderung. Das folgt dem Muster der übrigen Einstellungen.
- Doppelte Einträge und verwendende Einheiten werden ohne Unterschied zwischen Groß- und Kleinschreibung ermittelt. Das Umbenennen ersetzt auch Schreibvarianten in den Einheiten.
- Ein Wert einer Einheit, der nicht in der Liste steht, wird in ihrer Auswahlliste zusätzlich angeboten, damit er beim Speichern nicht verloren geht. Bei gepflegter Liste kommt das nicht vor.

## Risiken
- Das Umbenennen von „GA2“ ändert 714 Einheiten auf einmal. Die Transaktion stellt sicher, dass entweder alles oder nichts geändert wird; eine automatische Sicherung beim nächsten Start bleibt die Rückfallebene.
