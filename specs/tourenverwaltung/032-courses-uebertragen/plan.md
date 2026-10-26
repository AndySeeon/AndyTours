# Plan: 032 – Courses übertragen

## Vorgehen
Aus einer Planung bzw. aus den Etappen des Tourdetails wird je Strecke ein Course gebildet: Punkte mit Position, Höhe, Distanz und Zeit sowie die Abbiegungen (Core). Daraus entsteht eine Course-Datei im FIT-Format, die mit dem bereits eingebundenen `Garmin.FIT.Sdk` geschrieben wird (Import-Projekt). Die Datei kommt in den Ordner `Garmin\NewFiles` des Geräts. Den Edge 1040 erreicht die App wie beim Import über `MediaDevices` (ADR 0002), Edge 1000 und Edge 800 über ihr Laufwerk. Nach dem Trennen übernimmt der Edge die Datei unter „Courses“.

Das Fenster „Auf Garmin übertragen“ enthält:
- je Course ein Namensfeld, bei einer Mehrtagestour eine Zeile je Etappe mit GPS-Daten;
- bei einer gefahrenen Strecke die Auswahl des Fahrprofils;
- die Geräteauswahl, wenn mehrere Geräte angeschlossen sind;
- die Schaltfläche „Übertragen“.

Bei einer gefahrenen Strecke ermittelt die App die Abbiegungen mit dem Routingdienst aus den Einstellungen (ADR 0004):
1. Die Spur wird auf Stützpunkte ausgedünnt: Kurven bleiben erhalten, höchstens 1 km Abstand.
2. Der Dienst rechnet die Strecke über die Stützpunkte, in Paketen von 25 Punkten.
3. Ein gelieferter Abbiegepunkt zählt nur, wenn die berechnete Strecke im Umkreis von 150 m vor und nach ihm höchstens 30 m von der Spur abweicht. Er wird auf den nächsten Spurpunkt gesetzt.
4. Die Linie des Course bleibt die gefahrene Spur.

