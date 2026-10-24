namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Verschiebt einen Eintrag einer Auswahlliste um eine Stelle nach oben oder unten.
/// </summary>
public static class VerschiebeEintrag
{
    /// <summary>
    /// Liefert die Liste in der neuen Reihenfolge; die übergebene Liste bleibt unverändert.
    /// </summary>
    /// <remarks>
    /// Steht der Eintrag schon ganz oben (bzw. ganz unten) oder ist die Stelle ungültig, kommt die Liste
    /// unverändert zurück.
    /// </remarks>
    /// <param name="liste">Die bisherigen Einträge.</param>
    /// <param name="index">Stelle des verschobenen Eintrags.</param>
    /// <param name="nachOben"><c>true</c> = eine Stelle nach oben, <c>false</c> = eine Stelle nach unten.</param>
    /// <returns>Die neue Reihenfolge.</returns>
    public static List<string> Ausfuehren(IReadOnlyList<string> liste, int index, bool nachOben)
    {
        var neu = liste.ToList();
        int ziel = nachOben ? index - 1 : index + 1;
        if (index < 0 || index >= neu.Count || ziel < 0 || ziel >= neu.Count)
        {
            return neu;
        }

        (neu[index], neu[ziel]) = (neu[ziel], neu[index]);
        return neu;
    }
}
