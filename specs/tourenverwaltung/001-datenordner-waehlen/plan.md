# Plan: 001 – Datenordner wählen

## Vorgehen
- Fachlogik rund um den Datenordner als kleine Fachfunktionen in `AndyTours.Daten/Datenordner/` (testbar mit xUnit): Startordner ermitteln, Projektordner finden, Ordner prüfen (erreichbar / nicht beschreibbar / leer / mit Daten), Datenbestand kopieren, Ordner merken und lesen.
- „Während der Entwicklung“ = Debug-Build (`#if DEBUG`); Datenordner dann immer `<Projektordner>\AppDaten`, Projektordner = erster übergeordneter Ordner mit `AndyTours.slnx`.
- Fertige App: gemerkter Ordner aus `%APPDATA%\AndyTours\datenordner.txt`, sonst `Dokumente\AndyTours`.
- `App` übernimmt den Start: Datenordner ermitteln und prüfen, bei „nicht erreichbar“ eigenes Fenster mit „Erneut versuchen / Anderen Ordner wählen / Beenden“, danach Hauptfenster mit dem Ordner öffnen (kein `StartupUri` mehr).
- Neues Fenster „Einstellungen“ (Menü „Extras → Einstellungen…“) mit Pfadanzeige und „Ändern…“; im Debug-Build gesperrt mit Hinweis.
- Ablauf des Wechsels in einer App-Funktion: Ordnerdialog → Prüfung → Rückfragen (kopieren? / vorhandene Daten verwenden?) → ggf. kopieren → merken → Hauptfenster lädt neu. Der Wechsel gilt mit Abschluss der Rückfragen.
- Kopieren läuft im Hintergrund (`Task.Run`) und meldet nach jeder Datei den Fortschritt (`IProgress<KopierFortschritt>`); ein allgemeines Fortschrittsfenster zeigt Balken, Dateien „x von y“ und MB „x von y“. Das Fenster ist wiederverwendbar (z. B. für `datensicherung`).
- Schreibprobe mit Pufferzeit für Netzlaufwerke (UNC-Pfad oder Laufwerkstyp „Netzwerk“): Probedatei schreiben, 1 s warten, zurücklesen und vergleichen, löschen; bei Fehlschlag bis zu 3 Versuche mit je 2 s Pause. Lokale Laufwerke ohne Wartezeit, ein Versuch. Die gesamte Prüfung läuft mit 20 s Zeitlimit im Hintergrund, damit ein hängendes NAS die App nicht blockiert; Zeitüberschreitung = NichtErreichbar. Die Zeiten sind Parameter mit diesen Standardwerten (Tests verwenden 0).
- Statusleiste zeigt rechts den aktiven Datenordner.
- Menüpunkt „Altdaten aus BikeXperience übernehmen…“ und `FuehreAltdatenuebernahmeDurch` entfallen; die Umwandlungslogik in `AndyTours.Import/Altdaten` bleibt für `altdaten-umwandeln`.

