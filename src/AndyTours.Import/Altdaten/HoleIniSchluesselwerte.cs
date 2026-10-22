namespace AndyTours.Import.Altdaten;

/// <summary>
/// Liefert die Schlüssel-Wert-Paare eines INI-Abschnitts.
/// </summary>
public static class HoleIniSchluesselwerte
{
    /// <summary>
    /// Zerlegt die Zeilen eines Abschnitts am ersten „=" in Schlüssel und Wert.
    /// </summary>
    /// <remarks>
    /// Nur am ersten „=" trennen, weil Werte selbst ein „=" enthalten können (z. B. URLs mit
    /// <c>?type=Tracks</c>). Zeilen ohne „=" werden übergangen. Kommt ein Schlüssel mehrfach vor,
    /// gilt der letzte Wert – so verhält sich auch das Original.
    /// </remarks>
    /// <param name="ini">Die gelesene INI-Datei.</param>
    /// <param name="abschnitt">Name des Abschnitts ohne eckige Klammern.</param>
    /// <returns>Schlüssel und Werte; leer, wenn der Abschnitt fehlt. Groß-/Kleinschreibung der Schlüssel wird ignoriert.</returns>
    public static Dictionary<string, string> Ausfuehren(IniDatei ini, string abschnitt)
    {
        var werte = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!ini.Abschnitte.TryGetValue(abschnitt, out List<string>? zeilen))
        {
            return werte;
        }

        foreach (string zeile in zeilen)
        {
            int trenner = zeile.IndexOf('=');
            if (trenner <= 0)
            {
                continue;
            }

            werte[zeile[..trenner].Trim()] = zeile[(trenner + 1)..].Trim();
        }

        return werte;
    }
}
