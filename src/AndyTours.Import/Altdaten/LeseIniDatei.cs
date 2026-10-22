namespace AndyTours.Import.Altdaten;

/// <summary>
/// Zerlegt die Zeilen einer INI-Datei in Abschnitte.
/// </summary>
public static class LeseIniDatei
{
    /// <summary>
    /// Ordnet jede Zeile dem Abschnitt zu, unter dem sie steht.
    /// </summary>
    /// <remarks>
    /// Kommt ein Abschnitt mehrfach vor (in <c>bikexperience.ini</c> z. B. <c>[FieldLengths]</c>),
    /// werden seine Zeilen aneinandergehängt. Leerzeilen und Kommentarzeilen (beginnend mit „#")
    /// werden übersprungen, ebenso Zeilen vor dem ersten Abschnitt.
    /// </remarks>
    /// <param name="zeilen">Die Zeilen der Datei, z. B. aus <see cref="LeseTextdateiTolerant"/>.</param>
    /// <returns>Die gegliederte INI-Datei.</returns>
    public static IniDatei Ausfuehren(IEnumerable<string> zeilen)
    {
        var ini = new IniDatei();
        List<string>? aktuellerAbschnitt = null;

        foreach (string rohzeile in zeilen)
        {
            string zeile = rohzeile.Trim();
            if (zeile.Length == 0 || zeile.StartsWith('#'))
            {
                continue;
            }

            // Abschnittsüberschrift „[Name]": neuen Abschnitt beginnen oder vorhandenen fortsetzen
            if (zeile.StartsWith('[') && zeile.EndsWith(']'))
            {
                string name = zeile[1..^1].Trim();
                if (!ini.Abschnitte.TryGetValue(name, out aktuellerAbschnitt))
                {
                    aktuellerAbschnitt = new List<string>();
                    ini.Abschnitte[name] = aktuellerAbschnitt;
                }

                continue;
            }

            aktuellerAbschnitt?.Add(zeile);
        }

        return ini;
    }
}
