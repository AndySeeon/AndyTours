# Plan: 033 – Installationsprogramm

## Vorgehen
- **Veröffentlichen:** App und Umwandlungsprogramm werden als Release-Build für `win-x64` mit eingebautem .NET veröffentlicht (`dotnet publish … --self-contained`). Dadurch muss auf dem Rechner nichts weiter installiert sein. Beide Programme kommen in denselben Programmordner.
- **Installationsprogramm:** wird nur für den Benutzer installiert, ohne Administratorrechte.
  - Programmordner `%LOCALAPPDATA%\Programs\AndyTours`
  - Verknüpfung im Startmenü, auf Wunsch auch auf dem Desktop
  - Eintrag unter „Apps“ (`HKCU\…\Uninstall\AndyTours`)
  - Seiten in dieser Reihenfolge: Datenordner (Vorschlag `Dokumente\AndyTours`), Übernahme der bisherigen Daten (vorgewählt, Ordnerwahl ohne Vorschlag), Verknüpfungen, Zusammenfassung.
  - Der gewählte Datenordner kommt in die vorhandene Merkdatei `%APPDATA%\AndyTours\datenordner.txt` (`DatenordnerMerkdatei.Speichern`). Die App liest ihn beim ersten Start unverändert über `ErmittleStartDatenordner`; an der App ändert sich nichts.
  - Womit das Installationsprogramm gebaut wird, ist eine wichtige Entscheidung (siehe Offene Entscheidungen).
- **Übernahme:** Das Installationsprogramm ruft das vorhandene Umwandlungsprogramm auf. Es bekommt drei neue Schalter:
  - `--erkennen <Ordner>`: meldet über den Rückgabecode, was im Ordner liegt (BikeXperience-Daten / nur Aufzeichnungen / nichts), und schreibt die Anzahl der Aufzeichnungen aus.
  - `--aufzeichnungen <Ordner> <Zielordner>`: übernimmt alle FIT-Dateien des Ordners samt Unterordnern als Touren. Es baut den Bestand wie die Umwandlung in einem Zwischenordner auf und tauscht ihn danach aus. Bei einem Fehler bleibt der Datenordner so wie vorher. Nicht lesbare Dateien werden übersprungen und im Bericht `aufzeichnungsbericht.txt` genannt. Die Touren bekommen kein Rad; als Aktivität wird im neuen Datenordner „Radfahren“ als Hauptaktivität angelegt und zugeordnet.
  - `--ohne-rueckfrage`: Liegen im Ziel schon AndyTours-Daten, bleiben sie unverändert; dann gibt es Rückgabecode 2 und keine Rückfrage auf der Konsole.
  - Die Zusammenfassung gibt das Programm wie bisher auf der Konsole aus. Das Installationsprogramm fängt sie ab und zeigt sie an.
- **Gemeinsame Logik statt Kopie:** Wie eine Tour aus einer Aufzeichnung entsteht, übernehmen die vorhandenen Fachfunktionen des Edge-Imports (`LeseFitAufzeichnung`, `IstTourVorhanden`, `LegeAufzeichnungAb`, `ErstelleImportierteEinheit` mit Equipment `null`, `TrainingseinheitenTabelle.Einfuegen`). Damit sind die Werte dieselben wie beim Import vom Edge.
- **Protokoll:** Eine lesbare Textdatei `installationsprotokoll.txt` im Programmordner, je Änderung eine Zeile `Zeitpunkt | Aktion | Art | Ziel`:
  - Aktionen: `angelegt` und `geändert`
  - Arten: `Ordner`, `Datei`, `Verknüpfung`, `Registry`, `Einstellung`
  - Protokolliert werden auch die Ordner, die die App selbst außerhalb des Datenordners anlegt (`%APPDATA%\AndyTours`, `%LOCALAPPDATA%\AndyTours`). So kann das Deinstallationsprogramm auch sie entfernen.
  - Bei einer Aktualisierung werden die neuen Zeilen angehängt.
