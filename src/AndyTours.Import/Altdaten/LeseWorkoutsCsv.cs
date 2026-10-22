namespace AndyTours.Import.Altdaten;

/// <summary>
/// Liest die Hauptdatenbank <c>workouts.csv</c> des Originals.
/// </summary>
public static class LeseWorkoutsCsv
{
    /// <summary>
    /// Liest alle Trainingseinheiten der Datei.
    /// </summary>
    /// <remarks>
    /// Die erste Zeile enthält die Anzahl der Einheiten; sie wird mit der tatsächlichen Zeilenzahl
    /// verglichen. Fehlerhafte Zeilen brechen die Übernahme nicht ab, sondern werden übersprungen
    /// und im Bericht als Warnung mit Zeilennummer aufgeführt.
    /// </remarks>
    /// <param name="pfad">Vollständiger Pfad zur <c>workouts.csv</c>.</param>
    /// <param name="bericht">Bericht, in den Auffälligkeiten eingetragen werden.</param>
    /// <returns>Die gelesenen Einheiten in der Reihenfolge der Datei.</returns>
    public static List<AltWorkout> Ausfuehren(string pfad, Migrationsbericht bericht)
    {
        List<string> zeilen = LeseTextdateiTolerant.Ausfuehren(pfad, out int latin1);
        bericht.AnzahlLatin1Zeichen += latin1;

        var workouts = new List<AltWorkout>();
        if (zeilen.Count == 0)
        {
            bericht.Warnungen.Add("workouts.csv ist leer.");
            return workouts;
        }

        // Zeilen ab Index 1 sind Daten; Zeilennummer = Index + 1 (wie im Texteditor angezeigt).
        for (int index = 1; index < zeilen.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(zeilen[index]))
            {
                continue;
            }

            try
            {
                workouts.Add(WandleWorkoutZeile.Ausfuehren(zeilen[index], index + 1));
            }
            catch (FormatException fehler)
            {
                bericht.Warnungen.Add($"workouts.csv Zeile {index + 1} übersprungen: {fehler.Message}");
            }
        }

        // Kopfzeile mit der erwarteten Anzahl prüfen
        if (int.TryParse(zeilen[0].Trim(), out int erwartet) && erwartet != workouts.Count)
        {
            bericht.Warnungen.Add($"workouts.csv: Kopfzeile nennt {erwartet} Einheiten, gelesen wurden {workouts.Count}.");
        }

        return workouts;
    }
}
