using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ordnet die Einheiten einer Mehrtagestour zu Etappen.
/// </summary>
public static class OrdneEtappen
{
    /// <summary>
    /// Liefert die Einheiten nach Datum und Startzeit geordnet, unabhängig von der Reihenfolge der Markierung.
    /// </summary>
    /// <remarks>
    /// Eine Einheit ohne Startzeit steht am Anfang ihres Tages. Bei gleichem Datum und gleicher Startzeit entscheidet
    /// die Nummer, also die Reihenfolge, in der die Einheiten angelegt wurden.
    /// </remarks>
    /// <param name="einheiten">Die markierten Einheiten.</param>
    /// <returns>Die Einheiten in der Reihenfolge der Etappen.</returns>
    public static List<Trainingseinheit> Ausfuehren(IEnumerable<Trainingseinheit> einheiten) =>
        einheiten
            .OrderBy(e => e.Datum)
            .ThenBy(e => e.Startzeit ?? TimeOnly.MinValue)
            .ThenBy(e => e.Id)
            .ToList();
}
