using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Zerlegt eine Datenzeile aus <c>workouts.csv</c> in ihre Felder.
/// </summary>
/// <remarks>
/// Aufbau (aus <c>TourEntry.out()</c> des Originals ermittelt, siehe docs/Analyse, Abschnitt 2.1):
/// 27 Felder, Trennzeichen „;", Texte in Hochkommas, Dezimalpunkt „.".
/// Die Felder 15 (Farbschlüssel) und 24 (Änderungszähler für die Synchronisation) werden nicht
/// übernommen, weil AndyTours sie nicht braucht.
/// </remarks>
public static class WandleWorkoutZeile
{
    /// <summary>Anzahl der Felder je Zeile im Original.</summary>
    public const int Feldanzahl = 27;

    /// <summary>
    /// Wandelt die Zeile um.
    /// </summary>
    /// <param name="zeile">Eine Datenzeile (nicht die Kopfzeile mit der Anzahl).</param>
    /// <param name="zeilennummer">Zeilennummer in der Datei, wird im Ergebnis vermerkt.</param>
    /// <returns>Die gelesene Einheit mit den Namen der verknüpften Stammdaten.</returns>
    /// <exception cref="FormatException">Die Zeile hat nicht 27 Felder oder ein Feld ist ungültig.</exception>
    public static AltWorkout Ausfuehren(string zeile, int zeilennummer)
    {
        string[] f = zeile.Split(';');
        if (f.Length != Feldanzahl)
        {
            throw new FormatException($"{f.Length} statt {Feldanzahl} Felder.");
        }

        // Die Feldnummern in den Kommentaren entsprechen der Tabelle in der Analyse (1-basiert).
        var einheit = new Trainingseinheit
        {
            // 1: Datum – Pflichtfeld, ein fehlendes Datum ist ein Fehler
            Datum = ParseAltesDatum.Ausfuehren(f[0]) ?? throw new FormatException("Datum fehlt."),
            Name = EntferneHochkommas.Ausfuehren(f[1]),                 // 2: Tourname
            Fahrzeit = ParseFahrzeit.Ausfuehren(f[2]),                  // 3: Fahrzeit hh:mm:ss
            DistanzKm = ParseZahl.Ausfuehren(f[3]),                     // 4: Distanz km
            Hoehenmeter = ParseGanzzahl.Ausfuehren(f[4]),               // 5: Höhenmeter
            MaxHoehe = ParseGanzzahl.Ausfuehren(f[5]),                  // 6: höchster Punkt
            MaxGeschwindigkeit = ParseZahl.Ausfuehren(f[6]),            // 7: Höchstgeschwindigkeit
            HfDurchschnitt = ParseGanzzahl.Ausfuehren(f[7]),            // 8: Ø Herzfrequenz
            Trainingsbereich = EntferneHochkommas.Ausfuehren(f[9]),     // 10: Trainingsbereich
            StartHoehe = ParseGanzzahl.Ausfuehren(f[10]),               // 11: Starthöhe
            Trittfrequenz = ParseGanzzahl.Ausfuehren(f[11]),            // 12: Ø Trittfrequenz
            Temperatur = ParseGanzzahl.Ausfuehren(f[12]),               // 13: Ø Temperatur
            HfMax = ParseGanzzahl.Ausfuehren(f[13]),                    // 14: max. Herzfrequenz
            Startzeit = ParseStartzeit(f[15]),                          // 16: Startzeit hh:mm
            KoerpergewichtKg = ParseZahl.Ausfuehren(f[18]),             // 19: Körpergewicht
            HfMin = ParseGanzzahl.Ausfuehren(f[19]),                    // 20: min. Herzfrequenz
            LeistungWatt = ParseZahl.Ausfuehren(f[20]),                 // 21: Ø Leistung
            Erholungspuls = ParseGanzzahl.Ausfuehren(f[21]),            // 22: Erholungspuls
            Kommentar = EntferneHochkommas.Ausfuehren(f[22]),           // 23: Kommentar
        };

        var workout = new AltWorkout
        {
            Zeilennummer = zeilennummer,
            Einheit = einheit,
            EquipmentName = EntferneHochkommas.Ausfuehren(f[8]),        // 9: Equipment
            AktivitaetName = EntferneHochkommas.Ausfuehren(f[16]),      // 17: Aktivität
            // 18: Kategorien, durch Komma getrennt; „ " (Leerzeichen) bedeutet keine Kategorie
            KategorieNamen = EntferneHochkommas.Ausfuehren(f[17])
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
        };

        // 25–27: Anhänge; nur angegebene (nicht leere) Pfade übernehmen
        AnhangHinzufuegen(workout, AnhangTyp.Hoehenprofil, f[24]);
        AnhangHinzufuegen(workout, AnhangTyp.GpsTrack, f[25]);
        AnhangHinzufuegen(workout, AnhangTyp.Dokumentation, f[26]);

        return workout;
    }

    /// <summary>Liest die Startzeit „hh:mm"; ein leeres Feld ergibt <c>null</c>.</summary>
    private static TimeOnly? ParseStartzeit(string text)
    {
        string bereinigt = text.Trim();
        if (bereinigt.Length == 0)
        {
            return null;
        }

        // ParseFahrzeit liest „hh:mm" bereits zuverlässig; hier nur in eine Uhrzeit umwandeln.
        TimeSpan dauer = ParseFahrzeit.Ausfuehren(bereinigt);
        if (dauer >= TimeSpan.FromDays(1))
        {
            throw new FormatException($"„{text}“ ist keine gültige Uhrzeit.");
        }

        return TimeOnly.FromTimeSpan(dauer);
    }

    /// <summary>Hängt einen Anhang an, sofern im Feld ein Pfad steht.</summary>
    private static void AnhangHinzufuegen(AltWorkout workout, AnhangTyp typ, string feld)
    {
        string pfad = EntferneHochkommas.Ausfuehren(feld);
        if (pfad.Length > 0)
        {
            workout.Anhaenge.Add((typ, pfad));
        }
    }
}
