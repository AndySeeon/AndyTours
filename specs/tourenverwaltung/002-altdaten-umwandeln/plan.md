# Plan: 002 – Altdaten umwandeln

## Vorgehen
- Neues Konsolenprogramm `tools/AndyTours.Altdatenuebernahme` (`net10.0`), verweist auf `AndyTours.Import` und `AndyTours.Daten`; die vorhandene Umwandlungslogik in `AndyTours.Import/Altdaten` wird unverändert weiterverwendet.
- Aufruf: `dotnet run --project tools/AndyTours.Altdatenuebernahme -- [BikeXperience-Ordner] [Zielordner]`; ohne Angaben `<Projektordner>\Daten` bzw. `<Projektordner>\AppDaten` (Projektordner über `FindeProjektordner` aus 001).
- Ablauf: Aufrufparameter ermitteln → Zielordner prüfen (fehlt/leer, mit AndyTours-Daten, fremder Inhalt) → ggf. Rückfrage „ersetzen? (j/N)“ → Altdaten vollständig lesen (`UebernehmeAltdaten`, nur lesend; fehlende Pflichtdateien → Meldung, nichts geschrieben) → Zwischenordner neben dem Ziel aufbauen (Datenbank, Bericht, Kopie aller Unterordner mit Aufzeichnungen/Anhängen) → Zielordner austauschen → Zusammenfassung ausgeben.
- Austausch ohne halbe Übernahme: vorhandenes Ziel umbenennen → Zwischenordner in Zielnamen umbenennen → altes Ziel löschen; schlägt ein Schritt fehl, wird zurückbenannt und der Zwischenordner entfernt.
- Fachfunktionen des Programms als eigene Dateien (`static class` mit `Ausfuehren`); das Testprojekt verweist auf das Programm, damit sie testbar sind.
- Korrekturliste um „B�ckeralm“ → „Bäckeralm“ und „(�PD)“ → „(ÜPD)“ ergänzen.

## Dateien
- `tools/AndyTours.Altdatenuebernahme/AndyTours.Altdatenuebernahme.csproj` (neu) — Konsolenprogramm, Verweise auf Import und Daten
- `tools/AndyTours.Altdatenuebernahme/Program.cs` (neu) — Einstieg: Ablauf steuern, Konsolenausgaben, Rückfrage, Rückgabecode (0 = Erfolg, 1 = Fehler/abgebrochen)
- `tools/AndyTours.Altdatenuebernahme/Aufrufparameter.cs` (neu) — Quell- und Zielordner
- `tools/AndyTours.Altdatenuebernahme/LeseAufrufparameter.cs` (neu) — Argumente auswerten, Vorgaben `Daten`/`AppDaten` im Projektordner
- `tools/AndyTours.Altdatenuebernahme/ZielordnerZustand.cs` (neu) — Aufzählung: FehltOderLeer, MitDaten, FremderInhalt
- `tools/AndyTours.Altdatenuebernahme/PruefeZielordner.cs` (neu) — ermittelt den Zustand des Zielordners
- `tools/AndyTours.Altdatenuebernahme/IstJaAntwort.cs` (neu) — wertet die Antwort auf „ersetzen? (j/N)“ aus (nur „j“/„ja“, ohne Eingabe = Nein)
- `tools/AndyTours.Altdatenuebernahme/KopiereAufzeichnungen.cs` (neu) — kopiert alle Unterordner (Jahresordner mit Aufzeichnungen und Anhängen) des BikeXperience-Ordners
- `tools/AndyTours.Altdatenuebernahme/ErstelleZwischenordner.cs` (neu) — legt den Zwischenordner neben dem Ziel an und schreibt Datenbank, Bericht und Aufzeichnungen hinein
- `tools/AndyTours.Altdatenuebernahme/TauscheZielordnerAus.cs` (neu) — ersetzt den Zielordner durch den Zwischenordner, mit Rückbenennung im Fehlerfall
- `tools/AndyTours.Altdatenuebernahme/ErstelleZusammenfassung.cs` (neu) — Kurztext für die Konsole: Mengen, Anzahl Warnungen, Pfad des Berichts
- `src/AndyTours.Import/Altdaten/UmlautKorrekturliste.cs` (ändern) — zwei Einträge ergänzen, Hinweis „bewusst nicht aufgenommen“ entfernen
- `AndyTours.slnx` (ändern) — Ordner `/tools/` mit dem neuen Projekt
- `tests/AndyTours.Tests/AndyTours.Tests.csproj` (ändern) — Verweis auf das Programm
- `tests/AndyTours.Tests/Altdaten/RepariereUmlauteTests.cs` (ändern) — neue Einträge prüfen; Beispiel für „unbekannte Stelle“ austauschen (bisher „B�ckeralm“)
- `tests/AndyTours.Tests/Altdaten/AltdatenMigrationTests.cs` (ändern) — keine nicht reparierten Umlaute mehr
- `tests/AndyTours.Tests/Altdatenuebernahme/TestAltdaten.cs` (neu) — erzeugt einen kleinen BikeXperience-Ordner (INI inkl. `[WebAccess]`-Passwort, CSV mit fehlerhafter Zeile und fehlender Aufzeichnung, Wartungsdatei, Jahresordner)
- `tests/AndyTours.Tests/Altdatenuebernahme/LeseAufrufparameterTests.cs` (neu)
- `tests/AndyTours.Tests/Altdatenuebernahme/PruefeZielordnerTests.cs` (neu)
- `tests/AndyTours.Tests/Altdatenuebernahme/IstJaAntwortTests.cs` (neu)
- `tests/AndyTours.Tests/Altdatenuebernahme/TauscheZielordnerAusTests.cs` (neu)
- `tests/AndyTours.Tests/Altdatenuebernahme/UmwandlungTests.cs` (neu) — Gesamtablauf Zwischenordner + Austausch mit den Testaltdaten
- `docs/Konventionen.md` (ändern) — Projekttabelle um `tools/AndyTours.Altdatenuebernahme` ergänzen

