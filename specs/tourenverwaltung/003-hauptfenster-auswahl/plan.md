# Plan: 003 – Jahres- und Aktivitätsauswahl im Hauptfenster

## Vorgehen
- Die Grundform gibt es schon (Werkzeugleiste mit zwei Auswahllisten, Summen in der Statusleiste). Die Rechenlogik wandert aus `HauptfensterViewModel` in drei Fachfunktionen in `AndyTours.Core/Berechnungen`, damit sie testbar ist; das ViewModel ruft sie nur noch auf.
- `ErmittleAuswahljahre(einheiten, aktuellesJahr)`: alle Jahre mit Einheiten plus das aktuelle Jahr, absteigend, ohne Doppelte. Das aktuelle Jahr wird übergeben (nicht intern `DateTime.Today`), damit Tests unabhängig vom Datum sind.
- `FiltereAuswahl(einheiten, jahr?, aktivitaetId?)`: `null` steht für „alle Jahre“ bzw. „alle Aktivitäten“.
- `BerechneAuswahlsummen(einheiten)` → neuer Record `Auswahlsumme(Anzahl, DistanzKm, Hoehenmeter)`; leere Auswahl ergibt 0/0/0. Distanz wie bei `BerechneJahressummen` auf 2 Stellen gerundet, damit die Werte zum Umwandlungsbericht passen.
- ViewModel: Jahresliste über `ErmittleAuswahljahre` mit `DateTime.Today.Year`; Vorauswahl aktuelles Jahr und Hauptaktivität (keine Hauptaktivität → „alle Aktivitäten“). Neue Eigenschaft `Summentext` („76 Einheiten · 6.667,64 km · 63.128 hm“, de-DE); ohne Einheiten im Datenordner stattdessen „Noch keine Touren vorhanden.“.
- Hauptfenster: `Summentext` in der Werkzeugleiste rechts neben der Aktivitätsauswahl, fett. Die Statusleiste zeigt links nur noch die Gesamtzahl der Einheiten in der Datenbank.
- Der Wechsel des Datenordners läuft bereits über `LadeDatenbank`, das Listen, Vorauswahl und Summen neu aufbaut – dort ändert sich nur der Aufruf der neuen Funktionen.

## Dateien
- `src/AndyTours.Core/Modell/Auswahlsumme.cs` (neu) — Record mit Anzahl, Kilometern und Höhenmetern einer Auswahl
- `src/AndyTours.Core/Berechnungen/ErmittleAuswahljahre.cs` (neu) — Jahre für die Jahresliste inkl. aktuellem Jahr, neuestes zuerst
- `src/AndyTours.Core/Berechnungen/FiltereAuswahl.cs` (neu) — Einheiten nach Jahr und Aktivität filtern
- `src/AndyTours.Core/Berechnungen/BerechneAuswahlsummen.cs` (neu) — Summen einer Auswahl
- `src/AndyTours.App/ViewModels/HauptfensterViewModel.cs` (ändern) — neue Funktionen verwenden, Vorauswahl aktuelles Jahr, `Summentext`, Statustext nur noch Gesamtzahl
- `src/AndyTours.App/Views/Hauptfenster.xaml` (ändern) — Summen in der Werkzeugleiste neben der Auswahl, Kopfkommentar anpassen
- `tests/AndyTours.Tests/Berechnungen/ErmittleAuswahljahreTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/FiltereAuswahlTests.cs` (neu)
- `tests/AndyTours.Tests/Berechnungen/BerechneAuswahlsummenTests.cs` (neu) — u. a. Abgleich je Jahr mit `BerechneJahressummen`

