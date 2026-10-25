using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Zählt die Wartungen, die eine Tätigkeit verwenden.
/// </summary>
public static class ZaehleTaetigkeit
{
    /// <summary>
    /// Liefert die Anzahl der Wartungen mit dieser Tätigkeit – ohne Unterschied zwischen Groß- und
    /// Kleinschreibung und ohne Leerzeichen am Rand, so wie Einträge der Liste verglichen werden.
    /// </summary>
    /// <param name="wartungen">Alle Wartungen.</param>
    /// <param name="taetigkeit">Die Tätigkeit, z. B. „Kettenwechsel“.</param>
    /// <returns>Die Anzahl (0, wenn die Tätigkeit nicht verwendet wird).</returns>
    public static int Ausfuehren(IEnumerable<Wartung> wartungen, string taetigkeit)
    {
        string gesucht = taetigkeit.Trim();
        return wartungen.Count(w => string.Equals(w.Taetigkeit.Trim(), gesucht, StringComparison.OrdinalIgnoreCase));
    }
}
