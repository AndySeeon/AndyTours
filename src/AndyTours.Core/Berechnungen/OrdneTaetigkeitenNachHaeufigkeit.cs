using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ordnet die Tätigkeiten für die Auswahl im Wartungs- und im Alarmfenster nach ihrer bisherigen Häufigkeit.
/// </summary>
/// <remarks>
/// Die häufigsten Tätigkeiten stehen oben und sind so am schnellsten gewählt. Die Liste „Tätigkeit“ in den
/// Einstellungen behält ihre eigene Reihenfolge; sie entscheidet nur bei gleicher Häufigkeit.
/// </remarks>
public static class OrdneTaetigkeitenNachHaeufigkeit
{
    /// <summary>
    /// Liefert die Tätigkeiten der Liste, absteigend nach der Zahl der Wartungen über alle Räder, die sie
    /// verwenden; bei gleicher Zahl (auch bei nie verwendeten) in der Reihenfolge der Liste.
    /// </summary>
    /// <remarks>
    /// Gezählt werden nur durchgeführte Wartungen, keine Alarme, ohne Unterschied zwischen Groß- und
    /// Kleinschreibung und ohne Leerzeichen am Rand – so wie <see cref="ZaehleTaetigkeit"/> zählt.
    /// Die Sortierung ist stabil, deshalb bleibt bei Gleichstand die Reihenfolge der Liste erhalten.
    /// </remarks>
    /// <param name="taetigkeiten">Die Liste „Tätigkeit“ in der Reihenfolge der Einstellungen.</param>
    /// <param name="wartungen">Alle Wartungseinträge (Wartungen und Alarme).</param>
    /// <returns>Die Tätigkeiten in der neuen Reihenfolge, z. B. „Kettenwechsel“ vor „Reifen VR“.</returns>
    public static List<string> Ausfuehren(IEnumerable<string> taetigkeiten, IEnumerable<Wartung> wartungen)
    {
        // Einmal zählen statt für jede Tätigkeit alle Wartungen zu durchlaufen
        Dictionary<string, int> anzahl = wartungen
            .Where(w => w.Typ == WartungsTyp.Wartung)
            .GroupBy(w => w.Taetigkeit.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        return taetigkeiten
            .OrderByDescending(t => anzahl.GetValueOrDefault(t.Trim()))
            .ToList();
    }
}
