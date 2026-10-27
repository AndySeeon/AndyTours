# Plan: 030 – Tourenplanung auf der Karte

## Vorgehen
Ein neues, nicht modales Fenster „Tour planen“ zeigt eine Mapsui-Karte (wie die Streckenkarte, ADR 0003). Die
Kartenebenen samt Zwischenspeicher werden dafür aus `TourdetailFenster.xaml.cs` in eine gemeinsame Funktion
herausgelöst. Die Planung besteht aus den Wegpunkten des Benutzers, dem Fahrprofil und je einem Teilstück zwischen
zwei benachbarten Wegpunkten. Ein Teilstück enthält Punkte mit Höhe, seine Abbiegepunkte und die Angabe, ob es eine
Luftlinie ist.

Alle Änderungen an den Wegpunkten (anhängen, einfügen, verschieben, entfernen) sind reine Fachfunktionen im Core. Sie
liefern eine neue Planung, in der die betroffenen Teilstücke als „noch zu ermitteln“ markiert sind. Das ViewModel
ermittelt diese Teilstücke danach nacheinander und im Hintergrund über den in den Einstellungen gewählten
Routingdienst (BRouter voreingestellt oder openrouteservice, ADR 0004) und übergibt dabei das Fahrprofil. Antwortet der Dienst nicht, wird das Teilstück eine Luftlinie, und ein Hinweis erscheint. Die Antwort
jedes Dienstes wandelt eine eigene Core-Funktion in ein Teilstück um. Dabei entstehen die Abbiegepunkte aus den
Fahranweisungen des Dienstes, Anweisungen für „geradeaus“ fallen weg. Länge und Höhenmeter rechnet der Core aus den
Teilstücken.

Gespeichert wird eine GPX-1.1-Datei mit:
- der Strecke als Track (ein Segment je Teilstück, mit Höhe),
- den Abbiegepunkten als Wegpunkte (`wpt`) mit ihrer Richtung,
- einer AndyTours-Erweiterung, die Fahrprofil, Wegpunkte und Luftlinien festhält.

Andere Programme übergehen die Erweiterung. AndyTours erkennt daran beim Öffnen eine eigene Planung.

Ordner, Fahrprofil und Kartenausschnitt der Planung werden in der Tabelle `Einstellung` gemerkt.

Die Maus wertet das Code-Behind des Fensters aus, weil Mapsui keine Bindungen kennt:
- ein Klick ohne Bewegung neben die Strecke hängt einen Wegpunkt an,
- Ziehen auf einem Wegpunkt verschiebt ihn,
- Ziehen auf der Linie fügt einen Wegpunkt ein,
- ein Rechtsklick auf einen Wegpunkt entfernt ihn,
- jedes andere Ziehen verschiebt die Karte.

