using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bildet die Anfangsliste der Trainingsbereiche aus den Kürzeln, die in den Einheiten vorkommen.
/// </summary>
/// <remarks>
/// Wird nur verwendet, solange die Liste noch nie angelegt wurde (erster Start mit der Funktion
/// „Auswahllisten verwalten“, ältere Sicherung, anderer Datenordner). Danach pflegt der Benutzer die
/// Liste selbst.
/// </remarks>
public static class ErmittleTrainingsbereiche
{
    /// <summary>
    /// Liefert die vorkommenden Kürzel: ohne Leerzeichen am Rand, ohne leere Werte, ohne Dubletten
    /// (ohne Unterschied zwischen Groß- und Kleinschreibung; die zuerst gefundene Schreibweise gilt),
    /// alphabetisch geordnet.
    /// </summary>
    /// <param name="einheiten">Alle Trainingseinheiten.</param>
    /// <returns>Die Anfangsliste, z. B. „GA1“, „GA2“, „K0“.</returns>
    public static List<string> Ausfuehren(IEnumerable<Trainingseinheit> einheiten) => einheiten
        .Select(e => e.Trainingsbereich.Trim())
        .Where(b => b.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.CurrentCultureIgnoreCase)
        .ToList();
}
