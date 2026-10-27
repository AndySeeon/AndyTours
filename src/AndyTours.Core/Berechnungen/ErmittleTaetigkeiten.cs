using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bildet die Anfangsliste der Tätigkeiten aus den Tätigkeiten, die in den Wartungen vorkommen.
/// </summary>
/// <remarks>
/// Wird nur verwendet, solange die Liste „Tätigkeit“ noch nie angelegt wurde (erster Start mit der Funktion
/// „Wartung erfassen“, ältere Sicherung, anderer Datenordner). Danach pflegt der Benutzer die Liste selbst.
/// </remarks>
public static class ErmittleTaetigkeiten
{
    /// <summary>
    /// Liefert die vorkommenden Tätigkeiten: ohne Leerzeichen am Rand, ohne leere Werte, ohne Dubletten
    /// (ohne Unterschied zwischen Groß- und Kleinschreibung; die zuerst gefundene Schreibweise gilt),
    /// alphabetisch geordnet.
    /// </summary>
    /// <param name="wartungen">Alle Wartungen.</param>
    /// <returns>Die Anfangsliste, z. B. „Bremsbelag HR“, „Kettenwechsel“.</returns>
    public static List<string> Ausfuehren(IEnumerable<Wartung> wartungen) => wartungen
        .Select(w => w.Taetigkeit.Trim())
        .Where(t => t.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.CurrentCultureIgnoreCase)
        .ToList();
}
