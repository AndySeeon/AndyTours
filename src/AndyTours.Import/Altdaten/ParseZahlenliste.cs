namespace AndyTours.Import.Altdaten;

/// <summary>
/// Wandelt eine kommagetrennte Liste ganzer Zahlen um, z. B. Zähnezahlen „39,53".
/// </summary>
public static class ParseZahlenliste
{
    /// <summary>
    /// Liest alle Zahlen der Liste.
    /// </summary>
    /// <param name="text">Kommagetrennte Liste, z. B. „11,12,13,14".</param>
    /// <returns>Die Zahlen in der angegebenen Reihenfolge; leere Einträge werden übergangen.</returns>
    /// <exception cref="FormatException">Ein Eintrag ist keine Zahl.</exception>
    public static List<int> Ausfuehren(string text)
    {
        return text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseGanzzahl.Ausfuehren)
            .ToList();
    }
}
