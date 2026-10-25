using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Verbindet die Verläufe der Etappen zu einem durchgehenden Verlauf für das Höhenprofil.
/// </summary>
public static class VerbindeVerlaeufe
{
    /// <summary>
    /// Hängt die Messpunkte der Etappen hintereinander.
    /// </summary>
    /// <remarks>
    /// Distanz und Fahrzeit jeder Etappe werden um die letzte Distanz und Fahrzeit der vorigen Etappe verschoben. So
    /// zählen beide über die Etappen weiter, und Nächte und Pausen zwischen den Etappen entfallen. Grundlage sind die
    /// Aufzeichnungen, nicht die Werte der Einheiten. Etappen ohne Verlauf oder mit leerem Verlauf entfallen. Eine
    /// einzelne Etappe bleibt unverändert.
    /// </remarks>
    /// <param name="etappen">Die Etappen in ihrer Reihenfolge.</param>
    /// <returns>Der durchgehende Verlauf.</returns>
    public static Mehrtagesverlauf Ausfuehren(IReadOnlyList<Etappe> etappen)
    {
        List<Verlaufspunkt> punkte = [];
        List<Etappe> mitVerlauf = [];
        List<int> beginn = [];
        double distanz = 0;
        TimeSpan fahrzeit = TimeSpan.Zero;

        foreach (Etappe etappe in etappen)
        {
            if (etappe.Verlauf is not { Count: > 0 } verlauf)
            {
                continue;
            }

            mitVerlauf.Add(etappe);
            beginn.Add(punkte.Count);
            double verschiebungKm = distanz;
            TimeSpan verschiebungZeit = fahrzeit;
            punkte.AddRange(verlauf.Select(p => p with
            {
                DistanzKm = p.DistanzKm + verschiebungKm,
                Fahrzeit = p.Fahrzeit + verschiebungZeit,
            }));
            distanz = punkte[^1].DistanzKm;
            fahrzeit = punkte[^1].Fahrzeit;
        }

        return new Mehrtagesverlauf(punkte, mitVerlauf, beginn);
    }
}
