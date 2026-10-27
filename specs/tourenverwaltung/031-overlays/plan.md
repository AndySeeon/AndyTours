# Plan: 031 – Overlays

## Vorgehen
Overlays und ihre Ordner liegen in zwei neuen Tabellen der Datenbank (Schemaversion 4). Zu jedem Overlay werden Name, Ordner, eigene Farbe, der Ein/Aus-Zustand und die Streckenpunkte gespeichert, die Punkte als kompakter Binärwert. Die Datenbank wird automatisch gesichert und zieht beim Wechsel des Datenordners mit um, also gilt beides auch für die Overlays. Die gemeinsame Overlay-Farbe ist eine Einstellung im Datenbestand.

Die Fachlogik liegt in Core, ohne Oberfläche: GPX-Strecke lesen, Ordnernamen prüfen, Verschiebeziel prüfen, Ordnerinhalt sammeln, Ordnerpfade bilden und das Rechteck um ein Overlay bestimmen. Eine GPX-Datei kann mehrere Abschnitte enthalten (`trkseg`; ohne Track die `rte`). Jeder Abschnitt wird eine eigene Linie, damit keine falschen Verbindungsstriche entstehen. Eine Planung aus 030 ist eine normale GPX-Datei mit einem `trkseg` je Teilstück.

In der App gibt es einen gemeinsamen Overlay-Stand (`Overlaybestand`). Er hält die eingeschalteten Overlays und die gemeinsame Farbe und meldet Änderungen über Ereignisse, nach demselben Muster wie `Kartenabruf.Fehlgeschlagen`. Die Kartenfenster Tourdetail (gilt auch für die Mehrtagestour) und Tourenplanung legen eine Overlay-Ebene direkt über die Kartenausschnitte, also unter Strecke, Wegpunkte und Abbiegepunkte. Bei jeder Meldung zeichnen sie die Ebene neu und melden sich beim Schließen wieder ab.

Die Verwaltung ist ein nicht modales Fenster, das immer nur einmal offen ist. Es zeigt einen Ordnerbaum mit dreistufigen Kontrollkästchen und eine Leiste mit „Ordner anlegen“, „GPX hinzufügen…“, „Umbenennen“, „Verschieben nach…“, „Farbe“ und „Löschen“. Dieselben Befehle stehen auch im Kontextmenü. Jede Änderung wird sofort gespeichert.

