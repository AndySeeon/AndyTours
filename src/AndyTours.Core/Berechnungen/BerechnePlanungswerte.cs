using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Berechnet Länge und Höhenmeter einer geplanten Tour, wie das Planungsfenster sie nach jeder Änderung anzeigt.
/// </summary>
public static class BerechnePlanungswerte
{
    /// <summary>Mittlerer Erdradius in Kilometern.</summary>
    private const double ErdradiusKm = 6371.0;

    /// <summary>
    /// Höhenänderung in Metern, ab der ein Anstieg oder Abstieg zählt. Kleinere Schwankungen der Höhendaten
    /// (Rundung, Messraster) würden die Summe sonst aufblähen.
    /// </summary>
    private const double Schwelle = 3.0;

    /// <summary>
    /// Liefert Länge, Höhenmeter bergauf und bergab.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Länge: Summe der Großkreisabstände zwischen aufeinanderfolgenden Punkten aller Teilstücke. Ein noch nicht
    /// ermitteltes Teilstück zählt als gerade Linie zwischen seinen Wegpunkten, damit die Anzeige sofort stimmt.</item>
    /// <item>Höhenmeter: Über alle bekannten Höhen der Strecke in Fahrtrichtung wird eine Änderung erst gezählt, wenn sie
    /// seit dem letzten gezählten Punkt mindestens <see cref="Schwelle"/> Meter beträgt. Luftlinien und noch nicht
    /// ermittelte Teilstücke haben keine Höhen.</item>
    /// </list>
    /// </remarks>
    /// <param name="planung">Die Planung.</param>
    /// <returns>Die Werte; ohne Teilstück alles 0.</returns>
    public static Planungswerte Ausfuehren(Tourenplanung planung)
    {
        double laengeKm = 0;
        double bergauf = 0, bergab = 0;
        double? bezug = null;
        for (int i = 0; i < planung.Teilstuecke.Count; i++)
        {
            IReadOnlyList<Verlaufspunkt> punkte = planung.Teilstuecke[i]?.Punkte
                ?? [new Verlaufspunkt { Position = planung.Wegpunkte[i] }, new Verlaufspunkt { Position = planung.Wegpunkte[i + 1] }];
            Geoposition? vorher = null;
            foreach (Verlaufspunkt punkt in punkte)
            {
                if (punkt.Position is Geoposition position)
                {
                    if (vorher is not null)
                    {
                        laengeKm += AbstandKm(vorher, position);
                    }

                    vorher = position;
                }

                if (punkt.Hoehe is not double hoehe)
                {
                    continue;
                }

                if (bezug is null)
                {
                    bezug = hoehe;
                }
                else if (hoehe - bezug >= Schwelle)
                {
                    bergauf += hoehe - bezug.Value;
                    bezug = hoehe;
                }
                else if (bezug - hoehe >= Schwelle)
                {
                    bergab += bezug.Value - hoehe;
                    bezug = hoehe;
                }
            }
        }

        return new Planungswerte(laengeKm, (int)Math.Round(bergauf), (int)Math.Round(bergab));
    }

    /// <summary>Großkreisabstand zweier Positionen in Kilometern (Haversine-Formel).</summary>
    private static double AbstandKm(Geoposition a, Geoposition b)
    {
        double breiteA = a.Breite * Math.PI / 180, breiteB = b.Breite * Math.PI / 180;
        double dBreite = breiteB - breiteA;
        double dLaenge = (b.Laenge - a.Laenge) * Math.PI / 180;
        double h = Math.Sin(dBreite / 2) * Math.Sin(dBreite / 2)
            + Math.Cos(breiteA) * Math.Cos(breiteB) * Math.Sin(dLaenge / 2) * Math.Sin(dLaenge / 2);
        return 2 * ErdradiusKm * Math.Asin(Math.Min(1, Math.Sqrt(h)));
    }
}