- **Deinstallation:**
  - Bittet zuerst, eine laufende AndyTours zu beenden.
  - Fragt danach, ob der Datenordner gelöscht werden soll; vorgewählt ist „Behalten“.
  - Entfernt dann in umgekehrter Reihenfolge alles, was im Protokoll steht, zuletzt den Programmordner samt Protokoll.
- **Laufende App:** Installation und Deinstallation erkennen sie am Prozess `AndyTours.App` und warten, bis der Benutzer sie beendet hat.
- **Aktualisieren:** Ist schon ein Eintrag unter „Apps“ vorhanden, gilt die Installation als Aktualisierung. Die Seiten für Datenordner und Übernahme entfallen; Merkdatei und Datenordner bleiben unangetastet. Die Versionsnummer kommt aus `Directory.Build.props`.

## Dateien
- `tools/AndyTours.Altdatenuebernahme/Aufrufparameter.cs` (ändern) — zusätzlich Modus (Umwandeln / Erkennen / Aufzeichnungen) und Kennzeichen „ohne Rückfrage“
- `tools/AndyTours.Altdatenuebernahme/Programmmodus.cs` (neu) — Aufzählung der Modi
- `tools/AndyTours.Altdatenuebernahme/LeseAufrufparameter.cs` (ändern) — wertet `--erkennen`, `--aufzeichnungen` und `--ohne-rueckfrage` aus; bisheriger Aufruf unverändert
- `tools/AndyTours.Altdatenuebernahme/Rueckgabecode.cs` (neu) — Konstanten: 0 Erfolg, 1 Fehler, 2 Ziel hat schon Daten, 10/11/12 Ergebnis des Erkennens
- `tools/AndyTours.Altdatenuebernahme/Program.cs` (ändern) — verzweigt nach Modus; ohne Rückfrage keine Konsoleneingabe
- `src/AndyTours.Import/Altdaten/Altdatenart.cs` (neu) — Aufzählung: BikeXperience, NurAufzeichnungen, Nichts
- `src/AndyTours.Import/Altdaten/Altdatenfund.cs` (neu) — Art und gefundene Aufzeichnungsdateien
- `src/AndyTours.Import/Altdaten/ErkenneAltdaten.cs` (neu) — prüft einen Ordner: `bikexperience.ini` und `workouts.csv` → BikeXperience; sonst `*.fit` samt Unterordnern → NurAufzeichnungen; sonst Nichts
- `tools/AndyTours.Altdatenuebernahme/ErstelleAufzeichnungsordner.cs` (neu) — liest die Aufzeichnungen und baut im Zwischenordner Datenbank, Aufzeichnungen und Bericht auf. Nicht lesbare und doppelte Aufzeichnungen werden übersprungen. Liefert Anzahl übernommen und die Liste der übersprungenen Dateien.
- `tools/AndyTours.Altdatenuebernahme/Aufzeichnungsergebnis.cs` (neu) — Ergebnis der Übernahme der Aufzeichnungen
- `tools/AndyTours.Altdatenuebernahme/ErstelleAufzeichnungsbericht.cs` (neu) — Berichtstext mit Anzahl und übersprungenen Dateien samt Grund; Kurzfassung für die Konsole
- `tools/AndyTours.Installation/AndyTours.iss` (neu) — Inno-Setup-Skript: Installation nur für den Benutzer (`PrivilegesRequired=lowest`), Programmordner, Verknüpfungen, Eintrag unter „Apps“, Erkennen der Aktualisierung; im `[Code]`-Teil die Seiten für Datenordner und Übernahme, der Aufruf des Umwandlungsprogramms mit Anzeige der Zusammenfassung, das Schreiben der Merkdatei, das Protokoll, das Warten auf eine laufende App und die Deinstallation samt Rückfrage zum Datenordner
- `tools/AndyTours.Installation/Erstelle-Installation.ps1` (neu) — veröffentlicht App und Umwandlungsprogramm (`dotnet publish`, Release, `win-x64`, self-contained) und baut daraus mit dem Inno-Setup-Compiler `ISCC.exe` das Installationsprogramm; die Version kommt aus `Directory.Build.props`
- `docs/structure.md` (ändern) — Eintrag `tools/AndyTours.Installation/`
- `docs/tech.md` (ändern) — Werkzeug des Installationsprogramms und Befehl „Installationsprogramm bauen“ unter `## Befehle`
- `tests/AndyTours.Tests/Altdaten/ErkenneAltdatenTests.cs` (neu)
- `tests/AndyTours.Tests/Altdatenuebernahme/LeseAufrufparameterTests.cs` (ändern) — neue Schalter
- `tests/AndyTours.Tests/Altdatenuebernahme/AufzeichnungsuebernahmeTests.cs` (neu) — mit `FitTestdatei` erzeugte Aufzeichnungen, dazu eine beschädigte Datei und ein Unterordner; in `[Collection(FitTestdatei.Testgruppe)]`
- `tests/AndyTours.Tests/Altdatenuebernahme/ErstelleAufzeichnungsberichtTests.cs` (neu)