## Dateien
- `src/AndyTours.Daten/Datenordner/DatenordnerInhalt.cs` (neu) — Konstanten: Dateiname der Datenbank (`andytours.db`), Name des Entwicklungsordners (`AppDaten`), Name der Projektmappe zum Finden des Projektordners
- `src/AndyTours.Daten/Datenordner/DatenordnerZustand.cs` (neu) — Aufzählung: MitDaten, Leer, NichtBeschreibbar, NichtErreichbar
- `src/AndyTours.Daten/Datenordner/FindeProjektordner.cs` (neu) — sucht ab einem Startordner aufwärts den Ordner mit der Projektmappe
- `src/AndyTours.Daten/Datenordner/ErmittleStartDatenordner.cs` (neu) — liefert den beim Start zu verwendenden Ordner (Entwicklung / gemerkt / Standard)
- `src/AndyTours.Daten/Datenordner/PruefeDatenordner.cs` (neu) — legt einen fehlenden Ordner an und ermittelt den Zustand; asynchron mit Gesamt-Zeitlimit
- `src/AndyTours.Daten/Datenordner/IstNetzwerkpfad.cs` (neu) — erkennt UNC-Pfade und Netzlaufwerke
- `src/AndyTours.Daten/Datenordner/FuehreSchreibprobeDurch.cs` (neu) — Probedatei schreiben, nach Pufferzeit zurücklesen, löschen; mit Wiederholungen
- `src/AndyTours.Daten/Datenordner/KopierFortschritt.cs` (neu) — Fortschrittsangabe: kopierte/gesamte Dateien und Bytes
- `src/AndyTours.Daten/Datenordner/KopiereDatenbestand.cs` (neu) — kopiert den gesamten Inhalt eines Datenordners in einen anderen und meldet den Fortschritt je Datei; die Datenbankdatei wird zuletzt kopiert, damit eine Teilkopie nie als „Daten vorhanden“ gilt; bei Fehler werden bereits kopierte Dateien wieder entfernt
- `src/AndyTours.Daten/Datenordner/LeseGemerktenDatenordner.cs` (neu) — liest den gemerkten Pfad aus einer Merkdatei (Pfad als Parameter), ohne Existenzprüfung
- `src/AndyTours.Daten/Datenordner/MerkeDatenordner.cs` (neu) — schreibt den Pfad in die Merkdatei
- `src/AndyTours.App/Funktionen/LeseGemerktenDatenordner.cs` (löschen) — ersetzt durch die Fassung in `AndyTours.Daten`
- `src/AndyTours.App/Funktionen/MerkeDatenordner.cs` (löschen) — ersetzt durch die Fassung in `AndyTours.Daten`
- `src/AndyTours.App/Funktionen/FuehreAltdatenuebernahmeDurch.cs` (löschen) — kein Altdaten-Import in der App
- `src/AndyTours.App/Funktionen/Programmpfade.cs` (neu) — Merkdatei `%APPDATA%\AndyTours\datenordner.txt`, Standardordner `Dokumente\AndyTours`, Kennzeichen „Entwicklung“ (`#if DEBUG`)
- `src/AndyTours.App/Funktionen/WechsleDatenordner.cs` (neu) — Ablauf des Ordnerwechsels mit Ordnerdialog, Meldungen und Rückfragen; liefert den neuen Ordner oder `null`
- `src/AndyTours.App/Views/DatenordnerNichtErreichbarFenster.xaml` (neu) — Meldung mit den drei Schaltflächen
- `src/AndyTours.App/Views/DatenordnerNichtErreichbarFenster.xaml.cs` (neu) — liefert die gewählte Aktion
- `src/AndyTours.App/Views/FortschrittsFenster.xaml` (neu) — allgemeines Fortschrittsfenster: Titel, Balken, Dateien und MB „x von y“
- `src/AndyTours.App/Views/FortschrittsFenster.xaml.cs` (neu) — Code-Behind: verhindert das Schließen, solange das Kopieren läuft
- `src/AndyTours.App/Funktionen/KopiereMitFortschritt.cs` (neu) — zeigt das Fortschrittsfenster modal und kopiert im Hintergrund (bei der Umsetzung ergänzt, wiederverwendbar für `datensicherung`)
- `src/AndyTours.App/ViewModels/FortschrittsViewModel.cs` (neu) — nimmt `KopierFortschritt` entgegen und bereitet Prozent und Texte auf
- `src/AndyTours.App/Views/EinstellungenFenster.xaml` (neu) — Pfadanzeige, „Ändern…“, Entwicklungshinweis, „Schließen“
- `src/AndyTours.App/Views/EinstellungenFenster.xaml.cs` (neu) — Code-Behind (nur `InitializeComponent`, Schließen)
- `src/AndyTours.App/ViewModels/EinstellungenViewModel.cs` (neu) — Pfad, Änderbarkeit, Hinweistext, Befehl „Ändern“
- `src/AndyTours.App/App.xaml` (ändern) — `StartupUri` entfernen, `Startup`-Ereignis eintragen
- `src/AndyTours.App/App.xaml.cs` (ändern) — Startablauf: Ordner ermitteln, prüfen, ggf. Fenster „nicht erreichbar“, Hauptfenster öffnen
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Menü „Extras → Einstellungen…“, Altdaten-Menüpunkt entfernen, Datenordner in der Statusleiste, `DataContext` nicht mehr im XAML erzeugen
- `src/AndyTours.App/Views/Hauptfenster.xaml.cs` (ändern) — ViewModel per Konstruktor übernehmen
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — Konstruktor mit Datenordner, Eigenschaft `Datenordner`, Befehl `EinstellungenOeffnen`, `LadeDatenbank` öffentlich nutzbar für den Wechsel; Altdaten-Befehl entfernen; leerer Bestand ohne Datenbank
- `tests/AndyTours.Tests/Datenordner/FindeProjektordnerTests.cs` (neu)
- `tests/AndyTours.Tests/Datenordner/ErmittleStartDatenordnerTests.cs` (neu)
- `tests/AndyTours.Tests/Datenordner/PruefeDatenordnerTests.cs` (neu)
- `tests/AndyTours.Tests/Datenordner/KopiereDatenbestandTests.cs` (neu) — inkl. Fortschrittsmeldungen
- `tests/AndyTours.Tests/Datenordner/IstNetzwerkpfadTests.cs` (neu)
- `tests/AndyTours.Tests/Datenordner/FuehreSchreibprobeDurchTests.cs` (neu)
- `tests/AndyTours.Tests/Datenordner/MerkdateiTests.cs` (neu) — Merken und Lesen im Zusammenspiel

