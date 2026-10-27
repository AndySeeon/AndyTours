using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt aus dem Verlauf einer Tour die Strecke für die Streckenkarte.
/// </summary>
public static class ErmittleStrecke
{
    /// <summary>
    /// Sammelt die Positionen des Verlaufs und bestimmt das umschließende Rechteck.
    /// </summary>
    /// <remarks>
    /// Messpunkte ohne Position (z. B. im Tunnel) werden ausgelassen; die Linie verbindet so die Punkte davor und
    /// danach. Die Reihenfolge bleibt erhalten, damit der erste Punkt der Start und der letzte das Ziel ist.
    /// </remarks>
    /// <param name="verlauf">Die Messpunkte der Tour.</param>
    /// <returns>Die Strecke oder <c>null</c>, wenn kein Messpunkt eine Position hat (z. B. Fahrt auf der Rolle).</returns>
    public static Strecke? Ausfuehren(IReadOnlyList<Verlaufspunkt> verlauf)
    {
        List<Geoposition> punkte = verlauf
            .Where(p => p.Position is not null)
            .Select(p => p.Position!)
            .ToList();
        if (punkte.Count == 0)
        {
            return null;
        }

        var suedWest = new Geoposition(punkte.Min(p => p.Breite), punkte.Min(p => p.Laenge));
        var nordOst = new Geoposition(punkte.Max(p => p.Breite), punkte.Max(p => p.Laenge));
        return new Strecke(punkte, suedWest, nordOst);
    }
}