## Dateien
- `src/AndyTours.Core/Modell/Course.cs` (neu) — Name, Coursepunkte und Abbiegungen eines Course
- `src/AndyTours.Core/Modell/Coursepunkt.cs` (neu) — Position, Höhe, Distanz in km und Zeit seit dem Start
- `src/AndyTours.Core/Modell/Courseabbiegung.cs` (neu) — Nummer des Coursepunkts, Richtung, Ausfahrt
- `src/AndyTours.Core/Berechnungen/Geoabstand.cs` (neu) — Abstände in Metern: zwischen zwei Positionen, Punkt zu Strecke und zu Linie (gemeinsam für Course, Stützpunkte und Abbiegungen)
- `src/AndyTours.Core/Berechnungen/BildeCourse.cs` (neu) — `AusPlanung` (Punkte aller Teilstücke, Zeiten bei 15 km/h, Abbiegepunkte dem nächsten Punkt zugeordnet) und `AusVerlauf` (Punkte mit Position, eigene Fahrzeiten, übergebene Abbiegungen); Punkte ohne Position entfallen
- `src/AndyTours.Core/Berechnungen/BildeCoursename.cs` (neu) — Namensvorschlag (Planung: Dateiname ohne Endung bzw. „Planung JJJJ-MM-TT“; Etappe: Datum und Name wie beim GPX-Export ohne „.gpx“) und Dateiname auf dem Gerät (unzulässige Zeichen → „-“, Endung „.fit“)
- `src/AndyTours.Core/Berechnungen/WaehleStuetzpunkte.cs` (neu) — dünnt eine Spur auf Stützpunkte aus (Douglas-Peucker mit 20 m, dazu höchstens 1 km Abstand) und teilt sie in Pakete zu 25 Punkten, die sich am Rand überlappen
- `src/AndyTours.Core/Berechnungen/UebernimmAbbiegungen.cs` (neu) — prüft die gelieferten Abbiegepunkte gegen die Spur (30 m in ±150 m) und ordnet die gültigen dem nächsten Spurpunkt zu; doppelte an derselben Stelle entfallen
- `src/AndyTours.Core/Berechnungen/BeschreibeAbbiegung.cs` (ändern) — kurze Form für den Edge, z. B. „Kreisel 2. Ausf.“ (höchstens 16 Zeichen)
- `src/AndyTours.Core/Berechnungen/BildeRoutinganfrage.cs` (ändern) — zusätzlich eine Anfrage über mehrere Punkte (BRouter `lonlats`, openrouteservice `coordinates`)
- `src/AndyTours.Import/Garmin/SchreibeFitCourse.cs` (neu) — schreibt einen Course als FIT-Datei: Datei-Kennung „Course“, Course mit Name und Sportart Radfahren, Runde, Start-/Stopp-Ereignis, Punkte, Course-Punkte (links, rechts, leicht, scharf, Wenden; Kreisverkehr als allgemeiner Punkt mit Text)
- `src/AndyTours.Import/Garmin/Courseziel.cs` (neu) — ein Gerät als Ziel: Name, „Datei vorhanden?“, „Datei schreiben“
- `src/AndyTours.Import/Garmin/FindeLaufwerksziele.cs` (neu) — Garmin-Laufwerke (Edge 800/1000) mit Ordner `Garmin\NewFiles` als Ziele; Gerätename wie bei `FindeLaufwerksgeraete`
- `src/AndyTours.Import/Garmin/FindeLaufwerksgeraete.cs` (ändern) — `LeseGeraetename` für `FindeLaufwerksziele` zugänglich machen
- `src/AndyTours.App/Funktionen/FindeMtpZiele.cs` (neu) — Garmin-Mediengeräte (Edge 1040) als Ziele: Prüfen und Hochladen in `Garmin\NewFiles` mit Schreibzugriff, Laufwerksspeicher werden wie beim Import übergangen
- `src/AndyTours.App/Funktionen/FindeMtpGeraete.cs` (ändern) — Garmin-Erkennung, Gerätename, Laufwerksspeicher und Gerätefehler für `FindeMtpZiele` intern freigeben
- `src/AndyTours.App/Funktionen/Abbiegungsabruf.cs` (neu) — fragt die Pakete nacheinander beim Routingdienst ab (`Routingabruf`) und liefert die Abbiegungen oder einen Grund, warum sie fehlen
- `src/AndyTours.App/ViewModels/Coursezeile.cs` (neu) — eine Zeile im Übertragungsfenster: Etappe bzw. Planung, Name
- `src/AndyTours.App/ViewModels/CourseUebertragenViewModel.cs` (neu) — Zeilen, Fahrprofil (nur bei gefahrenen Strecken, vorbelegt mit dem Profil der Planungseinstellungen), Geräte, Befehl „Übertragen“ (Abbiegungen ermitteln, Rückfrage bei vorhandener Datei, schreiben), Hinweise; Geräte suchen, Fenster zeigen und Ergebnis als Meldung (gemeinsam für Planung und Tourdetail)
- `src/AndyTours.App/Views/CourseUebertragenFenster.xaml` (neu) — Namenszeilen, Profil- und Geräteauswahl, Hinweis, „Übertragen“ und „Abbrechen“
- `src/AndyTours.App/Views/CourseUebertragenFenster.xaml.cs` (neu) — Code-Behind: Schließen nach dem Übertragen
- `src/AndyTours.App/ViewModels/StreckenkarteViewModel.cs` (ändern) — Befehl „Auf Garmin übertragen…“ (wählbar wie der GPX-Export); neue Konstruktorparameter für Routingeinstellungen und Fahrprofil
- `src/AndyTours.App/ViewModels/TourdetailViewModel.cs` (ändern) — reicht die neuen Parameter an die Streckenkarte weiter
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Tourdetail.cs` (ändern) — übergibt Routingeinstellungen und Planungsprofil
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Import.cs` (ändern) — Text „Kein Garmin-Gerät gefunden …“ für die Übertragung freigeben
- `src/AndyTours.App/Views/TourdetailFenster.xaml` (ändern) — Schaltfläche „Auf _Garmin übertragen…“ neben „Als GPX exportieren…“
- `src/AndyTours.App/ViewModels/TourenplanungViewModel.Datei.cs` (ändern) — Befehl „Auf Garmin übertragen…“ (wählbar wie „Speichern…“)
- `src/AndyTours.App/Views/TourenplanungFenster.xaml` (ändern) — Schaltfläche „Auf _Garmin übertragen…“ nach „Speichern…“
- `tests/AndyTours.Tests/Berechnungen/BildeCourseTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BildeCoursenameTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/WaehleStuetzpunkteTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/UebernimmAbbiegungenTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BeschreibeAbbiegungTests.cs` (ändern) — kurze Form
- `tests/AndyTours.Tests/Berechnungen/BildeRoutinganfrageTests.cs` (ändern) — Anfrage über mehrere Punkte
- `tests/AndyTours.Tests/Garmin/SchreibeFitCourseTests.cs` (neu) — Datei schreiben und mit dem SDK zurücklesen
- `tests/AndyTours.Tests/Garmin/FindeLaufwerkszieleTests.cs` (neu) — mit Testordnern

## Abdeckung der Kriterien