## Abdeckung der Kriterien
- Wenn die fertige App zum ersten Mal startet, dann verwendet sie ohne Rückfrage den Ordner `Dokumente\AndyTours` als Datenordner. → `ErmittleStartDatenordner`, `Programmpfade`; geprüft durch `ErmittleStartDatenordnerTests` (keine Merkdatei → Standardordner) und manuell: Release-Build ohne Merkdatei starten, Statusleiste zeigt `Dokumente\AndyTours`.
- Wenn der Datenordner beim Start noch nicht existiert, dann legt die App ihn an. → `PruefeDatenordner`; geprüft durch `PruefeDatenordnerTests` (nicht vorhandener Unterordner eines Temp-Ordners wird angelegt, Zustand Leer).
- Wenn die App während der Entwicklung gestartet wird, dann verwendet sie immer den Ordner `AppDaten` im Projektordner, unabhängig von der Einstellung der fertigen App. → `ErmittleStartDatenordner`, `FindeProjektordner`; geprüft durch `ErmittleStartDatenordnerTests` (Entwicklung + gemerkter Ordner → trotzdem `AppDaten`), `FindeProjektordnerTests` und manuell: `dotnet run`, Statusleiste zeigt `…\AppDaten`, 932 Einheiten.
- Wenn der Benutzer im Menü „Extras → Einstellungen…“ wählt, dann öffnen sich die Einstellungen und zeigen den vollständigen Pfad des aktuellen Datenordners. → `Hauptfenster.xaml`, `EinstellungenFenster`, `EinstellungenViewModel`; manuelle Prüfung.
- Wenn die App während der Entwicklung läuft, dann lässt sich der Datenordner in den Einstellungen nicht ändern und ein Hinweis erklärt, warum. → `EinstellungenViewModel` (Änderbarkeit aus `Programmpfade`); manuelle Prüfung im Debug-Build.
- Wenn der Benutzer in der fertigen App einen anderen Ordner wählt und bestätigt, dann arbeitet die App sofort mit den Daten dieses Ordners und zeigt den Ordner in der Statusleiste an. → `WechsleDatenordner`, `HauptfensterViewModel`; manuelle Prüfung im Release-Build.
- Wenn der Benutzer die fertige App nach einem Ordnerwechsel neu startet, dann ist der zuletzt gewählte Ordner aktiv. → `MerkeDatenordner`, `LeseGemerktenDatenordner`, `ErmittleStartDatenordner`; geprüft durch `MerkdateiTests`, `ErmittleStartDatenordnerTests` und manuell (Release: wechseln, neu starten).
- Wenn der Benutzer die Ordnerauswahl abbricht, dann bleibt der bisherige Datenordner aktiv. → `WechsleDatenordner` (Dialog abgebrochen → `null`); manuelle Prüfung.
- Wenn der Benutzer einen Ordner wählt, in dem die App nicht schreiben darf, dann erscheint eine verständliche Meldung und der bisherige Ordner bleibt aktiv. → `PruefeDatenordner` (Zustand NichtBeschreibbar), `WechsleDatenordner`; geprüft durch `PruefeDatenordnerTests` (schreibgeschützter Ordner per ACL-Verweigerung), `FuehreSchreibprobeDurchTests` (Erfolg, Fehlschlag, Anzahl Versuche), `IstNetzwerkpfadTests` und manuell (z. B. `C:\Windows`; NAS-Freigabe nur lesend).
- Wenn der Benutzer einen Ordner wählt, in dem noch keine AndyTours-Daten liegen, dann fragt die App, ob die bisherigen Daten dorthin kopiert werden sollen. → `PruefeDatenordner` (Zustand Leer), `WechsleDatenordner`; geprüft durch `PruefeDatenordnerTests` und manuell.
- Wenn der Benutzer das Kopieren bestätigt, dann liegen danach alle bisherigen Daten im neuen Ordner und die App arbeitet mit ihnen. → `KopiereDatenbestand`, `WechsleDatenordner`; geprüft durch `KopiereDatenbestandTests` (Dateien inkl. Unterordner identisch) und manuell.
- Wenn der Benutzer das Kopieren ablehnt, dann beginnt die App im neuen Ordner mit einem leeren Datenbestand. → `WechsleDatenordner`, `HauptfensterViewModel` (keine Datenbank → leerer Bestand); manuelle Prüfung.
- Wenn die App Daten kopiert, dann zeigt sie den Fortschritt (Anteil, Anzahl Dateien, Datenmenge) an und friert dabei nicht ein; andere Aktionen sind bis zum Ende des Kopierens gesperrt. → `KopiereDatenbestand`, `KopierFortschritt`, `FortschrittsFenster`, `FortschrittsViewModel`, `WechsleDatenordner`; geprüft durch `KopiereDatenbestandTests` (eine Meldung je Datei, letzte Meldung = Gesamtwerte) und manuell (Release: Wechsel auf NAS-Ordner mit Kopieren, Fenster zeigt laufenden Fortschritt, Fenster lassen sich verschieben, Menü des Hauptfensters ist gesperrt).
- Wenn das Kopieren fehlschlägt, dann erscheint eine verständliche Meldung und der bisherige Ordner bleibt aktiv. → `KopiereDatenbestand` (Fehler → Aufräumen, Ausnahme), `WechsleDatenordner`; geprüft durch `KopiereDatenbestandTests` (gesperrte Quelldatei → Ausnahme, Ziel ohne Teilkopie).
- Wenn der Benutzer einen Ordner wählt, in dem bereits AndyTours-Daten liegen, dann erscheint ein Hinweis, dass die dort vorhandenen Daten verwendet werden, mit der Wahl „OK“ oder „Abbrechen“. → `PruefeDatenordner` (Zustand MitDaten), `WechsleDatenordner`; geprüft durch `PruefeDatenordnerTests` und manuell.
- Wenn der Benutzer diesen Hinweis mit „OK“ bestätigt, dann arbeitet die App mit den Daten des gewählten Ordners. → `WechsleDatenordner`, `HauptfensterViewModel`; manuelle Prüfung.
- Wenn der Benutzer diesen Hinweis mit „Abbrechen“ schließt, dann bleibt der bisherige Datenordner aktiv. → `WechsleDatenordner`; manuelle Prüfung.
- Wenn der Benutzer den Datenordner wechselt, dann bleiben die Daten im bisherigen Ordner unverändert erhalten. → `KopiereDatenbestand` (nur lesend auf der Quelle); geprüft durch `KopiereDatenbestandTests` (Quelle nach dem Kopieren unverändert).
- Wenn der gespeicherte Datenordner beim Start nicht erreichbar ist (z. B. NAS ausgeschaltet), dann erscheint eine Meldung mit der Wahl „Erneut versuchen“, „Anderen Ordner wählen“ oder „Beenden“. → `PruefeDatenordner` (Zustand NichtErreichbar), `App.xaml.cs`, `DatenordnerNichtErreichbarFenster`; geprüft durch `PruefeDatenordnerTests` (Pfad auf nicht vorhandenem Laufwerk) und manuell (Release, Merkdatei auf nicht vorhandenen Netzpfad setzen).

