using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Fasst Trainingseinheiten nach Kalenderjahr zusammen.
/// </summary>
public static class BerechneJahressummen
{
    /// <summary>
    /// Bildet für jedes Jahr, in dem Einheiten vorliegen, die Summen von Anzahl, Distanz,
    /// Höhenmetern und Fahrzeit.
    /// </summary>
    /// <param name="einheiten">Die auszuwertenden Trainingseinheiten (z. B. bereits nach Aktivität gefiltert).</param>
    /// <returns>Eine Summe je Jahr, aufsteigend nach Jahr sortiert. Jahre ohne Einheiten fehlen.</returns>
    public static List<Jahressumme> Ausfuehren(IEnumerable<Trainingseinheit> einheiten)
    {
        return einheiten
            // Gruppieren nach dem Jahr des Datums
            .GroupBy(einheit => einheit.Datum.Year)
            .OrderBy(gruppe => gruppe.Key)
            .Select(gruppe => new Jahressumme(
                Jahr: gruppe.Key,
                Anzahl: gruppe.Count(),
                // Distanz auf 2 Nachkommastellen runden: Die Einzelwerte haben 2 Stellen,
                // beim Addieren von double-Werten entstehen sonst Rundungsreste wie 1234.5600000001.
                DistanzKm: Math.Round(gruppe.Sum(einheit => einheit.DistanzKm), 2),
                Hoehenmeter: gruppe.Sum(einheit => einheit.Hoehenmeter),
                // TimeSpan hat keine Sum-Überladung, deshalb über die Ticks addieren
                Fahrzeit: TimeSpan.FromTicks(gruppe.Sum(einheit => einheit.Fahrzeit.Ticks))))
            .ToList();
    }
}