## Dateien
- `src/AndyTours.Core/Modell/Overlayordner.cs` (neu) — Ordner: Id, übergeordneter Ordner (oder keiner = oberste Ebene), Name
- `src/AndyTours.Core/Modell/Overlay.cs` (neu) — Overlay: Id, Ordner, Name, eigene Farbe (optional), eingeschaltet, Abschnitte (Listen von Positionen)
- `src/AndyTours.Core/Modell/Overlayfarbe.cs` (neu) — Aufzählung der wählbaren Farben (Violett, Blau, Hellblau, Türkis, Grün, Gelb, Orange, Rot, Pink, Braun, Grau, Schwarz)
- `src/AndyTours.Core/Berechnungen/LeseGpxStrecke.cs` (neu) — liest die Abschnitte einer GPX-Datei (Track-Segmente, sonst Routen); `null`, wenn die Datei nicht lesbar ist oder keinen Punkt enthält
- `src/AndyTours.Core/Berechnungen/PruefeOrdnername.cs` (neu) — Hinweis bei leerem Namen oder bei einem Namen, der im selben übergeordneten Ordner schon vorkommt (Groß-/Kleinschreibung egal, der umbenannte Ordner selbst ausgenommen); sonst `null`
- `src/AndyTours.Core/Berechnungen/PruefeVerschiebeziel.cs` (neu) — `false`, wenn ein Ordner in sich selbst oder in einen seiner Unterordner verschoben werden soll
- `src/AndyTours.Core/Berechnungen/SammleOrdnerinhalt.cs` (neu) — alle Unterordner und Overlays eines Ordners, auf allen Ebenen (für Ein-/Ausschalten und die Rückfrage beim Löschen)
- `src/AndyTours.Core/Berechnungen/BildeOrdnerpfade.cs` (neu) — Ordner mit ihrem vollen Pfad („Alpen › Tirol“), sortiert, für die Auswahl bei „Verschieben nach…“
- `src/AndyTours.Core/Berechnungen/UmschliesseOverlay.cs` (neu) — Süd-West- und Nord-Ost-Ecke aller Punkte eines Overlays
- `src/AndyTours.Daten/ErstelleSchema.cs` (ändern) — Tabellen `Overlayordner` (Eltern-Verweis mit ON DELETE CASCADE) und `Overlay` (Ordner-Verweis mit ON DELETE CASCADE, Punkte als BLOB), Schemaversion 4
- `src/AndyTours.Daten/OverlayPunkte.cs` (neu) — `Verpacken`/`Entpacken`: Abschnitte ↔ Binärwert
- `src/AndyTours.Daten/OverlayordnerTabelle.cs` (neu) — Laden, Einfuegen, Umbenennen, Verschieben, Loeschen
- `src/AndyTours.Daten/OverlaysTabelle.cs` (neu) — Laden, Einfuegen, Umbenennen, Verschieben, SetzeFarbe, SetzeEingeschaltet (mehrere auf einmal), Loeschen
- `src/AndyTours.Daten/OverlayfarbeEinstellung.cs` (neu) — Laden/Speichern der gemeinsamen Farbe, Standard Violett
- `src/AndyTours.Daten/Einstellungsschluessel.cs` (ändern) — Schlüssel `Overlay.Farbe`
- `src/AndyTours.App/Funktionen/Overlaybestand.cs` (neu) — gemeinsamer Stand: eingeschaltete Overlays, gemeinsame Farbe, Ereignisse `Geaendert` und `GanzesOverlayAngefordert`; wird aus dem Datenordner geladen
- `src/AndyTours.App/Funktionen/Overlaydarstellung.cs` (neu) — Farbwerte und Anzeigenamen der Overlay-Farben; erzeugt und füllt die Overlay-Ebene (3 px, leicht durchscheinend)
- `src/AndyTours.App/ViewModels/OverlaysViewModel.cs` (neu) — Baum, Befehle, Hinweise, Rückfragen, Speichern
- `src/AndyTours.App/ViewModels/Overlayfarbeintrag.cs` (neu) — Eintrag der Farbauswahl (Name, Farbfeld) für Verwaltung und Einstellungen
- `src/AndyTours.App/ViewModels/Overlayknoten.cs` (neu) — Baumknoten (Ordner oder Overlay) mit dreistufigem Kontrollkästchen
- `src/AndyTours.App/Views/OverlaysFenster.xaml` (neu) — Fenster „Overlays“ mit Leiste, Baum und Hinweisen
- `src/AndyTours.App/Views/OverlaysFenster.xaml.cs` (neu) — Doppelklick, Kontextmenü, Dateiauswahl, Fensterlebenszeit
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Overlays.cs` (neu) — Befehl „Tourenplanung → Overlays…“, nur ein Fenster gleichzeitig
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — `LadeDatenbank` lädt auch den Overlay-Stand (Start, Datenordnerwechsel, Wiederherstellung)
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Menüeintrag „_Overlays…“ unter „Tourenplanung“
- `src/AndyTours.App/Views/TourdetailFenster.xaml.cs` (ändern) — Overlay-Ebene über der Kartenebene, Ereignisse abonnieren und beim Schließen abmelden; Schaltfläche „Overlays…“ öffnet die Verwaltung über den Befehl des Hauptfensters
- `src/AndyTours.App/Views/TourdetailFenster.xaml` (ändern) — Schaltfläche „O_verlays…“ in der Leiste der Karte
- `src/AndyTours.App/Views/TourenplanungFenster.xaml.cs` (ändern) — ebenso
- `src/AndyTours.App/Views/TourenplanungFenster.xaml` (ändern) — Schaltfläche „O_verlays…“ in der Leiste
- `src/AndyTours.App/ViewModels/EinstellungenViewModel.cs` (ändern) — gemeinsame Overlay-Farbe lesen und speichern, an `Overlaybestand` melden
- `src/AndyTours.App/Views/EinstellungenFenster.xaml` (ändern) — Gruppe „Overlays“ mit Farbauswahl
- `tests/AndyTours.Tests/Berechnungen/LeseGpxStreckeTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/PruefeOrdnernameTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/PruefeVerschiebezielTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/SammleOrdnerinhaltTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BildeOrdnerpfadeTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/UmschliesseOverlayTests.cs` (neu)
- `tests/AndyTours.Tests/Daten/OverlaysTests.cs` (neu) — Tabellen, Punkte, kaskadierendes Löschen, Erhalt nach erneutem Öffnen
- `tests/AndyTours.Tests/Daten/OverlayfarbeTests.cs` (neu)

## Abdeckung der Kriterien

### Verwaltung öffnen
- Wenn der Benutzer „Tourenplanung → Overlays…“ wählt, dann öffnet sich die Overlay-Verwaltung mit dem Ordnerbaum und den Overlays, jeweils mit einem Kontrollkästchen zum Einblenden. → `Hauptfenster.xaml`, `HauptfensterViewModel.Overlays.cs`, `OverlaysFenster.xaml`, `Overlayknoten.cs`; manuelle Prüfung: Menü wählen, Baum mit Kontrollkästchen erscheint
- Wenn noch keine Overlays vorhanden sind, dann zeigt die Verwaltung einen Hinweis, wie man Overlays hinzufügt. → `OverlaysViewModel` (`IstLeer`), `OverlaysFenster.xaml`; manuelle Prüfung mit leerem Datenbestand

### Overlays aufnehmen
- Wenn der Benutzer GPX-Dateien hinzufügt, dann erscheinen sie als Overlays im gewählten Ordner, benannt nach dem Dateinamen ohne Endung. → `OverlaysViewModel` (GPX hinzufügen), `LeseGpxStrecke`, `OverlaysTabelle.Einfuegen`; geprüft durch `LeseGpxStreckeTests`, `OverlaysTests` (`dotnet test AndyTours.slnx`); manuelle Prüfung: zwei GPX-Dateien in einen Ordner hinzufügen
- Wenn ein Overlay hinzugefügt wurde, dann bleibt es erhalten, auch wenn die ursprüngliche Datei danach verschoben oder gelöscht wird. → die Punkte stehen in der Datenbank (`OverlayPunkte`, `OverlaysTabelle`); geprüft durch `OverlaysTests` (Punkte nach erneutem Öffnen gleich)
- Wenn eine Planung aus der Tourenplanung gespeichert wurde, dann lässt sich diese Datei wie jede andere GPX-Datei als Overlay hinzufügen. → `LeseGpxStrecke`; geprüft durch `LeseGpxStreckeTests` (Datei aus `PlanungsGpx.Schreiben` ergibt einen Abschnitt je Teilstück)
- Wenn eine Datei keine Strecke enthält oder nicht lesbar ist, dann wird sie nicht aufgenommen und ein Hinweis nennt die Datei. → `LeseGpxStrecke` (liefert `null`), `OverlaysViewModel` (Hinweis); geprüft durch `LeseGpxStreckeTests` (kein XML, nur Wegpunkte, leere Segmente, ungültige Koordinaten)
- Wenn mehrere Dateien auf einmal hinzugefügt werden und einzelne davon fehlerhaft sind, dann werden die übrigen trotzdem aufgenommen. → `OverlaysViewModel` (jede Datei für sich, der Hinweis sammelt die fehlerhaften); manuelle Prüfung: eine gültige und eine leere Datei zusammen hinzufügen

### Ordner verwalten
- Wenn der Benutzer einen Ordner anlegt, dann erscheint er im gewählten Ordner als Unterordner oder, wenn keiner gewählt ist, auf oberster Ebene. → `OverlaysViewModel` (Ordner anlegen), `OverlayordnerTabelle.Einfuegen`; geprüft durch `OverlaysTests`; manuelle Prüfung mit und ohne Auswahl
- Wenn der Benutzer einen Ordner umbenennt, dann erscheint er unter dem neuen Namen. → `OverlayordnerTabelle.Umbenennen`; geprüft durch `OverlaysTests`
- Wenn ein Ordnername leer ist oder im selben übergeordneten Ordner schon vorkommt, dann wird er nicht übernommen und ein Hinweis erscheint. → `PruefeOrdnername`; geprüft durch `PruefeOrdnernameTests` (leer, nur Leerzeichen, gleicher Name in anderer Schreibweise, gleicher Name in anderem Ordner erlaubt, eigener Name beim Umbenennen erlaubt)
- Wenn der Benutzer einen Ordner löscht, der Overlays oder Unterordner enthält, dann kommt eine Rückfrage; nach dem Bestätigen sind der Ordner und sein gesamter Inhalt gelöscht. → `OverlaysViewModel` (Rückfrage mit Anzahl aus `SammleOrdnerinhalt`), `OverlayordnerTabelle.Loeschen` (kaskadierend); geprüft durch `SammleOrdnerinhaltTests`, `OverlaysTests` (Unterordner und Overlays mitgelöscht)
- Wenn der Benutzer ein Overlay oder einen Ordner in einen anderen Ordner verschiebt, dann steht es danach dort. → „Verschieben nach…“ mit `EintragWaehlenFenster` und `BildeOrdnerpfade` (Eintrag „(oberste Ebene)“), `OverlaysTabelle.Verschieben`, `OverlayordnerTabelle.Verschieben`; geprüft durch `BildeOrdnerpfadeTests`, `OverlaysTests`
- Wenn der Benutzer einen Ordner in sich selbst oder in einen seiner Unterordner verschieben will, dann geschieht nichts. → `PruefeVerschiebeziel`; die Auswahl bietet diese Ordner gar nicht erst an; geprüft durch `PruefeVerschiebezielTests`

### Overlays bearbeiten
- Wenn der Benutzer ein Overlay umbenennt, dann erscheint es unter dem neuen Namen. → `OverlaysTabelle.Umbenennen`; geprüft durch `OverlaysTests`; ein leerer Name wird nicht übernommen
- Wenn der Benutzer ein Overlay löscht, dann kommt eine Rückfrage; nach dem Bestätigen ist es entfernt und verschwindet von allen Karten. → `OverlaysViewModel` (Rückfrage), `OverlaysTabelle.Loeschen`, `Overlaybestand.Geaendert`; geprüft durch `OverlaysTests`; manuelle Prüfung: eingeschaltetes Overlay bei offenem Tourdetail löschen

### Einblenden
- Wenn der Benutzer ein Overlay einschaltet, dann erscheint seine Strecke als Linie auf den Karten im Tourdetail, in der Mehrtagestour und in der Tourenplanung. → `OverlaysTabelle.SetzeEingeschaltet`, `Overlaybestand`, `Overlaydarstellung`, `TourdetailFenster.xaml.cs`, `TourenplanungFenster.xaml.cs`; manuelle Prüfung in allen drei Karten
- Wenn der Benutzer einen Ordner einschaltet, dann werden alle Overlays darin einschließlich der Unterordner eingeschaltet; ausschalten wirkt ebenso auf den ganzen Inhalt. → `Overlayknoten` (dreistufig), `SammleOrdnerinhalt`; geprüft durch `SammleOrdnerinhaltTests`; manuelle Prüfung mit Unterordner
- Wenn der Benutzer ein Overlay ausschaltet, dann verschwindet es von den Karten. → wie beim Einschalten; manuelle Prüfung
- Wenn eine Karte bereits geöffnet ist, während Overlays ein- oder ausgeschaltet werden, dann zeigt sie die Änderung sofort. → `Overlaybestand.Geaendert`, Neuzeichnen in beiden Kartenfenstern; manuelle Prüfung: Tourdetail und Tourenplanung offen lassen und umschalten
- Wenn Overlays eingeblendet sind, dann liegen sie unter der eigenen Strecke, den Wegpunkten und Abbiegepunkten und verdecken diese nicht. → Overlay-Ebene an Position 1 direkt über den Kartenausschnitten; manuelle Prüfung: Overlay entlang einer Tour bzw. Planung einblenden
- Wenn der Benutzer in der Verwaltung ein Overlay doppelt anklickt, dann zeigen die geöffneten Karten dessen ganze Strecke. → `OverlaysFenster.xaml.cs`, `Overlaybestand.GanzesOverlayAngefordert`, `UmschliesseOverlay`; geprüft durch `UmschliesseOverlayTests`; manuelle Prüfung
- Wenn die App neu gestartet wird, dann sind dieselben Overlays wieder eingeschaltet. → Spalte „Eingeschaltet“ in `Overlay`, `Overlaybestand` wird in `LadeDatenbank` geladen; geprüft durch `OverlaysTests` (Zustand nach erneutem Öffnen); manuelle Prüfung: Neustart

### Farbe
- Wenn ein Overlay keine eigene Farbe hat, dann erscheint es in der gemeinsamen Overlay-Farbe, voreingestellt Violett. → `OverlayfarbeEinstellung` (Standard Violett), `Overlaydarstellung`; geprüft durch `OverlayfarbeTests`
- Wenn der Benutzer in den Einstellungen die gemeinsame Overlay-Farbe ändert, dann erscheinen alle Overlays ohne eigene Farbe in der neuen Farbe. → `EinstellungenViewModel`, `EinstellungenFenster.xaml`, `Overlaybestand.Geaendert`; geprüft durch `OverlayfarbeTests` (Speichern/Laden); manuelle Prüfung bei offener Karte
- Wenn der Benutzer einem Overlay eine eigene Farbe gibt, dann erscheint es in dieser Farbe, unabhängig von der gemeinsamen Farbe. → `OverlaysTabelle.SetzeFarbe`, `Overlaydarstellung`; geprüft durch `OverlaysTests`; manuelle Prüfung
- Wenn der Benutzer die eigene Farbe eines Overlays zurücksetzt, dann erscheint es wieder in der gemeinsamen Farbe. → Eintrag „Gemeinsame Farbe“ setzt die eigene Farbe auf leer; geprüft durch `OverlaysTests`; manuelle Prüfung

## Entscheidungen
- Overlays stehen in der Datenbank statt als GPX-Kopien im Datenordner (P1a). So sind sie in der automatischen Sicherung, die nur die Datenbank sichert.
- Farben stammen aus einer festen Liste von zwölf Farben (P2a). Der erste Eintrag „Gemeinsame Farbe“ setzt die eigene Farbe zurück. Kein Windows-Forms-Farbdialog.
- Verschoben wird über das Kontextmenü bzw. die Schaltfläche „Verschieben nach…“ und das vorhandene Fenster „Eintrag wählen“ (P3a). Ziehen und Ablegen gibt es nicht.
- Die Overlay-Linie ist 3 px breit und leicht durchscheinend (P4a). Die eigene Strecke hat 4 px und ist deckend.
- Overlays verweisen über die Kennung auf ihren Ordner, so bleibt das Umbenennen eines Ordners folgenlos. Overlay-Namen dürfen doppelt vorkommen, nur Ordnernamen nicht.
- Die Verwaltung ist nicht modal und nur einmal offen. Sie öffnet sich über „Tourenplanung → Overlays…“ und über die Schaltfläche „O_verlays…“ (Alt+V) in jeder Karte (K1a); ein erneuter Aufruf holt das offene Fenster nach vorn. Sie gehört zum Hauptfenster und bleibt offen, wenn eine Karte geschlossen wird.

## Risiken
- Sehr große GPX-Dateien mit mehreren 10 000 Punkten können viele eingeschaltete Overlays beim Zeichnen bremsen. Gegenmaßnahme bei Bedarf: Punkte beim Hinzufügen ausdünnen. Das wird erst gemacht, wenn es tatsächlich langsam ist.
- Schemaversion 4: Eine ältere AndyTours-Version lehnt Sicherungen dieser Version als „neuer“ ab. Das ist gewollt und schon so umgesetzt.
