using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Findet das Teilstück einer Planung, das einer Stelle auf der Karte am nächsten liegt – dort entsteht beim Wegziehen
/// der Linie ein neuer Wegpunkt („Zwischenpunkt einfügen“).
/// </summary>
public static class FindeEinfuegestelle
{
    /// <summary>Meter je Grad geografischer Breite (Näherung, für Radtouren genau genug).</summary>
    private const double MeterJeGradBreite = 111_320;

    /// <summary>
    /// Liefert das nächstgelegene Teilstück und den Abstand der Stelle zu seiner Linie.
    /// </summary>
    /// <remarks>
    /// Ein noch nicht ermitteltes Teilstück zählt als gerade Linie zwischen seinen Wegpunkten, so wie es auch auf der
    /// Karte erscheint. Gerechnet wird in Metern auf einer örtlichen Ebene um die Stelle. Ob die Stelle nah genug an
    /// der Linie liegt, entscheidet der Aufrufer anhand des Abstands. Der neue Wegpunkt bekommt die Nummer
    /// <c>Teilstueck + 1</c> (siehe <see cref="AenderePlanung.Einfuegen"/>).
    /// </remarks>
    /// <param name="planung">Die Planung.</param>
    /// <param name="stelle">Die Stelle, an der die Linie gegriffen wurde.</param>
    /// <returns>Nummer des Teilstücks und Abstand in Metern oder <c>null</c>, wenn die Planung kein Teilstück hat.</returns>
    public static (int Teilstueck, double AbstandMeter)? Ausfuehren(Tourenplanung planung, Geoposition stelle)
    {
        (int Teilstueck, double AbstandMeter)? naechstes = null;
        for (int i = 0; i < planung.Teilstuecke.Count; i++)
        {
            List<Geoposition> linie = planung.Teilstuecke[i] is Teilstueck teilstueck
                ? [.. teilstueck.Punkte.Where(p => p.Position is not null).Select(p => p.Position!)]
                : [planung.Wegpunkte[i], planung.Wegpunkte[i + 1]];
            double abstand = AbstandZurLinie(stelle, linie);
            if (naechstes is null || abstand < naechstes.Value.AbstandMeter)
            {
                naechstes = (i, abstand);
            }
        }

        return naechstes;
    }

    /// <summary>Kleinster Abstand der Stelle zu einer Linie aus Punkten, in Metern.</summary>
    private static double AbstandZurLinie(Geoposition stelle, List<Geoposition> linie)
    {
        double meterJeGradLaenge = MeterJeGradBreite * Math.Cos(stelle.Breite * Math.PI / 180);
        (double X, double Y) Eben(Geoposition p) =>
            ((p.Laenge - stelle.Laenge) * meterJeGradLaenge, (p.Breite - stelle.Breite) * MeterJeGradBreite);

        if (linie.Count == 1)
        {
            (double x, double y) = Eben(linie[0]);
            return Math.Sqrt(x * x + y * y);
        }

        double kleinster = double.MaxValue;
        for (int i = 0; i + 1 < linie.Count; i++)
        {
            kleinster = Math.Min(kleinster, AbstandZumUrsprung(Eben(linie[i]), Eben(linie[i + 1])));
        }

        return kleinster;
    }

    /// <summary>Abstand des Ursprungs (der Stelle) von der Strecke zwischen zwei Punkten.</summary>
    private static double AbstandZumUrsprung((double X, double Y) a, (double X, double Y) b)
    {
        double dx = b.X - a.X, dy = b.Y - a.Y;
        double laengeQuadrat = dx * dx + dy * dy;
        double t = laengeQuadrat == 0 ? 0 : Math.Clamp(-(a.X * dx + a.Y * dy) / laengeQuadrat, 0, 1);
        double nx = a.X + t * dx, ny = a.Y + t * dy;
        return Math.Sqrt(nx * nx + ny * ny);
    }
}