### Übertragen
- Wenn der Benutzer im Planungsfenster „Auf Garmin übertragen…“ wählt, dann wird die geplante Strecke als Course auf den angeschlossenen Edge übertragen. → `TourenplanungViewModel.Datei.cs`, `CourseUebertragenViewModel`, `BildeCourse.AusPlanung`, `SchreibeFitCourse`, `FindeMtpZiele`/`FindeLaufwerksziele`; geprüft durch `BildeCourseTests`, `SchreibeFitCourseTests`, `FindeLaufwerkszieleTests`; manuelle Prüfung: Planung auf den Edge 1040 übertragen, nach dem Trennen unter „Courses“ öffnen
- Wenn der Benutzer im Tourdetail „Auf Garmin übertragen…“ wählt, dann wird die gefahrene Strecke der Einheit als Course übertragen. → `StreckenkarteViewModel`, `BildeCourse.AusVerlauf` (bei bearbeitetem Track die bearbeitete Fassung wie beim GPX-Export); geprüft durch `BildeCourseTests`; manuelle Prüfung: Einheit übertragen und auf dem Edge öffnen
- Wenn der Benutzer überträgt, dann kann er vorher den Namen des Course eingeben; vorgeschlagen wird bei einer gespeicherten Planung der Dateiname ohne Endung, bei einer ungespeicherten „Planung JJJJ-MM-TT“, bei einer Einheit Datum und Name wie beim GPX-Export. → `CourseUebertragenFenster`, `BildeCoursename`; geprüft durch `BildeCoursenameTests`
- Wenn der Benutzer die Namenseingabe abbricht, dann wird nichts übertragen. → „Abbrechen“ bzw. Schließen des Fensters ohne „Übertragen“; manuelle Prüfung: Fenster abbrechen, `Garmin\NewFiles` bleibt unverändert
- Wenn die Übertragung gelungen ist, dann erscheint der Hinweis „Course „X“ übertragen. Er erscheint auf dem Edge nach dem Trennen vom Computer unter „Courses“.“ → `CourseUebertragenViewModel` (Ergebnistext, nach dem Schließen als Meldung; fehlen die Abbiegungen, mit Grund als Warnung); manuelle Prüfung

### Inhalt des Course
- Wenn ein Course auf dem Edge geöffnet wird, dann zeigt er dieselbe Strecke wie die Karte in AndyTours, mit Höhenprofil. → `BildeCourse` (alle Punkte mit Höhe), `SchreibeFitCourse`; geprüft durch `SchreibeFitCourseTests` (Punkte und Höhen zurückgelesen); manuelle Prüfung auf dem Edge
- Wenn eine Planung mit Abbiegepunkten übertragen wird, dann kündigt der Edge beim Nachfahren die Abbiegungen mit ihrer Richtung an. → `BildeCourse.AusPlanung`, `SchreibeFitCourse` (Course-Punkte mit Typ und Text); geprüft durch `BildeCourseTests`, `SchreibeFitCourseTests`; manuelle Prüfung: Course auf dem Edge starten, Abbiegeliste ansehen
- Wenn eine gefahrene Strecke übertragen wird, dann werden ihre Abbiegungen mit dem gewählten Routingdienst ermittelt und der Edge kündigt sie beim Nachfahren an; die Linie des Course bleibt die gefahrene Strecke. → `WaehleStuetzpunkte`, `BildeRoutinganfrage` (mehrere Punkte), `Abbiegungsabruf`, `UebernimmAbbiegungen`, `BildeCourse.AusVerlauf`; geprüft durch `WaehleStuetzpunkteTests`, `BildeRoutinganfrageTests`, `UebernimmAbbiegungenTests`, `BildeCourseTests` (Linie gleich dem Verlauf); manuelle Prüfung: Einheit übertragen, Abbiegeliste auf dem Edge
- Wenn die Abbiegungen einer gefahrenen Strecke nicht ermittelt werden können (z. B. ohne Internet), dann wird der Course ohne Abbiegehinweise übertragen und ein Hinweis nennt den Grund. → `Abbiegungsabruf` (Grund: keine Verbindung bzw. Dienst unvollständig eingestellt), `CourseUebertragenViewModel`; manuelle Prüfung ohne Netzwerk
- Wenn eine gefahrene Strecke abseits von Wegen verläuft, dann enthält der Course dort keine Abbiegehinweise. → `UebernimmAbbiegungen` (Abweichung über 30 m); geprüft durch `UebernimmAbbiegungenTests`