## Dateien
- `src/AndyTours.Core/Modell/Fahrprofil.cs` (neu) — Enum Rennrad, Trekking, Mountainbike mit den Anzeigetexten der Spec
- `src/AndyTours.Core/Modell/Abbiegerichtung.cs` (neu) — Enum: links, rechts, leicht/scharf links/rechts, Kreisverkehr, wenden
- `src/AndyTours.Core/Modell/Abbiegepunkt.cs` (neu) — Position, Richtung, Ausfahrt im Kreisverkehr
- `src/AndyTours.Core/Modell/Teilstueck.cs` (neu) — Punkte (Position, Höhe), Abbiegepunkte, `IstLuftlinie`
- `src/AndyTours.Core/Modell/Tourenplanung.cs` (neu) — Fahrprofil, Wegpunkte, Teilstücke (je Lücke eines oder `null` = noch zu ermitteln)
- `src/AndyTours.Core/Modell/Routingdienst.cs` (neu) — Enum BRouter, Openrouteservice
- `src/AndyTours.Core/Modell/Routingeinstellungen.cs` (neu) — gewählter Dienst, BRouter-Adresse (Standard `https://brouter.de/brouter`), Zugangsschlüssel für openrouteservice
- `src/AndyTours.Core/Modell/Routinganfrage.cs` (neu) — Adresse, Methode, Inhalt und Kopfzeilen einer Anfrage
- `src/AndyTours.Core/Modell/Planungswerte.cs` (neu) — Länge in km, Höhenmeter bergauf und bergab
- `src/AndyTours.Core/Modell/Planungseinstellungen.cs` (neu) — gemerktes Fahrprofil, Kartenausschnitt (Südwest/Nordost) und Ordner
- `src/AndyTours.Core/Berechnungen/AenderePlanung.cs` (neu) — `Anhaengen`, `Einfuegen`, `Verschieben`, `Entfernen`, `EntferneLetzten`, `WechsleProfil`, `SetzeTeilstueck`; markiert nur die betroffenen Teilstücke als zu ermitteln
- `src/AndyTours.Core/Berechnungen/FindeEinfuegestelle.cs` (neu) — sucht zu einer Position auf der Linie das Teilstück, hinter dessen Anfangswegpunkt eingefügt wird
- `src/AndyTours.Core/Berechnungen/ErstelleLuftlinie.cs` (neu) — Teilstück als gerade Linie ohne Abbiegepunkte
- `src/AndyTours.Core/Berechnungen/BerechnePlanungswerte.cs` (neu) — Länge (Großkreisabstand) und Höhenmeter aus den ermittelten Teilstücken
- `src/AndyTours.Core/Berechnungen/BeschreibeAbbiegung.cs` (neu) — Text eines Abbiegepunkts, z. B. „Rechts“, „Leicht links“, „Kreisverkehr 2. Ausfahrt“
- `src/AndyTours.Core/Berechnungen/BildeRoutinganfrage.cs` (neu) — Anfrage je Dienst mit zugeordnetem Fahrprofil (BRouter: `fastbike`/`trekking`/`mtb`, GeoJSON, `timode`; openrouteservice: `cycling-road`/`cycling-regular`/`cycling-mountain`, Höhen und Anweisungen, Schlüssel in der Kopfzeile)
- `src/AndyTours.Core/Berechnungen/LeseBRouterAntwort.cs` (neu) — GeoJSON mit Höhen und Abbiegehinweisen (`voicehints`) → Teilstück; Hinweise „geradeaus“ fallen weg
- `src/AndyTours.Core/Berechnungen/LeseOpenrouteserviceAntwort.cs` (neu) — GeoJSON mit Höhen und Fahranweisungen → Teilstück; Anweisungen „geradeaus“, Start und Ziel fallen weg
- `src/AndyTours.Core/Berechnungen/PlanungsGpx.cs` (neu) — `Schreiben` (GPX mit Track, Abbiegepunkten, AndyTours-Erweiterung) und `Lesen` (`null` bei fremder oder unlesbarer Datei)
- `src/AndyTours.Core/Berechnungen/BildePlanungsdateiname.cs` (neu) — „Planung JJJJ-MM-TT.gpx“
- `src/AndyTours.Daten/Einstellungsschluessel.cs` (ändern) — Schlüssel `Tourenplanung.Profil`, `Tourenplanung.Ausschnitt`, `Tourenplanung.Ordner`, `Routing.Dienst`, `Routing.BRouterAdresse`, `Routing.OpenrouteserviceSchluessel`
- `src/AndyTours.Daten/PlanungseinstellungenEinstellung.cs` (neu) — `Laden`/`Speichern` der Planungseinstellungen
- `src/AndyTours.Daten/RoutingEinstellung.cs` (neu) — `Laden`/`Speichern` der Routingeinstellungen; ohne Eintrag BRouter mit Standardadresse
- `src/AndyTours.App/Funktionen/Kartenebenen.cs` (neu) — erzeugt die Kartenebene einer Kartenart mit Zwischenspeicher (aus dem Tourdetail herausgelöst)
- `src/AndyTours.App/Funktionen/Routingabruf.cs` (neu) — schickt die gebildete Anfrage ab (Kennung „AndyTours“, Zeitlimit, Abbruch) und gibt die Antwort an die passende Lesefunktion; Fehler → `null`
- `src/AndyTours.App/ViewModels/TourenplanungViewModel.cs` (neu) — Planung, Fahrprofil, Werte, Hinweis, Befehle „Letzten Punkt entfernen“, „Alles löschen“, Ermittlung der offenen Teilstücke, Änderungsmerker
- `src/AndyTours.App/ViewModels/TourenplanungViewModel.Datei.cs` (neu) — „Speichern…“, „Öffnen…“, Rückfrage bei ungespeicherten Änderungen
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.Tourenplanung.cs` (neu) — Befehl „Tourenplanung → Tour planen…“, Einstellungen laden/speichern, Gebiet der zuletzt gefahrenen Einheit
- `src/AndyTours.App/Views/TourenplanungFenster.xaml` (neu) — Karte, Leiste mit Fahrprofil, Werten, Hinweis und Schaltflächen
- `src/AndyTours.App/Views/TourenplanungFenster.xaml.cs` (neu) — Ebenen (Strecke, Wegpunkte, Abbiegepunkte), Mausauswertung, Ausschnitt merken, Schließen abfangen
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Menüeintrag „Tourenplanung → Tour _planen…“
- `src/AndyTours.App/Views/TourdetailFenster.xaml.cs` (ändern) — nutzt `Kartenebenen` statt der eigenen Methode
- `src/AndyTours.App/ViewModels/EinstellungenViewModel.cs` (ändern) — Routingdienst wählen, BRouter-Adresse ändern, Zugangsschlüssel eingeben; Speichern sofort wie die übrigen Einstellungen
- `src/AndyTours.App/Views/EinstellungenFenster.xaml` (ändern) — Gruppe „Routing (Tourenplanung)“: Dienst, Serveradresse, Zugangsschlüssel als maskiertes Feld
- `tests/AndyTours.Tests/Berechnungen/AenderePlanungTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/FindeEinfuegestelleTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/ErstelleLuftlinieTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BerechnePlanungswerteTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BeschreibeAbbiegungTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BildeRoutinganfrageTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/LeseBRouterAntwortTests.cs` (neu) — mit selbst geschriebenen, kurzen Beispielantworten
- `tests/AndyTours.Tests/Berechnungen/LeseOpenrouteserviceAntwortTests.cs` (neu) — ebenso
- `tests/AndyTours.Tests/Berechnungen/PlanungsGpxTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BildePlanungsdateinameTests.cs` (neu)
- `tests/AndyTours.Tests/Daten/PlanungseinstellungenTests.cs` (neu)
- `tests/AndyTours.Tests/Daten/RoutingEinstellungTests.cs` (neu)

## Abdeckung der Kriterien
- Wenn der Benutzer im Menü „Tourenplanung → Tour planen…“ wählt, dann öffnet sich ein Fenster mit einer Karte. → umgesetzt in `Hauptfenster.xaml`, `HauptfensterViewModel.Tourenplanung.cs`; manuelle Prüfung: Menü wählen, Fenster mit Karte erscheint
- Wenn das Planungsfenster öffnet, dann zeigt die Karte den zuletzt in der Planung gezeigten Ausschnitt; beim ersten Mal zeigt sie das Gebiet der zuletzt gefahrenen Einheit. → umgesetzt in `PlanungseinstellungenEinstellung.cs`, `HauptfensterViewModel.Tourenplanung.cs`, `TourenplanungFenster.xaml.cs`; geprüft durch `PlanungseinstellungenTests` / manuelle Prüfung: Ausschnitt verschieben, Fenster schließen und neu öffnen; bei leerer Einstellung zeigt die Karte das Gebiet der letzten Tour
- Wenn der Benutzer in die Karte neben die Strecke klickt, dann wird dort ein Wegpunkt gesetzt und als neuer letzter Punkt an die Planung angehängt. → umgesetzt in `AenderePlanung.Anhaengen`, `TourenplanungFenster.xaml.cs`; geprüft durch `AenderePlanungTests` / manuelle Prüfung: zweimal neben die Strecke klicken, Punkte erscheinen am Ende
- Wenn die Planung mindestens zwei Wegpunkte hat, dann verbindet eine Linie die Wegpunkte in ihrer Reihenfolge und folgt dabei den Wegen, die das gewählte Fahrprofil vorsieht. → umgesetzt in `BildeRoutinganfrage.cs`, `Routingabruf.cs`, `LeseBRouterAntwort.cs`, `LeseOpenrouteserviceAntwort.cs`, `TourenplanungViewModel.cs`; geprüft durch `LeseBRouterAntwortTests`, `LeseOpenrouteserviceAntwortTests` / manuelle Prüfung: Linie folgt den Wegen
- Wenn der Benutzer das Fahrprofil wählt, dann stehen „Rennrad (nur Straße)“, „Trekking/Tourenrad“ und „Mountainbike (bevorzugt offroad)“ zur Auswahl. → umgesetzt in `Fahrprofil.cs`, `TourenplanungFenster.xaml`; manuelle Prüfung: Auswahlliste zeigt die drei Texte
- Wenn das Profil „Rennrad (nur Straße)“ gewählt ist, dann führt die Strecke nur über asphaltierte Straßen und Radwege, soweit möglich. → umgesetzt in `BildeRoutinganfrage.cs` (`fastbike` bzw. `cycling-road`); geprüft durch `BildeRoutinganfrageTests` / manuelle Prüfung: Strecke durch ein Waldgebiet planen, sie bleibt auf Asphalt
- Wenn das Profil „Trekking/Tourenrad“ gewählt ist, dann führt die Strecke über Straßen, Radwege und gut befahrbare Schotterwege. → umgesetzt in `BildeRoutinganfrage.cs` (`trekking` bzw. `cycling-regular`); geprüft durch `BildeRoutinganfrageTests` / manuelle Prüfung: dieselbe Strecke nimmt Schotterwege
- Wenn das Profil „Mountainbike (bevorzugt offroad)“ gewählt ist, dann bevorzugt die Strecke Feld-, Wald- und Schotterwege. → umgesetzt in `BildeRoutinganfrage.cs` (`mtb` bzw. `cycling-mountain`); geprüft durch `BildeRoutinganfrageTests` / manuelle Prüfung: dieselbe Strecke bevorzugt Feld- und Waldwege
- Wenn der Benutzer das Fahrprofil während der Planung wechselt, dann wird die ganze Strecke mit dem neuen Profil neu ermittelt; die Wegpunkte bleiben an ihrer Stelle. → umgesetzt in `AenderePlanung.WechsleProfil`, `TourenplanungViewModel.cs`; geprüft durch `AenderePlanungTests` (alle Teilstücke offen, Wegpunkte gleich) / manuelle Prüfung: Profil wechseln
- Wenn das Planungsfenster öffnet, dann ist das zuletzt gewählte Fahrprofil eingestellt, beim ersten Mal „Trekking/Tourenrad“. → umgesetzt in `PlanungseinstellungenEinstellung.cs`, `HauptfensterViewModel.Tourenplanung.cs`; geprüft durch `PlanungseinstellungenTests` (ohne Eintrag Trekking) / manuelle Prüfung: Profil wählen, Fenster neu öffnen
- Wenn der Benutzer eine gespeicherte Planung öffnet, dann ist das Fahrprofil eingestellt, mit dem sie gespeichert wurde. → umgesetzt in `PlanungsGpx.Lesen`, `TourenplanungViewModel.Datei.cs`; geprüft durch `PlanungsGpxTests` (Profil bleibt beim Hin- und Rückweg erhalten)
- Wenn der Benutzer die Karte mit gedrückter Maustaste verschiebt, dann wird kein Wegpunkt gesetzt. → umgesetzt in `TourenplanungFenster.xaml.cs` (Bewegung über 4 Pixel ist kein Klick); manuelle Prüfung: Karte ziehen, kein Punkt entsteht
- Wenn die Verbindung über Straßen für ein Teilstück nicht ermittelt werden kann (z. B. ohne Internet), dann wird dieses Teilstück als gerade Linie gezeichnet und ein Hinweis erscheint. → umgesetzt in `ErstelleLuftlinie.cs`, `Routingabruf.cs`, `TourenplanungViewModel.cs`; geprüft durch `ErstelleLuftlinieTests` / manuelle Prüfung: Netzwerk trennen, Punkt setzen, gerade Linie und Hinweis erscheinen
- Wenn sich die Strecke ändert, dann zeigt das Fenster sofort die Länge in km sowie die Höhenmeter bergauf und bergab an. → umgesetzt in `BerechnePlanungswerte.cs`, `TourenplanungViewModel.cs`; geprüft durch `BerechnePlanungswerteTests` / manuelle Prüfung: Werte ändern sich mit jedem Punkt
- Wenn der Benutzer die Linie zwischen zwei Wegpunkten an einer Stelle mit der Maus wegzieht, dann entsteht dort ein neuer Wegpunkt zwischen diesen beiden, und die Strecke wird über ihn neu ermittelt. → umgesetzt in `FindeEinfuegestelle.cs`, `AenderePlanung.Einfuegen`, `TourenplanungFenster.xaml.cs`; geprüft durch `FindeEinfuegestelleTests`, `AenderePlanungTests` / manuelle Prüfung: Linie wegziehen
- Wenn der Benutzer einen Wegpunkt mit der Maus zieht, dann wandert der Punkt mit und die angrenzenden Teilstücke werden neu ermittelt. → umgesetzt in `AenderePlanung.Verschieben`, `TourenplanungFenster.xaml.cs`; geprüft durch `AenderePlanungTests` (nur angrenzende Teilstücke offen) / manuelle Prüfung: Wegpunkt ziehen
- Wenn der Benutzer mit der rechten Maustaste auf einen Wegpunkt klickt, dann wird dieser Punkt entfernt und seine Nachbarn werden mit dem gewählten Fahrprofil neu verbunden. → umgesetzt in `AenderePlanung.Entfernen`, `TourenplanungFenster.xaml.cs`; geprüft durch `AenderePlanungTests` (Nachbarn durch ein offenes Teilstück verbunden) / manuelle Prüfung: Rechtsklick auf Wegpunkt
- Wenn der Benutzer „Letzten Punkt entfernen“ wählt, dann verschwindet der zuletzt gesetzte Wegpunkt samt seinem Teilstück. → umgesetzt in `AenderePlanung.EntferneLetzten`, `TourenplanungViewModel.cs`; geprüft durch `AenderePlanungTests`
- Wenn der Benutzer „Alles löschen“ wählt und die Rückfrage bestätigt, dann ist die Planung leer. → umgesetzt in `TourenplanungViewModel.cs` (Rückfrage, dann leere Planung); manuelle Prüfung: „Alles löschen“ bestätigen und abbrechen
- Wenn die Strecke an einer Abzweigung abbiegt, dann entsteht dort automatisch ein Abbiegepunkt mit der Richtung (z. B. „links“, „rechts“, „leicht rechts“, „Kreisverkehr 2. Ausfahrt“); Straßennamen werden nicht angegeben. → umgesetzt in `LeseBRouterAntwort.cs`, `LeseOpenrouteserviceAntwort.cs`, `BeschreibeAbbiegung.cs`; geprüft durch `LeseBRouterAntwortTests`, `LeseOpenrouteserviceAntwortTests`, `BeschreibeAbbiegungTests`
- Wenn die Strecke an einer Abzweigung geradeaus weitergeht, dann entsteht dort kein Abbiegepunkt. → umgesetzt in `LeseBRouterAntwort.cs`, `LeseOpenrouteserviceAntwort.cs`; geprüft durch `LeseBRouterAntwortTests`, `LeseOpenrouteserviceAntwortTests` (Anweisung „geradeaus“ ergibt keinen Abbiegepunkt)
- Wenn sich die Strecke ändert, dann werden die Abbiegepunkte sofort neu ermittelt und als kleine Markierungen auf der Karte angezeigt. → umgesetzt in `TourenplanungViewModel.cs`, `TourenplanungFenster.xaml.cs` (Ebene Abbiegepunkte); manuelle Prüfung: Wegpunkt verschieben, Markierungen wandern mit
- Wenn ein Teilstück als gerade Linie gezeichnet ist, dann enthält es keine Abbiegepunkte. → umgesetzt in `ErstelleLuftlinie.cs`; geprüft durch `ErstelleLuftlinieTests`
- Wenn der Benutzer die Planung speichert, dann enthält die Datei auch alle Abbiegepunkte mit ihrer Richtung. → umgesetzt in `PlanungsGpx.Schreiben`; geprüft durch `PlanungsGpxTests` (`wpt` mit Richtung)
- Wenn der Benutzer „Speichern…“ wählt, dann kann er Ordner und Dateinamen wählen; vorgeschlagen werden der zuletzt verwendete Ordner und der Name „Planung JJJJ-MM-TT“. → umgesetzt in `TourenplanungViewModel.Datei.cs`, `BildePlanungsdateiname.cs`, `PlanungseinstellungenEinstellung.cs`; geprüft durch `BildePlanungsdateinameTests` / manuelle Prüfung: Speicherdialog zeigt Ordner und Namen
- Wenn der Benutzer die Datei speichert, dann ist sie eine GPX-Datei mit der geplanten Strecke, die andere Programme öffnen können. → umgesetzt in `PlanungsGpx.Schreiben`; geprüft durch `PlanungsGpxTests` (gültiges GPX 1.1) / manuelle Prüfung: Datei in einem anderen GPX-Programm öffnen
- Wenn die Planung weniger als zwei Wegpunkte hat, dann lässt sie sich nicht speichern. → umgesetzt in `TourenplanungViewModel.Datei.cs` (CanExecute); manuelle Prüfung: mit einem Punkt ist „Speichern…“ gesperrt
- Wenn die gewählte Datei schon existiert, dann fragt die App, ob sie überschrieben werden soll. → umgesetzt in `TourenplanungViewModel.Datei.cs` (Überschreibrückfrage des Speicherdialogs); manuelle Prüfung: vorhandene Datei wählen
- Wenn die Datei nicht geschrieben werden kann, dann erscheint eine Fehlermeldung und die Planung bleibt erhalten. → umgesetzt in `TourenplanungViewModel.Datei.cs`; manuelle Prüfung: in einen schreibgeschützten Ordner speichern, Fehlermeldung, Planung bleibt
- Wenn der Benutzer „Öffnen…“ wählt und eine von AndyTours gespeicherte Planung auswählt, dann erscheinen ihre Wegpunkte und ihre Strecke wieder und lassen sich weiterbearbeiten. → umgesetzt in `PlanungsGpx.Lesen`, `TourenplanungViewModel.Datei.cs`; geprüft durch `PlanungsGpxTests` (Wegpunkte, Teilstücke, Luftlinien bleiben erhalten) / manuelle Prüfung: gespeicherte Planung öffnen und weiterbearbeiten
- Wenn die gewählte Datei keine von AndyTours gespeicherte Planung ist oder nicht gelesen werden kann, dann erscheint ein Hinweis und die aktuelle Planung bleibt unverändert. → umgesetzt in `PlanungsGpx.Lesen`, `TourenplanungViewModel.Datei.cs`; geprüft durch `PlanungsGpxTests` (fremdes GPX, kaputte Datei → `null`) / manuelle Prüfung: GPX-Export einer Tour öffnen
- Wenn der Benutzer eine geöffnete Planung erneut speichert, dann werden Ordner und Dateiname der geöffneten Datei vorgeschlagen. → umgesetzt in `TourenplanungViewModel.Datei.cs` (Pfad der geöffneten Datei wird gemerkt); manuelle Prüfung: öffnen, ändern, „Speichern…“
- Wenn der Benutzer das Fenster schließt oder eine andere Planung öffnet und es ungespeicherte Änderungen gibt, dann fragt die App, ob die Änderungen verworfen werden sollen. → umgesetzt in `TourenplanungViewModel.Datei.cs`, `TourenplanungFenster.xaml.cs` (Schließen abfangen); manuelle Prüfung: ändern, dann schließen bzw. öffnen, Rückfrage erscheint

## Entscheidungen
- Beim Ziehen eines Wegpunkts oder der Linie folgen die angrenzenden Teilstücke als gestrichelte Luftlinie der Maus. Ermittelt werden sie erst beim Loslassen, damit der Routingdienst nicht bei jeder Mausbewegung gefragt wird.
- Eine Mausbewegung bis 4 Pixel gilt als Klick. Ein Wegpunkt wird bis 10 Pixel, die Linie bis 6 Pixel Abstand getroffen.
- Teilstücke werden einzeln ermittelt: Eine Änderung fragt nur die betroffenen Teilstücke neu ab, ein Profilwechsel alle. Laufende Abfragen einer überholten Planung werden abgebrochen.
- Das Fahrprofil wird beim Wechsel gemerkt, der Kartenausschnitt beim Schließen des Fensters. Den Ordner merkt sich die App nach erfolgreichem Speichern oder Öffnen.
- Gibt es keine Einheit mit Aufzeichnung, zeigt die Karte beim ersten Öffnen Deutschland.
- Fähren werden gemieden (BRouter `profile:allow_ferries=0`, openrouteservice `avoid_features: ["ferries"]`): Bei der ersten Prüfung führten alle BRouter-Profile mit der Fähre quer über den Chiemsee.
- Kartenart ist OpenStreetMap. Die Umschaltung der Streckenkarte gehört nicht zur Spec.

## Offene Entscheidungen
- Welcher Routingdienst ermittelt die Strecke mit Fahrprofil, Höhen und Abbiegehinweisen? → decided in docs/adr/0004-routingdienst.md

## Risiken
- Der Routingdienst ist ein fremder Onlinedienst. Er kann Nutzungsgrenzen haben oder seine Schnittstelle ändern. Der öffentliche BRouter-Server wird privat betrieben.
- Bei freien Diensten ist die Zahl der Abfragen begrenzt. Bei vielen Wegpunkten und häufigem Profilwechsel kann eine Tagesgrenze erreicht werden.
- Mapsui bietet kein fertiges Ziehen von Zeichen. Maus, Treffertest und das Sperren der Kartenverschiebung während des Ziehens sind eigener Code und nur manuell prüfbar.
