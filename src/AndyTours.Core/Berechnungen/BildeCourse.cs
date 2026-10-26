using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bildet einen <see cref="Course"/> aus einer Planung oder aus der Aufzeichnung einer Einheit (Spec 032).
/// </summary>
public static class BildeCourse
{
    /// <summary>Tempo des virtuellen Partners bei einer Planung in km/h.</summary>
    public const double PlanungstempoKmh = 15;

    /// <summary>
    /// Bildet den Course einer Planung.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Die Punkte aller Teilstücke werden aneinandergereiht; der erste Punkt eines Teilstücks entfällt, wenn er
    /// mit dem letzten des vorigen übereinstimmt. Ein noch nicht ermitteltes Teilstück zählt als gerade Linie.</item>
    /// <item>Die Zeiten folgen aus der Distanz bei <see cref="PlanungstempoKmh"/>.</item>
    /// <item>Jeder Abbiegepunkt wird dem nächstgelegenen Punkt seines Teilstücks zugeordnet.</item>
    /// </list>
    /// </remarks>
    /// <param name="planung">Die Planung mit mindestens zwei Wegpunkten.</param>
    /// <param name="name">Name des Course.</param>
    /// <returns>Der Course.</returns>
    public static Course AusPlanung(Tourenplanung planung, string name)
    {
        List<Geoposition> positionen = [];
        List<double?> hoehen = [];
        List<Courseabbiegung> abbiegungen = [];
        for (int i = 0; i < planung.Teilstuecke.Count; i++)
        {
            Teilstueck? teilstueck = planung.Teilstuecke[i];
            List<(Geoposition Position, double? Hoehe)> punkte = teilstueck is null
                ? [(planung.Wegpunkte[i], null), (planung.Wegpunkte[i + 1], null)]
                : [.. teilstueck.Punkte.Where(p => p.Position is not null).Select(p => (p.Position!, p.Hoehe))];
            if (punkte.Count > 0 && positionen.Count > 0 && punkte[0].Position == positionen[^1])
            {
                punkte.RemoveAt(0);
            }

            // Ein Abbiegepunkt am Anfang eines Teilstücks darf auch auf dessen gemeinsamem Punkt mit dem vorigen liegen
            int anfang = Math.Max(positionen.Count - 1, 0);
            positionen.AddRange(punkte.Select(p => p.Position));
            hoehen.AddRange(punkte.Select(p => p.Hoehe));
            foreach (Abbiegepunkt abbiegepunkt in teilstueck?.Abbiegepunkte ?? [])
            {
                abbiegungen.Add(new Courseabbiegung(Naechster(positionen, anfang, abbiegepunkt.Position), abbiegepunkt.Richtung, abbiegepunkt.Ausfahrt));
            }
        }

        List<Coursepunkt> coursepunkte = [];
        double distanzKm = 0;
        for (int i = 0; i < positionen.Count; i++)
        {
            if (i > 0)
            {
                distanzKm += Geoabstand.Meter(positionen[i - 1], positionen[i]) / 1000;
            }

            coursepunkte.Add(new Coursepunkt(positionen[i], hoehen[i], distanzKm, TimeSpan.FromHours(distanzKm / PlanungstempoKmh)));
        }

        return new Course(name, coursepunkte, abbiegungen);
    }

    /// <summary>
    /// Bildet den Course einer gefahrenen Strecke.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Die Linie besteht aus allen Punkten der Aufzeichnung mit Position, in ihrer Reihenfolge; Punkte ohne
    /// Position (z. B. im Tunnel) entfallen. Die Nummern der <paramref name="abbiegungen"/> beziehen sich auf genau
    /// diese Punkte (<see cref="Spur"/>).</item>
    /// <item>Die Zeiten sind die eigenen Fahrzeiten seit dem ersten Punkt, damit der virtuelle Partner so fährt wie
    /// damals. Steigen sie nicht an (z. B. bei einem umgekehrten Track), gilt wie bei einer Planung
    /// <see cref="PlanungstempoKmh"/>.</item>
    /// </list>
    /// </remarks>
    /// <param name="verlauf">Die Punkte der Aufzeichnung.</param>
    /// <param name="name">Name des Course.</param>
    /// <param name="abbiegungen">Die Abbiegungen (<see cref="UebernimmAbbiegungen"/>); leer, wenn keine bekannt sind.</param>
    /// <returns>Der Course.</returns>
    public static Course AusVerlauf(IReadOnlyList<Verlaufspunkt> verlauf, string name, IReadOnlyList<Courseabbiegung> abbiegungen)
    {
        List<Verlaufspunkt> punkte = [.. verlauf.Where(p => p.Position is not null)];
        bool eigeneZeiten = punkte.Count > 1 && punkte[^1].Fahrzeit > punkte[0].Fahrzeit;
        List<Coursepunkt> coursepunkte = [];
        double distanzKm = 0;
        TimeSpan zeit = TimeSpan.Zero;
        for (int i = 0; i < punkte.Count; i++)
        {
            if (i > 0)
            {
                distanzKm += Geoabstand.Meter(punkte[i - 1].Position!, punkte[i].Position!) / 1000;
            }

            // Die Zeit darf nie zurückgehen, auch wenn die Aufzeichnung an einer Stelle springt
            TimeSpan neu = eigeneZeiten ? punkte[i].Fahrzeit - punkte[0].Fahrzeit : TimeSpan.FromHours(distanzKm / PlanungstempoKmh);
            zeit = neu > zeit ? neu : zeit;
            coursepunkte.Add(new Coursepunkt(punkte[i].Position!, punkte[i].Hoehe, distanzKm, zeit));
        }

        return new Course(name, coursepunkte, abbiegungen);
    }

    /// <summary>
    /// Die Positionen einer Aufzeichnung, auf die sich die Abbiegungen in <see cref="AusVerlauf"/> beziehen.
    /// </summary>
    /// <param name="verlauf">Die Punkte der Aufzeichnung.</param>
    /// <returns>Die Positionen aller Punkte, die eine haben.</returns>
    public static IReadOnlyList<Geoposition> Spur(IReadOnlyList<Verlaufspunkt> verlauf) =>
        [.. verlauf.Where(p => p.Position is not null).Select(p => p.Position!)];

    /// <summary>Nummer des Punkts ab <paramref name="anfang"/>, der einer Position am nächsten liegt.</summary>
    private static int Naechster(List<Geoposition> positionen, int anfang, Geoposition ziel)
    {
        int bester = anfang;
        double kleinster = double.MaxValue;
        for (int i = anfang; i < positionen.Count; i++)
        {
            double abstand = Geoabstand.Meter(positionen[i], ziel);
            if (abstand < kleinster)
            {
                kleinster = abstand;
                bester = i;
            }
        }

        return bester;
    }
}
