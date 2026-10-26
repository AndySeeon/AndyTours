using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Abstände zwischen Positionen in Metern, für die Courses (Spec 032).
/// </summary>
/// <remarks>
/// Der Abstand zweier Punkte folgt dem Großkreis (Haversine-Formel). Für den Abstand eines Punkts zu einer Strecke
/// genügt eine ebene Näherung um den Punkt herum, weil es nur um wenige hundert Meter geht.
/// </remarks>
public static class Geoabstand
{
    /// <summary>Mittlerer Erdradius in Metern.</summary>
    private const double ErdradiusMeter = 6_371_000;

    /// <summary>
    /// Großkreisabstand zweier Positionen.
    /// </summary>
    /// <param name="a">Erste Position.</param>
    /// <param name="b">Zweite Position.</param>
    /// <returns>Der Abstand in Metern.</returns>
    public static double Meter(Geoposition a, Geoposition b)
    {
        double breiteA = Bogen(a.Breite), breiteB = Bogen(b.Breite);
        double dBreite = breiteB - breiteA;
        double dLaenge = Bogen(b.Laenge - a.Laenge);
        double h = Math.Sin(dBreite / 2) * Math.Sin(dBreite / 2)
            + Math.Cos(breiteA) * Math.Cos(breiteB) * Math.Sin(dLaenge / 2) * Math.Sin(dLaenge / 2);
        return 2 * ErdradiusMeter * Math.Asin(Math.Min(1, Math.Sqrt(h)));
    }

    /// <summary>
    /// Kürzester Abstand eines Punkts zur geraden Strecke zwischen zwei Positionen.
    /// </summary>
    /// <param name="punkt">Der Punkt.</param>
    /// <param name="von">Anfang der Strecke.</param>
    /// <param name="nach">Ende der Strecke.</param>
    /// <returns>Der Abstand in Metern.</returns>
    public static double ZurStrecke(Geoposition punkt, Geoposition von, Geoposition nach)
    {
        // Ebene Koordinaten in Metern mit dem Punkt als Ursprung
        double massstab = Math.Cos(Bogen(punkt.Breite));
        (double ax, double ay) = Eben(von);
        (double bx, double by) = Eben(nach);
        double dx = bx - ax, dy = by - ay;
        double laenge2 = dx * dx + dy * dy;
        double t = laenge2 == 0 ? 0 : Math.Clamp(-(ax * dx + ay * dy) / laenge2, 0, 1);
        double x = ax + t * dx, y = ay + t * dy;
        return Math.Sqrt(x * x + y * y);

        (double X, double Y) Eben(Geoposition p) =>
            (Bogen(p.Laenge - punkt.Laenge) * massstab * ErdradiusMeter, Bogen(p.Breite - punkt.Breite) * ErdradiusMeter);
    }

    /// <summary>
    /// Kürzester Abstand eines Punkts zu einer Linie aus mehreren Positionen.
    /// </summary>
    /// <param name="punkt">Der Punkt.</param>
    /// <param name="linie">Die Linie; mindestens eine Position.</param>
    /// <returns>Der Abstand in Metern.</returns>
    public static double ZurLinie(Geoposition punkt, IReadOnlyList<Geoposition> linie)
    {
        if (linie.Count == 1)
        {
            return Meter(punkt, linie[0]);
        }

        double kleinster = double.MaxValue;
        for (int i = 1; i < linie.Count; i++)
        {
            kleinster = Math.Min(kleinster, ZurStrecke(punkt, linie[i - 1], linie[i]));
        }

        return kleinster;
    }

    /// <summary>Grad in Bogenmaß.</summary>
    private static double Bogen(double grad) => grad * Math.PI / 180;
}
