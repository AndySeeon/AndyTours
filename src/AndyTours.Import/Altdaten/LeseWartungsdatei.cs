namespace AndyTours.Import.Altdaten;

/// <summary>
/// Liest die Wartungseinträge und Alarme aus <c>maintanence.dat</c> (Schreibweise wie im Original).
/// </summary>
public static class LeseWartungsdatei
{
    /// <summary>
    /// Liest alle Einträge der Datei.
    /// </summary>
    /// <remarks>
    /// Die Datei hat keine Kopfzeile. Fehlerhafte Zeilen werden übersprungen und im Bericht gemeldet.
    /// </remarks>
    /// <param name="pfad">Vollständiger Pfad zur <c>maintanence.dat</c>.</param>
    /// <param name="bericht">Bericht, in den Auffälligkeiten eingetragen werden.</param>
    /// <returns>Die gelesenen Einträge in der Reihenfolge der Datei.</returns>
    public static List<AltWartung> Ausfuehren(string pfad, Migrationsbericht bericht)
    {
        List<string> zeilen = LeseTextdateiTolerant.Ausfuehren(pfad, out int latin1);
        bericht.AnzahlLatin1Zeichen += latin1;

        var wartungen = new List<AltWartung>();
        for (int index = 0; index < zeilen.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(zeilen[index]))
            {
                continue;
            }

            try
            {
                wartungen.Add(WandleWartungszeile.Ausfuehren(zeilen[index], index + 1));
            }
            catch (FormatException fehler)
            {
                bericht.Warnungen.Add($"maintanence.dat Zeile {index + 1} übersprungen: {fehler.Message}");
            }
        }

        return wartungen;
    }
}