## Abdeckung der Kriterien
Die Kurzbezeichnungen „Installationsprogramm“ und „Deinstallationsprogramm“ stehen für `tools/AndyTours.Installation/AndyTours.iss`. Die manuellen Prüfungen laufen mit einem Test-Datenordner, nicht mit `AppDaten`.

### Installieren
- Wenn der Benutzer das Installationsprogramm startet, dann führt es ihn in wenigen Schritten durch die Installation: Datenordner, Übernahme der bisherigen Daten, Verknüpfungen und Zusammenfassung vor dem Start. → Installationsprogramm (Seitenfolge); manuelle Prüfung: Installationsprogramm starten und die vier Seiten in dieser Reihenfolge durchgehen.
- Wenn die Installation fertig ist, dann lässt sich AndyTours über das Startmenü starten. Weitere Programme müssen dafür nicht installiert werden. → `Erstelle-Installation.ps1` (self-contained), Installationsprogramm (Verknüpfung); manuelle Prüfung: AndyTours über das Startmenü starten; in der Windows-Sandbox ohne .NET prüfen, dass die App startet.
- Wenn der Benutzer installiert, dann braucht er keine Administratorrechte, denn AndyTours wird nur für ihn installiert. → Installationsprogramm (Installation nur für den Benutzer); manuelle Prüfung: Es erscheint keine Abfrage der Benutzerkontensteuerung, und der Programmordner liegt unter `%LOCALAPPDATA%\Programs\AndyTours`.
- Wenn das Installationsprogramm nach dem Datenordner fragt, dann schlägt es `Dokumente\AndyTours` vor. → Installationsprogramm (Seite Datenordner); manuelle Prüfung.
- Wenn der Benutzer einen Datenordner gewählt hat, dann verwendet AndyTours beim ersten Start genau diesen Ordner. → Installationsprogramm schreibt die Merkdatei; die App liest sie über `DatenordnerMerkdatei.Laden` und `ErmittleStartDatenordner`, geprüft durch die vorhandenen `MerkdateiTests` und `ErmittleStartDatenordnerTests`; manuelle Prüfung: einen Testordner wählen, die App starten, die Statusleiste zeigt diesen Ordner.
- Wenn die Installation fertig ist, dann gibt es immer eine Verknüpfung im Startmenü. → Installationsprogramm; manuelle Prüfung.
- Wenn der Benutzer zusätzlich eine Desktop-Verknüpfung gewählt hat, dann liegt nach der Installation eine Verknüpfung auf dem Desktop. → Installationsprogramm (Seite Verknüpfungen); manuelle Prüfung mit und ohne Häkchen.
- Wenn die Installation fertig ist, dann erscheint AndyTours in den Windows-Einstellungen unter „Apps“ und lässt sich dort deinstallieren. → Installationsprogramm (Eintrag unter „Apps“); manuelle Prüfung: Einstellungen → Apps → Installierte Apps.

