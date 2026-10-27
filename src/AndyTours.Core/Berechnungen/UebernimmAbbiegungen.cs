using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Übernimmt die Abbiegungen, die der Routingdienst für eine gefahrene Strecke geliefert hat, auf deren Spur
/// (Spec 032).
/// </summary>
/// <remarks>
/// Der Dienst rechnet die Strecke über die Stützpunkte nach (<see cref="WaehleStuetzpunkte"/>). Wo er dabei denselben
/// Weg nimmt wie die gefahrene Spur, stimmen auch seine Abbiegungen; wo er einen anderen Weg wählt (z. B. weil die Spur
/// abseits von Wegen verläuft), gehören sie nicht dazu.
/// <list type="bullet">
/// <item>Ein Abbiegepunkt zählt nur, wenn er selbst und die berechnete Strecke im Umkreis von
/// <see cref="UmkreisMeter"/> vor und nach ihm höchstens <see cref="AbweichungMeter"/> von der Spur entfernt
/// liegen.</item>
/// <item>Er kommt an den nächsten Punkt der Spur. Gesucht wird in Fahrtrichtung ab der vorigen Abbiegung, damit eine
/// Spur, die eine Stelle zweimal passiert, die Abbiegung am richtigen Durchgang erhält.</item>
/// <item>Wendehinweise entfallen: Sie entstehen fast nur, wenn der Dienst an einem Stützpunkt kurz über die Spur
/// hinausfährt und umkehrt.</item>
/// <item>Fällt eine Abbiegung auf denselben Spurpunkt wie die vorige, entfällt sie.</item>
/// </list>
/// </remarks>
public static class UebernimmAbbiegungen
{
    /// <summary>Größte erlaubte Abweichung der berechneten Strecke von der Spur in Metern.</summary>
    public const double AbweichungMeter = 30;

    /// <summary>Umkreis vor und nach einem Abbiegepunkt, in dem die berechnete Strecke geprüft wird, in Metern.</summary>
    public const double UmkreisMeter = 150;

    /// <summary>
    /// Ordnet die Abbiegungen der berechneten Teilstücke der Spur zu.
    /// </summary>
    /// <param name="spur">Die Positionen der gefahrenen Strecke (<see cref="BildeCourse.Spur"/>).</param>
    /// <param name="teilstuecke">Die vom Dienst berechneten Teilstücke in Fahrtrichtung, je Paket eines.</param>
    /// <returns>Die übernommenen Abbiegungen in Fahrtrichtung; die Punktnummern beziehen sich auf die Spur.</returns>
    public static IReadOnlyList<Courseabbiegung> Ausfuehren(IReadOnlyList<Geoposition> spur, IReadOnlyList<Teilstueck> teilstuecke)
    {
        List<Courseabbiegung> abbiegungen = [];
        if (spur.Count < 2)
        {
            return abbiegungen;
        }

        double[] spurDistanz = Distanzen(spur);
        int ab = 0;
        foreach (Teilstueck teilstueck in teilstuecke.Where(t => !t.IstLuftlinie))
        {
            List<Geoposition> berechnet = [.. teilstueck.Punkte.Where(p => p.Position is not null).Select(p => p.Position!)];
            if (berechnet.Count < 2)
            {
                continue;
            }

            double[] berechnetDistanz = Distanzen(berechnet);
            int abBerechnet = 0;
            foreach (Abbiegepunkt abbiegepunkt in teilstueck.Abbiegepunkte.Where(a => a.Richtung != Abbiegerichtung.Wenden))
            {
                // Auch in der berechneten Strecke in Fahrtrichtung suchen, falls sie eine Stelle zweimal passiert
                if (Durchgang(berechnet, abBerechnet, abbiegepunkt.Position) is not int mitte)
                {
                    continue;
                }

                abBerechnet = mitte;
                if (Durchgang(spur, ab, abbiegepunkt.Position) is not int punkt)
                {
                    continue;
                }

                // Die Spur rund um den Punkt, etwas weiter als der Umkreis, damit Kurven mitzählen
                int von = punkt, bis = punkt;
                while (von > 0 && spurDistanz[punkt] - spurDistanz[von] < 2 * UmkreisMeter)
                {
                    von--;
                }

                while (bis < spur.Count - 1 && spurDistanz[bis] - spurDistanz[punkt] < 2 * UmkreisMeter)
                {
                    bis++;
                }

                List<Geoposition> umgebung = [.. spur.Skip(von).Take(bis - von + 1)];
                bool folgtDerSpur = Enumerable.Range(0, berechnet.Count)
                    .Where(i => Math.Abs(berechnetDistanz[i] - berechnetDistanz[mitte]) <= UmkreisMeter)
                    .All(i => Geoabstand.ZurLinie(berechnet[i], umgebung) <= AbweichungMeter);
                if (!folgtDerSpur || (abbiegungen.Count > 0 && abbiegungen[^1].Punkt == punkt))
                {
                    continue;
                }

                abbiegungen.Add(new Courseabbiegung(punkt, abbiegepunkt.Richtung, abbiegepunkt.Ausfahrt));
                ab = punkt;
            }
        }

        return abbiegungen;
    }

    /// <summary>
    /// Der Punkt einer Linie zu einer Position: ab <paramref name="ab"/> der erste Durchgang, der näher als
    /// <see cref="AbweichungMeter"/> kommt, und darin der nächste Punkt; <c>null</c>, wenn keiner so nahe kommt.
    /// </summary>
    private static int? Durchgang(IReadOnlyList<Geoposition> linie, int ab, Geoposition position)
    {
        int? bester = null;
        double kleinster = double.MaxValue;
        for (int i = ab; i < linie.Count; i++)
        {
            double abstand = Geoabstand.Meter(linie[i], position);
            if (abstand <= AbweichungMeter)
            {
                if (abstand < kleinster)
                {
                    kleinster = abstand;
                    bester = i;
                }
            }
            else if (bester is not null)
            {
                // Der Durchgang ist vorbei
                break;
            }
        }

        return bester;
    }

    /// <summary>Distanz jedes Punkts vom ersten entlang der Linie in Metern.</summary>
    private static double[] Distanzen(IReadOnlyList<Geoposition> linie)
    {
        double[] distanzen = new double[linie.Count];
        for (int i = 1; i < linie.Count; i++)
        {
            distanzen[i] = distanzen[i - 1] + Geoabstand.Meter(linie[i - 1], linie[i]);
        }

        return distanzen;
    }
}