## Abdeckung der Kriterien
- Wenn das Hauptfenster geöffnet wird, dann kann der Benutzer ein Jahr oder „alle Jahre“ und eine Aktivität oder „alle Aktivitäten“ auswählen. → `HauptfensterViewModel` (Listen mit „alle Jahre“/„alle Aktivitäten“ als erstem Eintrag), `Hauptfenster.xaml`; manuell: App starten, beide Listen aufklappen.
- Wenn der Benutzer die Jahresliste öffnet, dann enthält sie alle Jahre, in denen es Einheiten gibt, und immer auch das aktuelle Jahr, das neueste zuerst. → `ErmittleAuswahljahre`; geprüft durch `ErmittleAuswahljahreTests` (absteigend, ohne Doppelte, aktuelles Jahr ergänzt bzw. nicht doppelt, leere Einheiten → nur aktuelles Jahr).
- Wenn der Benutzer die Aktivitätsliste öffnet, dann enthält sie alle Aktivitäten. → `HauptfensterViewModel.LadeDatenbank`; manuell: Liste zeigt „alle Aktivitäten“, „Bike“ und die zweite Aktivität aus `AppDaten`.
- Wenn das Programm startet, dann sind das aktuelle Jahr und die Hauptaktivität vorausgewählt. → `HauptfensterViewModel.LadeDatenbank`; manuell: App starten, Auswahl zeigt 2026 und „Bike“.
- Wenn das aktuelle Jahr noch keine Einheiten hat, dann steht es trotzdem in der Liste und ist vorausgewählt. → `ErmittleAuswahljahre`, `HauptfensterViewModel`; geprüft durch `ErmittleAuswahljahreTests` (aktuelles Jahr ohne Einheiten ist erster Eintrag) und manuell: Datenordner mit leerer Datenbank wählen, Auswahl zeigt das aktuelle Jahr.
- Wenn eine Auswahl getroffen ist, dann stehen oben neben der Auswahl die Anzahl der Einheiten, die Kilometer und die Höhenmeter dieser Auswahl. → `FiltereAuswahl`, `BerechneAuswahlsummen`, `Summentext`, `Hauptfenster.xaml`; geprüft durch `FiltereAuswahlTests`, `BerechneAuswahlsummenTests` und manuell: Summen stehen in der Werkzeugleiste neben der Aktivitätsauswahl.
- Wenn der Benutzer Jahr oder Aktivität ändert, dann zeigen die Summen sofort die Werte der neuen Auswahl. → `HauptfensterViewModel` (`OnAusgewaehltesJahrChanged`, `OnAusgewaehlteAktivitaetChanged`); manuell: Jahr und Aktivität umschalten, Summen ändern sich ohne weitere Aktion.
- Wenn die Auswahl keine Einheiten enthält, dann zeigen die Summen 0. → `BerechneAuswahlsummen`; geprüft durch `BerechneAuswahlsummenTests` (leere Auswahl → 0/0/0) und manuell: ein Jahr mit einer Aktivität ohne Touren wählen.
- Wenn der Datenordner keine Einheiten enthält, dann erscheint ein Hinweis, dass noch keine Touren vorhanden sind. → `HauptfensterViewModel` (`Summentext`); manuell: leeren Datenordner wählen, Hinweis „Noch keine Touren vorhanden.“ erscheint neben der Auswahl.
- Wenn der Benutzer den Datenordner wechselt, dann werden Auswahllisten, Vorauswahl und Summen mit den Daten des neuen Ordners neu aufgebaut. → `HauptfensterViewModel.LadeDatenbank`; manuell: von `AppDaten` zu einem leeren Ordner und zurück wechseln, Listen und Summen passen jeweils.
- Wenn ein Jahr und „alle Aktivitäten“ gewählt sind, dann stimmen Anzahl, Kilometer und Höhenmeter mit den Jahressummen im Umwandlungsbericht überein. → `FiltereAuswahl`, `BerechneAuswahlsummen`; geprüft durch `BerechneAuswahlsummenTests` (je Jahr gleiche Werte wie `BerechneJahressummen`, das auch den Bericht erzeugt) und manuell: 2026 / „alle Aktivitäten“ gegen `AppDaten\migrationsbericht.txt`.

## Entscheidungen
- Der Hinweis „Noch keine Touren vorhanden.“ steht an der Stelle der Summen, damit er nicht übersehen wird.
- Gibt es keine Hauptaktivität, ist „alle Aktivitäten“ vorausgewählt.
- Die Oberfläche selbst wird nicht automatisch getestet (das Testprojekt verweist nicht auf die WPF-App); deshalb steckt die Logik in testbaren Fachfunktionen, und die Anzeige wird manuell geprüft.