### Bisherige Daten übernehmen
- Wenn das Installationsprogramm die Übernahme anbietet, dann ist sie vorgewählt, und der Benutzer kann sie abwählen. → Installationsprogramm (Seite Übernahme); manuelle Prüfung.
- Wenn der Benutzer die Übernahme gewählt hat, dann fragt das Installationsprogramm nach dem Ordner mit den bisherigen Daten, ohne einen Ordner vorzuschlagen. → Installationsprogramm (Seite Übernahme); manuelle Prüfung: Das Ordnerfeld ist leer.
- Wenn im angegebenen Ordner BikeXperience-Daten liegen, dann wandelt das vorhandene Umwandlungsprogramm sie in den gewählten Datenordner um. → `ErkenneAltdaten` und Aufruf des Umwandlungsprogramms mit beiden Ordnern; geprüft durch `ErkenneAltdatenTests`, die vorhandenen `UmwandlungTests` und `LeseAufrufparameterTests`; manuelle Prüfung mit einer Kopie von `Daten/` in einen leeren Testordner.
- Wenn die Umwandlung fertig ist, dann zeigt das Installationsprogramm die Zusammenfassung des Umwandlungsprogramms und wo der Bericht liegt. → `ErstelleZusammenfassung` (vorhanden) und Anzeige der abgefangenen Ausgabe; manuelle Prüfung.
- Wenn im angegebenen Ordner keine BikeXperience-Daten, aber Aufzeichnungen im FIT-Format liegen (auch in Unterordnern), dann fragt das Installationsprogramm, ob diese als Touren übernommen werden sollen, und nennt die Anzahl der gefundenen Aufzeichnungen. → `ErkenneAltdaten`, `--erkennen`, Rückfrage im Installationsprogramm; geprüft durch `ErkenneAltdatenTests` (Unterordner, Anzahl) und manuell mit einem Ordner, der nur Jahresordner mit FIT-Dateien enthält.
- Wenn der Benutzer die Übernahme der Aufzeichnungen bestätigt, dann ist danach jede lesbare Aufzeichnung eine Tour im Datenordner, mit denselben Werten wie bei einem Import vom Edge. → `ErstelleAufzeichnungsordner` mit den Fachfunktionen des Edge-Imports; geprüft durch `AufzeichnungsuebernahmeTests` (Anzahl der Einheiten, Werte gleich `ErstelleImportierteEinheit`, Aufzeichnung im Jahresordner).
- Wenn Aufzeichnungen als Touren übernommen werden, dann bekommen die Touren kein Rad und die Hauptaktivität „Radfahren“ zugeordnet. → `ErstelleAufzeichnungsordner` (Equipment `null`, legt im neuen Datenordner die Aktivität „Radfahren“ als Hauptaktivität an; die Aktivität ist in der Datenbank ein Pflichtfeld); geprüft durch `AufzeichnungsuebernahmeTests`.
- Wenn eine Aufzeichnung nicht lesbar ist, dann wird sie übersprungen, die übrigen werden übernommen, und ein Bericht nennt die übersprungenen Dateien. → `ErstelleAufzeichnungsordner`, `ErstelleAufzeichnungsbericht`; geprüft durch `AufzeichnungsuebernahmeTests` (beschädigte Datei) und `ErstelleAufzeichnungsberichtTests`.
- Wenn die Übernahme der Aufzeichnungen fertig ist, dann zeigt das Installationsprogramm, wie viele Touren übernommen und wie viele Dateien übersprungen wurden und wo der Bericht liegt. → `ErstelleAufzeichnungsbericht` (Kurzfassung) und Anzeige im Installationsprogramm; geprüft durch `ErstelleAufzeichnungsberichtTests` und manuell.
- Wenn der Benutzer die Übernahme der Aufzeichnungen verneint, dann wird nichts übernommen, und die Installation geht ohne bisherige Daten weiter. → Installationsprogramm (kein Aufruf des Umwandlungsprogramms); manuelle Prüfung: Der Datenordner ist danach leer, und die Installation endet normal.
- Wenn im angegebenen Ordner weder BikeXperience-Daten noch Aufzeichnungen liegen, dann erscheint ein Hinweis, und der Benutzer kann einen anderen Ordner wählen oder ohne Übernahme weitermachen. → `ErkenneAltdaten` (Nichts), Hinweis im Installationsprogramm; geprüft durch `ErkenneAltdatenTests` und manuell mit einem leeren Ordner.
- Wenn im gewählten Datenordner schon AndyTours-Daten liegen, dann bleiben sie unverändert, die Übernahme entfällt, und ein Hinweis sagt das. → `--ohne-rueckfrage` (Rückgabecode 2) mit `PruefeZielordner` (vorhanden), Hinweis im Installationsprogramm; geprüft durch `LeseAufrufparameterTests` und `AufzeichnungsuebernahmeTests` (Ziel mit Datenbank bleibt unverändert); manuelle Prüfung.
- Wenn die Übernahme mit einem Fehler abbricht, dann ist AndyTours trotzdem installiert, der Datenordner bleibt wie vorher, und ein Hinweis nennt den Grund. → Zwischenordner und `TauscheZielordnerAus` (vorhanden), Rückgabecode 1 mit Fehlertext, Installationsprogramm macht weiter; geprüft durch die vorhandenen `TauscheZielordnerAusTests` und `AufzeichnungsuebernahmeTests` (Fehler beim Schreiben → Ziel unverändert); manuelle Prüfung: die Datenbankdatei im Zielordner während der Übernahme sperren.
- Wenn die Übernahme läuft, dann bleibt der Ordner mit den bisherigen Daten unverändert. → Umwandlung und Übernahme lesen die Quelle nur; geprüft durch die vorhandenen `UmwandlungTests` und `AufzeichnungsuebernahmeTests` (Dateiliste und Inhalte der Quelle vorher und nachher gleich).