## Entscheidungen
- Menüpunkt „Altdaten aus BikeXperience übernehmen…“ wird entfernt (Benutzerwahl a); die Altdaten wurden am 22.10.2026 einmalig per Hilfsprogramm nach `AppDaten/` übernommen (932 Einheiten, 930 Dateien).
- Entwicklung = Debug-Build; Projektordner über die Projektmappe `AndyTours.slnx` gefunden.
- Fachlogik in `AndyTours.Daten`, weil die Tests das WPF-Projekt nicht einbinden können.
- Eigenes Fenster für „nicht erreichbar“, weil das Standard-Meldungsfenster keine eigenen Beschriftungen erlaubt; Rückfragen „kopieren?“ (Ja/Nein) und „vorhandene Daten verwenden?“ (OK/Abbrechen) als Standard-Meldungsfenster.
- Der Wechsel gilt mit Abschluss der Rückfragen, ohne zusätzliches „OK“ im Einstellungsfenster.
- „Anderen Ordner wählen“ beim Start: keine Kopier-Rückfrage (alter Ordner nicht erreichbar); bei leerem Ordner Start mit leerem Bestand.
- „Daten vorhanden“ = Datenbankdatei im Ordner vorhanden.
- Fortschrittsanzeige ohne „Abbrechen“ (nicht in der Spezifikation); Fortschritt je Datei genügt, da die Aufzeichnungen klein sind (Ø ca. 0,3 MB).
- Pufferzeiten für Netzlaufwerke: 1 s bis zum Zurücklesen, bis zu 3 Versuche mit 2 s Pause, Gesamtlimit 20 s.

## Risiken
- Auch mit Pufferzeit kann ein NAS mit stark verzögerter Rechteprüfung die Schreibprobe bestehen und später doch das Schreiben verweigern; das fällt dann erst beim Speichern auf.
- Das Fortschrittsfenster ist modal zum Hauptfenster und lässt sich nicht schließen, solange das Kopieren läuft; ein Schließen der App über die Taskleiste während des Kopierens würde eine Teilkopie hinterlassen (wird beim nächsten Wechsel als „Daten vorhanden“ erkannt, falls die Datenbank schon kopiert war). Gegenmaßnahme: Datenbankdatei als letzte Datei kopieren.