## Abdeckung der Kriterien
- Wenn der Benutzer das Programm mit dem BikeXperience-Ordner und einem Zielordner aufruft, dann schreibt es in diesen Zielordner. → `LeseAufrufparameter`, `Program`; geprüft durch `LeseAufrufparameterTests`, `UmwandlungTests`.
- Wenn der Benutzer keinen Zielordner angibt, dann schreibt das Programm in den Ordner `AppDaten` im Projektordner. → `LeseAufrufparameter`; geprüft durch `LeseAufrufparameterTests`.
- Wenn der Benutzer auch keinen BikeXperience-Ordner angibt, dann liest das Programm aus dem Ordner `Daten` im Projektordner. → `LeseAufrufparameter`; geprüft durch `LeseAufrufparameterTests`.
- Wenn die Umwandlung fertig ist, dann enthält der Zielordner alle Trainingseinheiten, Räder, Wartungen, Aktivitäten, Kategorien, Schwellwerte und persönlichen Daten aus BikeXperience. → `UebernehmeAltdaten`, `ErstelleZwischenordner`; geprüft durch `UmwandlungTests` (Datenbank im Ziel laden, Mengen vergleichen) und `AltdatenMigrationTests` (echte Daten).
- Wenn die Umwandlung fertig ist, dann liegen alle Aufzeichnungen und Anhänge im Zielordner und lassen sich den Trainingseinheiten zuordnen. → `KopiereAufzeichnungen`; geprüft durch `UmwandlungTests` (jeder vorhandene Anhangpfad existiert relativ zum Ziel) und manuell: echter Lauf, Bericht ohne neue „nicht gefunden“-Warnungen außer den zwei bekannten.
- Wenn die Umwandlung fertig ist, dann liegt im Zielordner ein Bericht mit den übernommenen Mengen, den Jahressummen (Anzahl, km, hm, Fahrzeit), den Hinweisen, Warnungen und nicht reparierten Umlauten. → `ErstelleBerichtstext` (vorhanden), `ErstelleZwischenordner`; geprüft durch `UmwandlungTests` (Bericht vorhanden, Abschnitte enthalten).
- Wenn die Umwandlung fertig ist, dann zeigt das Programm eine kurze Zusammenfassung (Mengen, Anzahl der Warnungen) und wo der Bericht liegt. → `ErstelleZusammenfassung`, `Program`; geprüft durch `UmwandlungTests` (Text enthält Mengen, Warnungsanzahl, Berichtspfad) und manuell beim echten Lauf.
- Wenn die Jahressummen im Bericht mit der Statistik von BikeXperience verglichen werden, dann stimmen sie überein. → `BerechneJahressummen`, `ErstelleBerichtstext`; geprüft durch `AltdatenMigrationTests` (Vergleich mit unabhängiger CSV-Auswertung) und manuell: Bericht gegen Karteikarte „Statistik“ in BikeXperience.
- Wenn Texte beschädigte Umlaute enthalten, dann werden sie repariert, soweit das eindeutig möglich ist, und die übrigen Stellen nennt der Bericht. → `RepariereUmlaute`, `UmlautKorrekturliste`, `UebernehmeAltdaten`; geprüft durch `RepariereUmlauteTests`.
- Wenn ein Tourname „Stoisser B?ckeralm“ mit beschädigtem Umlaut lautet, dann heißt er nach der Umwandlung „Stoisser Bäckeralm“. → `UmlautKorrekturliste`; geprüft durch `RepariereUmlauteTests` und `AltdatenMigrationTests` (keine nicht reparierten Texte).
- Wenn ein Tourname die Abkürzung „(?PD)“ mit beschädigtem Umlaut enthält, dann lautet sie nach der Umwandlung „(ÜPD)“. → `UmlautKorrekturliste`; geprüft durch `RepariereUmlauteTests` und `AltdatenMigrationTests`.
- Wenn zu einer Trainingseinheit die Aufzeichnung fehlt, dann wird die Einheit trotzdem übernommen und der Bericht nennt die fehlende Datei. → `UebernehmeAltdaten` (vorhanden); geprüft durch `UmwandlungTests`.
- Wenn eine Zeile der Altdaten fehlerhaft ist, dann wird der Rest trotzdem übernommen und der Bericht nennt die Zeile. → `LeseWorkoutsCsv` (vorhanden); geprüft durch `UmwandlungTests`.
- Wenn die alte Konfiguration Zugangsdaten enthält (Dropbox, Web-Zugang), dann werden diese nicht übernommen. → `LeseBikexperienceIni` liest nur benötigte Abschnitte; geprüft durch `UmwandlungTests` (eingeschleustes Passwort in keiner Datei des Ziels).
- Wenn im angegebenen BikeXperience-Ordner keine BikeXperience-Daten liegen, dann meldet das Programm das verständlich und schreibt nichts. → `Program` (Lesen vor jedem Schreiben, `FileNotFoundException` → Meldung); geprüft durch `UmwandlungTests` (leerer Quellordner → Ausnahme, Ziel und Umgebung unverändert) und manuell (Aufruf mit leerem Ordner).
- Wenn der Zielordner bereits AndyTours-Daten enthält, dann fragt das Programm, ob sie ersetzt werden sollen; ohne ausdrückliches Ja bleibt alles unverändert. → `PruefeZielordner`, `IstJaAntwort`, `Program`; geprüft durch `PruefeZielordnerTests`, `IstJaAntwortTests` und manuell (zweiter Lauf, Antwort leer bzw. „n“).
- Wenn der Benutzer das Ersetzen bestätigt, dann enthält der Zielordner danach nur die neu umgewandelten Daten. → `TauscheZielordnerAus`; geprüft durch `TauscheZielordnerAusTests` (alte Zusatzdatei ist danach weg).
- Wenn die Umwandlung mit einem Fehler abbricht, dann bleibt der Zielordner wie vorher. → `ErstelleZwischenordner`, `TauscheZielordnerAus`; geprüft durch `TauscheZielordnerAusTests` (gesperrte Datei im Ziel → Fehler, Ziel unverändert, kein Zwischenordner übrig) und `UmwandlungTests`.
- Wenn die Umwandlung läuft, dann bleibt der BikeXperience-Ordner unverändert. → nur lesende Zugriffe; geprüft durch `UmwandlungTests` (Dateiliste und Inhalte der Quelle vorher = nachher).