### Änderungen festhalten
- Wenn die Installation fertig ist, dann gibt es ein lesbares Protokoll aller Änderungen am Rechner: angelegte Ordner und Dateien, Verknüpfungen, Einträge in Windows und der eingestellte Datenordner. → Installationsprogramm (`installationsprotokoll.txt`); manuelle Prüfung: das Protokoll im Editor öffnen und mit dem Programmordner, den Verknüpfungen, dem Registry-Eintrag und der Merkdatei vergleichen.
- Wenn das Protokoll geöffnet wird, dann steht zu jeder Änderung, was angelegt oder geändert wurde und wann. → Zeilenformat `Zeitpunkt | Aktion | Art | Ziel`; manuelle Prüfung.

### Aktualisieren
- Wenn AndyTours schon installiert ist und der Benutzer eine neuere Fassung installiert, dann bleiben Datenordner, Daten und Einstellungen erhalten, und die Übernahme der bisherigen Daten wird nicht mehr angeboten. → Installationsprogramm (Aktualisierung erkannt, Seiten entfallen, Merkdatei unangetastet); manuelle Prüfung: Version in `Directory.Build.props` erhöhen, neu bauen, darüber installieren, danach zeigt die App dieselben Touren.
- Wenn eine neuere Fassung installiert wird, dann ergänzt das Installationsprogramm das vorhandene Protokoll um die neuen Änderungen. → Installationsprogramm (Zeilen anhängen); manuelle Prüfung: Die alten Zeilen sind noch da, neue mit dem aktuellen Zeitpunkt sind dazugekommen.
- Wenn AndyTours beim Installieren läuft, dann bittet das Installationsprogramm, AndyTours zu beenden, und fährt erst danach fort. → Installationsprogramm (Prozessprüfung `AndyTours.App`); manuelle Prüfung.

