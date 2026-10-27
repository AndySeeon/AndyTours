# Tasks: 004 – Automatische Datensicherung

- [x] 1. Sicherungseinstellungen-Modell mit Standardwerten anlegen (`Sicherungseinstellungen`)
- [x] 2. Eingabewert 1–99 prüfen, mit Tests (`PruefeSicherungswert`, `PruefeSicherungswertTests`)
- [x] 3. Fälligkeit der Sicherung ermitteln, mit Tests (`IstSicherungFaellig`, `IstSicherungFaelligTests`)
- [x] 4. Tabelle `Einstellung` im Schema ergänzen, Schemaversion 2 (`ErstelleSchema`)
- [x] 5. Sicherungseinstellungen laden und speichern, mit Tests (`LadeSicherungseinstellungen`, `SpeichereSicherungseinstellungen`, `SicherungseinstellungenTests`)
- [x] 6. Konvention zur Ablage der Einstellungen ergänzen (`docs/Konventionen.md`)
- [x] 7. Dateinamenmuster und Sicherungsordner ermitteln (`Sicherungsdatei`, `ErmittleSicherungsordner`)
- [x] 8. Sicherungskopie erstellen, mit Tests (`ErstelleSicherung`, `ErstelleSicherungTests`)
- [x] 9. Alte Sicherungen entfernen, mit Tests (`EntferneAlteSicherungen`, `EntferneAlteSicherungenTests`)
- [x] 10. Ablauf der Startsicherung, mit Tests (`Startsicherungsergebnis`, `FuehreStartsicherungDurch`, `FuehreStartsicherungDurchTests`)
- [x] 11. Startsicherung beim Programmstart aufrufen, Warnung bei Fehler (`App.xaml.cs`)
- [x] 12. Einstellungen um die Gruppe „Datensicherung“ erweitern (`EinstellungenViewModel`, `EinstellungenFenster.xaml`)
- [x] 13. Build und alle Tests ausführen
- [x] 14. Manuelle Prüfung in der App: Sicherung beim 3. Start, Ordner wählen und Neustart, Intervall 1 / Höchstzahl 2, ungültiger Wert „0“, nicht erreichbarer Sicherungsordner, Datenordnerwechsel (Release-Build)