## Entscheidungen
- BikeXperience-Ordner optional mit Vorgabe `Daten` im Projektordner (Benutzerwahl b).
- Kriterium „mehrere Sportler“ gestrichen: Ein BikeXperience-Datenordner enthält immer genau einen Sportler.
- Zwischenordner liegt direkt neben dem Ziel (gleiches Laufwerk), damit der Austausch ein schnelles Umbenennen ist – auch auf dem NAS.
- Zielordner mit fremden Dateien, aber ohne AndyTours-Datenbank: Abbruch mit Meldung, damit nichts Fremdes gelöscht wird.
- Rückfrage: nur „j“/„ja“ (ohne Groß-/Kleinschreibung) gilt als Ja; keine Eingabe (z. B. umgeleitete Eingabe) = Nein.
- Fachfunktionen liegen im Programm selbst (sie brauchen Import und Daten zugleich); das Testprojekt verweist auf das Programm.

## Risiken
- Das Umbenennen des Zielordners scheitert, wenn AndyTours oder der Explorer gerade darin geöffnet ist; dann bleibt das Ziel wie vorher und das Programm meldet den Grund – AndyTours vorher schließen.
- Der Zwischenordner braucht vorübergehend den doppelten Platz (derzeit ca. 275 MB zusätzlich).