### Deinstallieren
- Wenn der Benutzer AndyTours deinstalliert, dann entfernt das Deinstallationsprogramm alles, was im Protokoll steht: Programmdateien, Verknüpfungen, Einträge in Windows und die Einstellungen der App außerhalb des Datenordners. → Deinstallationsprogramm (Protokoll rückwärts abarbeiten); manuelle Prüfung: Jeder Eintrag des Protokolls ist danach verschwunden.
- Wenn der Benutzer deinstalliert, dann fragt das Deinstallationsprogramm, ob auch der Datenordner mit allen Touren gelöscht werden soll; vorgewählt ist „Behalten“. → Deinstallationsprogramm; manuelle Prüfung.
- Wenn der Benutzer den Datenordner behält, dann sind die Daten danach unverändert vorhanden, und eine spätere Installation verwendet sie wieder. → Deinstallationsprogramm lässt den Datenordner stehen; bei der Neuinstallation greift „Datenordner hat schon Daten“; manuelle Prüfung: deinstallieren, neu installieren, denselben Ordner wählen, die App zeigt die Touren.
- Wenn die Deinstallation fertig ist, dann bleibt außerhalb des Datenordners kein Ordner und keine Datei von AndyTours zurück. → Deinstallationsprogramm (zuletzt Programmordner samt Protokoll, dazu `%APPDATA%\AndyTours` und `%LOCALAPPDATA%\AndyTours`); manuelle Prüfung: Die drei Ordner, die Verknüpfungen und der Registry-Eintrag fehlen.
- Wenn AndyTours beim Deinstallieren läuft, dann bittet das Deinstallationsprogramm, AndyTours zu beenden, und fährt erst danach fort. → Deinstallationsprogramm (Prozessprüfung); manuelle Prüfung.

## Entscheidungen
- Die Übernahme der Aufzeichnungen kommt in das vorhandene Umwandlungsprogramm, nicht in das Installationsprogramm. So bleibt alles, was Daten schreibt, in C# und testbar. Das Installationsprogramm muss dann nur ein Programm aufrufen.
- Bei der Übernahme wird nur ein leerer oder fehlender Datenordner befüllt (Zwischenordner, dann Austausch); vorhandene Daten bleiben immer unverändert.
- Der Kartenspeicher (`%LOCALAPPDATA%\AndyTours`) und die Merkdatei (`%APPDATA%\AndyTours`) gelten als „Einstellungen der App außerhalb des Datenordners“. Sie stehen im Protokoll und werden beim Deinstallieren entfernt.
- An der App ändert sich nichts: Den Datenordner bekommt sie über die vorhandene Merkdatei.
- Inno Setup statt WiX/MSI oder eines eigenen C#-Programms; Inno Setup erzeugt das Deinstallationsprogramm selbst, das lesbare Protokoll schreibt der `[Code]`-Teil zusätzlich.

## Offene Entscheidungen
- Womit wird das Installationsprogramm gebaut? (z. B. Inno Setup, WiX/MSI oder ein eigenes C#-Programm; nötig sind Installation nur für den Benutzer, eigene Seiten mit Rückfragen, lesbares Protokoll, Deinstallation über „Apps“) → decided in docs/adr/0005-installationsprogramm.md

## Risiken
- Manuelle Prüfungen auf dem eigenen Rechner ändern die echte Merkdatei `%APPDATA%\AndyTours\datenordner.txt` und legen Verknüpfungen an. Deshalb nur mit einem Test-Datenordner prüfen, möglichst in der Windows-Sandbox. Echte Daten in `AppDaten/` und `Daten/` werden nicht als Ziel verwendet.
- Die Pakete werden groß (self-contained mit WPF, etwa 150–200 MB), weil die App und das Umwandlungsprogramm beide .NET enthalten.
- Das Installationsprogramm ist nicht signiert; Windows SmartScreen warnt beim ersten Start.
- Steht der Datenordner im Programmordner oder darunter, würde die Deinstallation ihn mitlöschen. Das Installationsprogramm muss einen Datenordner im Programmordner ablehnen.
