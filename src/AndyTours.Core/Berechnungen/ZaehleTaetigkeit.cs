using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Zählt die Wartungen bzw. Alarme, die eine Tätigkeit verwenden.
/// </summary>
public static class ZaehleTaetigkeit
{
    /// <summary>
    /// Liefert die Anzahl der Einträge eines Typs mit dieser Tätigkeit – ohne Unterschied zwischen Groß- und
    /// Kleinschreibung und ohne Leerzeichen am Rand, so wie Einträge der Liste verglichen werden.
    /// </summary>
    /// <remarks>
    /// Wartungen und Alarme stehen im selben Bestand; Rückfrage und Löschsperre der Liste „Tätigkeit“ nennen
    /// beide getrennt und rufen die Funktion deshalb je Typ auf.
    /// </remarks>
    /// <param name="wartungen">Alle Wartungseinträge (Wartungen und Alarme).</param>
    /// <param name="taetigkeit">Die Tätigkeit, z. B. „Kettenwechsel“.</param>
    /// <param name="typ">Welche Einträge gezählt werden: Wartungen oder Alarme.</param>
    /// <returns>Die Anzahl (0, wenn die Tätigkeit nicht verwendet wird).</returns>
    public static int Ausfuehren(IEnumerable<Wartung> wartungen, string taetigkeit, WartungsTyp typ)
    {
        string gesucht = taetigkeit.Trim();
        return wartungen.Count(w => w.Typ == typ && string.Equals(w.Taetigkeit.Trim(), gesucht, StringComparison.OrdinalIgnoreCase));
    }
}