### Mehrtagestour
- Wenn das Tourdetail mehrerer Einheiten geöffnet ist, dann wird jede Etappe mit GPS-Daten als eigener Course übertragen, benannt nach Datum und Name ihrer Einheit; der Name lässt sich je Etappe ändern. → `CourseUebertragenViewModel` (eine `Coursezeile` je Etappe mit Positionen), `BildeCoursename`; geprüft durch `BildeCoursenameTests`; manuelle Prüfung mit einer Mehrtagestour

### Geräte
- Wenn ein Edge 1040, Edge 1000 oder Edge 800 per USB angeschlossen ist, dann lässt sich auf dieses Gerät übertragen. → `FindeMtpZiele` (Edge 1040), `FindeLaufwerksziele` (Edge 1000/800); geprüft durch `FindeLaufwerkszieleTests`; manuelle Prüfung mit dem Edge 1040
- Wenn mehrere Garmin-Geräte angeschlossen sind, dann wählt der Benutzer, auf welches übertragen wird. → Geräteauswahl in `CourseUebertragenFenster` (nur sichtbar bei mehr als einem Ziel); manuelle Prüfung, soweit zwei Geräte vorhanden sind
- Wenn kein Garmin-Gerät angeschlossen ist, dann erscheint der Hinweis „Kein Garmin-Gerät gefunden. Bitte den Edge per USB anschließen.“ → Prüfung vor dem Öffnen des Fensters in beiden Befehlen; manuelle Prüfung ohne Gerät

### Grenz- und Fehlerfälle
- Wenn die Planung weniger als zwei Wegpunkte hat oder keine der angezeigten Einheiten GPS-Daten enthält, dann ist „Auf Garmin übertragen…“ nicht wählbar. → `CanExecute` wie `KannSpeichern` bzw. `HatStrecke`; manuelle Prüfung mit leerer Planung und einer Rollenfahrt
- Wenn auf dem Gerät schon ein noch nicht übernommener Course mit demselben Namen wartet, dann fragt die App, ob er ersetzt werden soll; bei „Nein“ wird dieser Course nicht übertragen. → `Courseziel` („Datei vorhanden?“ in `Garmin\NewFiles`), Rückfrage in `CourseUebertragenViewModel`; geprüft durch `FindeLaufwerkszieleTests`; manuelle Prüfung: zweimal übertragen, ohne das Gerät zu trennen
- Wenn die Übertragung scheitert (z. B. weil das Gerät getrennt wurde), dann erscheint ein Hinweis mit dem Grund, und Planung bzw. Einheit bleiben unverändert. → Fehlerbehandlung in `CourseUebertragenViewModel` (Gerätefehler wie beim Import); die Übertragung liest Planung und Etappen nur; manuelle Prüfung: Kabel vor „Übertragen“ ziehen

## Entscheidungen
- P1a: Das Fahrprofil für die Abbiegungen einer gefahrenen Strecke ist im Übertragungsfenster wählbar und mit dem zuletzt in der Tourenplanung gewählten Profil vorbelegt. Die Wahl wird nicht gespeichert.
- P2a: Ein Kreisverkehr wird ein allgemeiner Course-Punkt mit Text, z. B. „Kreisel 2. Ausf.“; das FIT-Format kennt keinen Kreisverkehr.
- P3a: Eine Planung erhält Zeiten bei 15 km/h, eine gefahrene Strecke ihre eigenen Fahrzeiten (virtueller Partner).
- Die Stützpunkte und die Schwellen (20 m, 1 km, 25 Punkte je Anfrage, 30 m in ±150 m) sind feste Werte im Code. Sie werden an echten Touren nachjustiert und nicht in den Einstellungen angeboten.
- Der Dateiname auf dem Gerät ist der Course-Name mit „.fit“. Die Rückfrage betrifft daher eine gleichnamige Datei in `Garmin\NewFiles`.

## Risiken
- Ein bereits übernommener Course gleichen Namens wird vom Edge ersetzt (bei der manuellen Prüfung mit dem Edge 1040 festgestellt).
- Bei einer gefahrenen Strecke kann der Routingdienst an Stützpunkten Wendehinweise oder Abbiegungen auf Nachbarwegen liefern. Die Abweichungsprüfung fängt die meisten ab; vereinzelte Fehlhinweise sind möglich.
- Bei langen Touren dauert das Ermitteln der Abbiegungen mehrere Sekunden bis wenige Minuten (eine Anfrage je 25 Stützpunkte). Das Fenster zeigt so lange „Abbiegungen werden ermittelt …“.
- Sehr lange Courses: Der Edge verarbeitet große Dateien langsam. Die Spur wird trotzdem nicht ausgedünnt, damit sie der Karte in AndyTours entspricht.
